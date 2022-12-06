using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;
using YAPaint.Models.ExtraColorSpaces;
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
                                                            .Where(
                                                                t => t.GetInterfaces()
                                                                      .Contains(typeof(IColorBaseConverter))
                                                                  && t.IsClass)
                                                            .ToList();

    private static readonly List<IColorBaseConverter> ColorSpaces = SpaceTypes
                                                                    .Select(
                                                                        t => t.GetProperty("Instance")
                                                                              ?.GetValue(null))
                                                                    .Cast<IColorBaseConverter>()
                                                                    .ToList();

    private readonly Stopwatch _timer = new Stopwatch();
    private int _operationsCount;

    private PortableBitmap _portableBitmap;
    private string _selectedColorSpace = nameof(Rgb);

    private IColorBaseConverter CurrentColorConverter =>
        ColorSpaces.First(s => s.GetType().Name == _selectedColorSpace);

    private bool _isFirstChannelVisible = true;
    private bool _isSecondChannelVisible = true;
    private bool _isThirdChannelVisible = true;

    [Reactive]
    public string Message { get; set; } = "Timings will be displayed here";

    public string SelectedColorSpace
    {
        get => _selectedColorSpace;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedColorSpace, value);
            if (_portableBitmap is null)
            {
                return;
            }

            MyFileLogger.SharedTimer.Restart();

            _portableBitmap.ConvertTo(CurrentColorConverter);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Changed ColorSpace in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("INF", $"{Message}\n");
        }
    }

    [Reactive]
    public AvaloniaBitmap AvaloniaImage { get; set; }

    public static IReadOnlyCollection<string> ThreeChannelColorSpaceNames { get; } = SpaceTypes
        .Where(t => t.GetInterfaces().Contains(typeof(IColorConverter)))
        .Select(t => t.Name)
        .ToList();

    public static IReadOnlyCollection<string> ColorSpaceNames { get; } = SpaceTypes.Select(t => t.Name).ToList();

    public async Task Open()
    {
        try
        {
            var dialog = new OpenFileDialog { Filters = FileFilters, AllowMultiple = false };
            string[] result = await dialog.ShowAsync(new Window()); // TODO: find real parent

            if (result is null)
            {
                return;
            }

            MyFileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result[0], FileMode.Open);

            MyFileLogger.Log("DBG", $"Stream created at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s");

            _portableBitmap = new PortableBitmap(
                PnmParser.ReadImage(stream),
                CurrentColorConverter,
                _isFirstChannelVisible,
                _isSecondChannelVisible,
                _isThirdChannelVisible);

            AvaloniaImage = _portableBitmap.ToAvalonia();

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Opened in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            MyFileLogger.Log("ERR", $"{e}\n");
        }
    }

    public async Task SaveRaw()
    {
        try
        {
            var dialog = new SaveFileDialog { Filters = FileFilters };
            string result = await dialog.ShowAsync(new Window()); // TODO: find real parent

            if (result is null)
            {
                return;
            }

            MyFileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.ConvertTo(CurrentColorConverter);
            _portableBitmap.SaveRaw(stream);

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            MyFileLogger.Log("ERR", $"{e}\n");
        }
    }

    public async Task SavePlain()
    {
        try
        {
            var dialog = new SaveFileDialog { Filters = FileFilters };
            string result = await dialog.ShowAsync(new Window()); // TODO: find real parent

            if (result is null)
            {
                return;
            }

            MyFileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.ConvertTo(CurrentColorConverter);
            _portableBitmap.SavePlain(stream);

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            MyFileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ToggleFirstChannel()
    {
        _isFirstChannelVisible = !_isFirstChannelVisible;
        if (_portableBitmap is null)
        {
            return;
        }

        MyFileLogger.SharedTimer.Restart();

        _portableBitmap.ToggleFirstChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        MyFileLogger.SharedTimer.Stop();
        _operationsCount++;
        Message = $"({_operationsCount}) Toggled in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
        MyFileLogger.Log("INF", $"{Message}\n");
    }

    public void ToggleSecondChannel()
    {
        _isSecondChannelVisible = !_isSecondChannelVisible;
        if (_portableBitmap is null)
        {
            return;
        }

        MyFileLogger.SharedTimer.Restart();

        _portableBitmap.ToggleSecondChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        MyFileLogger.SharedTimer.Stop();
        _operationsCount++;
        Message = $"({_operationsCount}) Toggled in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
        MyFileLogger.Log("INF", $"{Message}\n");
    }

    public void ToggleThirdChannel()
    {
        _isThirdChannelVisible = !_isThirdChannelVisible;
        if (_portableBitmap is null)
        {
            return;
        }

        MyFileLogger.SharedTimer.Restart();

        _portableBitmap.ToggleThirdChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        MyFileLogger.SharedTimer.Stop();
        _operationsCount++;
        Message = $"[{_operationsCount}] Toggled in {_timer.Elapsed}";
        MyFileLogger.Log("INF", $"{Message}\n");
    }
}
