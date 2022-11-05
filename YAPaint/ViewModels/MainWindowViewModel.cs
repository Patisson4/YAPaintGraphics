using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _message = "Nothing here";

    private readonly List<FileDialogFilter> _fileFilters = new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "Portable Bitmaps", Extensions = { "pnm", "pbm", "pgm", "ppm" } },
        new FileDialogFilter { Name = "All", Extensions = { "*" } },
    };

    private readonly List<string> _spaces = Assembly.GetExecutingAssembly()
                                                    .GetTypes()
                                                    .Where(t => t.GetInterfaces().Contains(typeof(IColorSpace)))
                                                    .Select(t => t.Name)
                                                    .ToList();

    public IReadOnlyList<string> ColorSpaces => _spaces;

    [Reactive]
    public string Message { get; set; } = "Timings will be displayed here";

    [Reactive]
    public string SelectedColorSpace { get; set; } = nameof(Rgb);

    [Reactive]
    public AvaloniaBitmap BitmapImage { get; set; }

    public async Task Open()
    {
        try
        {
            await (SelectedColorSpace switch
            {
                nameof(Rgb) => OpenAs<Rgb>(),
                nameof(GreyScale) => OpenAs<GreyScale>(),
                nameof(BlackAndWhite) => OpenAs<BlackAndWhite>(),
                _ => throw new ArgumentException("Unsupported color space"),
            });
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }

    public async Task SaveRaw()
    {
        try
        {
            await (SelectedColorSpace switch
            {
                nameof(Rgb) => SaveRawAs<Rgb>(),
                nameof(GreyScale) => SaveRawAs<GreyScale>(),
                nameof(BlackAndWhite) => SaveRawAs<BlackAndWhite>(),
                _ => throw new ArgumentException("Unsupported color space"),
            });
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }

    public async Task SavePlain()
    {
        try
        {
            await (SelectedColorSpace switch
            {
                nameof(Rgb) => SavePlainAs<Rgb>(),
                nameof(GreyScale) => SavePlainAs<GreyScale>(),
                nameof(BlackAndWhite) => SavePlainAs<BlackAndWhite>(),
                _ => throw new ArgumentException("Unsupported color space"),
            });
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }

    private async Task OpenAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new OpenFileDialog { Filters = _fileFilters, AllowMultiple = false };
        string[] result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            await using var stream = new FileStream(result[0], FileMode.Open);
            BitmapImage = PnmParser.ReadImage<TColorSpace>(stream).ToAvalonia();
        }
    }

    private async Task SaveRawAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new SaveFileDialog { Filters = _fileFilters };
        string result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            await using var stream = new FileStream(result, FileMode.Create);
            BitmapImage.ToPortable<TColorSpace>().SaveRaw(stream);
        }
    }

    private async Task SavePlainAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new SaveFileDialog { Filters = _fileFilters };
        string result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            await using var stream = new FileStream(result, FileMode.Create);
            BitmapImage.ToPortable<TColorSpace>().SavePlain(stream);
        }
    }
}
