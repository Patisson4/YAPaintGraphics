using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using ReactiveUI.Fody.Helpers;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly Stopwatch _timer = new Stopwatch();

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

    [Reactive]
    public float Gamma { get; set; } = 2.0f;
    
    public CultureInfo InvariantCultureInfo { get; } = CultureInfo.InvariantCulture;

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
            Logger.Sink?.Log(LogEventLevel.Error, "All", e, e.ToString());
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
            Logger.Sink?.Log(LogEventLevel.Error, "All", e, e.ToString());
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
            Logger.Sink?.Log(LogEventLevel.Error, "All", e, e.ToString());
        }
    }

    public void ApplyGamma()
    {
        //TODO: change initial call to internal field
        BitmapImage = BitmapImage.ToPortable<Rgb>().ApplyGamma(Gamma).ToAvalonia();
    }
    
    public void ConvertToGamma()
    {
        //TODO: save result in internal field instead of assignment
        var result = BitmapImage.ToPortable<Rgb>().ApplyGamma(1 / Gamma);
        BitmapImage = result.ApplyGamma(Gamma).ToAvalonia();
    }

    private async Task OpenAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new OpenFileDialog { Filters = _fileFilters, AllowMultiple = false };
        string[] result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            _timer.Restart();

            await using var stream = new FileStream(result[0], FileMode.Open);
            BitmapImage = PnmParser.ReadImage<TColorSpace>(stream).ToAvalonia();

            _timer.Stop();
            Message = $"Opened in {_timer.Elapsed}";
        }
    }

    private async Task SaveRawAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new SaveFileDialog { Filters = _fileFilters };
        string result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            _timer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            BitmapImage.ToPortable<TColorSpace>().SaveRaw(stream);

            _timer.Stop();
            Message = $"Saved in {_timer.Elapsed}";
        }
    }

    private async Task SavePlainAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new SaveFileDialog { Filters = _fileFilters };
        string result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            _timer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            BitmapImage.ToPortable<TColorSpace>().SavePlain(stream);

            _timer.Stop();
            Message = $"Saved in {_timer.Elapsed}";
        }
    }
}
