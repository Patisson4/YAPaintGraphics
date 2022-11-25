using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;
using YAPaint.Models;
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

    private int _operationsCount;

    private PortableBitmap _portableBitmap;

    [Reactive]
    public string Message { get; set; } = "Timings will be displayed here";

    [Reactive]
    public string SelectedColorSpace { get; set; } = "RGB";

    [Reactive]
    public AvaloniaBitmap AvaloniaImage { get; set; }

    public static IReadOnlyCollection<string> ThreeChannelColorSpaceNames { get; } = new[] { "RGB" };

    public static IReadOnlyCollection<string> ColorSpaceNames { get; } = new[] { "RGB" };

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

            MyFileLogger.Log("DBG", $"Stream created at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s\n");

            _portableBitmap = PortableBitmap.FromStream(stream);

            var map = _portableBitmap.ToAvalonia();

            AvaloniaImage = map;

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Opened in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("DBG", $"{Message}\n\n");
        }
        catch (Exception e)
        {
            MyFileLogger.Log("ERR", e.ToString());
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
            _portableBitmap.SaveRaw(stream);

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("DBG", $"{Message}\n\n");
        }
        catch (Exception e)
        {
            MyFileLogger.Log("ERR", e.ToString());
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
            _portableBitmap.SavePlain(stream);

            MyFileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            MyFileLogger.Log("DBG", $"{Message}\n\n");
        }
        catch (Exception e)
        {
            MyFileLogger.Log("ERR", e.ToString());
        }
    }

    public void ToggleFirstChannel()
    {
        MyFileLogger.SharedTimer.Restart();

        _portableBitmap.ToggleFirstChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        MyFileLogger.SharedTimer.Stop();
        _operationsCount++;
        Message = $"({_operationsCount}) Toggled in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
        MyFileLogger.Log("DBG", $"{Message}\n\n");
    }

    public void ToggleSecondChannel()
    {
        MyFileLogger.SharedTimer.Restart();

        _portableBitmap.ToggleSecondChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        MyFileLogger.SharedTimer.Stop();
        _operationsCount++;
        Message = $"({_operationsCount}) Toggled in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
        MyFileLogger.Log("DBG", $"{Message}\n\n");
    }

    public void ToggleThirdChannel()
    {
        MyFileLogger.SharedTimer.Restart();

        _portableBitmap.ToggleThirdChannel();
        AvaloniaImage = _portableBitmap.ToAvalonia();

        MyFileLogger.SharedTimer.Stop();
        _operationsCount++;
        Message = $"({_operationsCount}) Toggled in {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s";
        MyFileLogger.Log("DBG", $"{Message}\n\n");
    }
}
