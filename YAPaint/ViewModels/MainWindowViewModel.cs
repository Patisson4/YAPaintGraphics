using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Logging;
using ReactiveUI.Fody.Helpers;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly List<FileDialogFilter> FileFilters = new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "Portable Bitmaps", Extensions = { "pnm", "pbm", "pgm", "ppm" } },
        new FileDialogFilter { Name = "All", Extensions = { "*" } },
    };

    private static readonly List<Type> SpaceTypes = Assembly.GetExecutingAssembly()
                                                            .GetTypes()
                                                            .Where(t => t.GetInterfaces().Contains(typeof(IColorSpace)))
                                                            .ToList();

    private readonly Stopwatch _timer = new Stopwatch();

    private int _operationsCount;

    private PortableBitmap _portableBitmap;

    [Reactive]
    public string Message { get; set; } = "Timings will be displayed here";

    [Reactive]
    public string SelectedColorSpace { get; set; } = nameof(Rgb);

    [Reactive]
    public AvaloniaBitmap AvaloniaImage { get; set; }

    public static IReadOnlyCollection<string> ThreeChannelColorSpaceNames { get; } = SpaceTypes
        .Where(t => t.GetInterfaces().Contains(typeof(IThreeChannelColorSpace)))
        .Select(t => t.Name)
        .ToList();

    public static IReadOnlyCollection<string> ColorSpaceNames { get; } = SpaceTypes.Select(t => t.Name).ToList();

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
        var dialog = new SaveFileDialog { Filters = FileFilters };
        string result = await dialog.ShowAsync(new Window()); // TODO: find real parent

        if (result is not null)
        {
            _timer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.SaveRaw(stream);

            _timer.Stop();
            _operationsCount++;
            Message = $"[{_operationsCount}] Saved in {_timer.Elapsed}";
        }
    }

    public async Task SavePlain()
    {
        var dialog = new SaveFileDialog { Filters = FileFilters };
        string result = await dialog.ShowAsync(new Window()); // TODO: find real parent

        if (result is not null)
        {
            _timer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.SavePlain(stream);

            _timer.Stop();
            _operationsCount++;
            Message = $"[{_operationsCount}] Saved in {_timer.Elapsed}";
        }
    }

    public void ToggleFirstChannel()
    {
        _timer.Restart();

        _portableBitmap.ToggleFirstChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        _timer.Stop();
        _operationsCount++;
        Message = $"[{_operationsCount}] Toggled in {_timer.Elapsed}";
    }

    public void ToggleSecondChannel()
    {
        _timer.Restart();

        _portableBitmap.ToggleSecondChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        _timer.Stop();
        _operationsCount++;
        Message = $"[{_operationsCount}] Toggled in {_timer.Elapsed}";
    }

    public void ToggleThirdChannel()
    {
        _timer.Restart();

        _portableBitmap.ToggleThirdChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        _timer.Stop();
        _operationsCount++;
        Message = $"[{_operationsCount}] Toggled in {_timer.Elapsed}";
    }

    private async Task OpenAs<TColorSpace>() where TColorSpace : IColorSpace
    {
        var dialog = new OpenFileDialog { Filters = FileFilters, AllowMultiple = false };
        string[] result = await dialog.ShowAsync(new Window()); // TODO: find real parent

        if (result is not null)
        {
            _timer.Restart();

            await using var stream = new FileStream(result[0], FileMode.Open);
            _portableBitmap = PnmParser.ReadImage<TColorSpace>(stream);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            _timer.Stop();
            _operationsCount++;
            Message = $"[{_operationsCount}] Opened in {_timer.Elapsed}";
        }
    }
}
