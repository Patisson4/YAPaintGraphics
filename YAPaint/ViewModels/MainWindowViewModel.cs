using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ScottPlot;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;
using YAPaint.Models.ExtraColorSpaces;
using YAPaint.Tools;
using YAPaint.Views;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private static readonly List<FileDialogFilter> PnmFileFilters = new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "Portable Bitmap", Extensions = { "pnm", "pbm", "pgm", "ppm" } },
        new FileDialogFilter { Name = "All", Extensions = { "*" } },
    };

    private static readonly List<FileDialogFilter> PngFileFilters = new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "Portable Network Graphics", Extensions = { "png" } },
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
                                                                    .OfType<IColorBaseConverter>()
                                                                    .ToList();

    private int _operationsCount;

    private PortableBitmap _portableBitmap;
    private string _selectedColorSpace = nameof(Rgb);

    private IColorBaseConverter CurrentColorConverter =>
        ColorSpaces.First(s => s.GetType().Name == _selectedColorSpace);

    //TODO: read actual value from _portableBitmap
    private bool _isFirstChannelVisible = true;
    private bool _isSecondChannelVisible = true;
    private bool _isThirdChannelVisible = true;

    private Plot Plot { get; } = new Plot();

    private readonly MainWindow _view;

    public MainWindowViewModel(MainWindow view)
    {
        _view = view;
        IsColorConverter = CurrentColorConverter is IColorConverter;
        FirstChannelName = (CurrentColorConverter as IColorConverter)?.FirstChannelName;
        SecondChannelName = (CurrentColorConverter as IColorConverter)?.SecondChannelName;
        ThirdChannelName = (CurrentColorConverter as IColorConverter)?.ThirdChannelName;
    }

    public static IReadOnlyCollection<string> ThreeChannelColorSpaceNames { get; } = SpaceTypes
        .Where(t => t.GetInterfaces().Contains(typeof(IColorConverter)))
        .Select(t => t.Name)
        .ToList();

    public static IReadOnlyCollection<string> ColorSpaceNames { get; } = SpaceTypes.Select(t => t.Name).ToList();

    public string SelectedColorSpace
    {
        get => _selectedColorSpace;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedColorSpace, value);

            IsColorConverter = CurrentColorConverter is IColorConverter;
            FirstChannelName = (CurrentColorConverter as IColorConverter)?.FirstChannelName;
            SecondChannelName = (CurrentColorConverter as IColorConverter)?.SecondChannelName;
            ThirdChannelName = (CurrentColorConverter as IColorConverter)?.ThirdChannelName;

            if (_portableBitmap is null)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            _portableBitmap.ConvertTo(CurrentColorConverter);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Changed ColorSpace in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
    }

    [Reactive]
    public bool IsColorConverter { get; private set; }

    [Reactive]
    public string FirstChannelName { get; private set; }

    [Reactive]
    public string SecondChannelName { get; private set; }

    [Reactive]
    public string ThirdChannelName { get; private set; }

    [Reactive]
    public string Message { get; set; } = "Timings will be displayed here";

    [Reactive]
    public WriteableBitmap AvaloniaImage { get; set; }

    [Reactive]
    public float Gamma { get; set; } = 1;

    [Reactive]
    public float NewWidth { get; set; }

    [Reactive]
    public float NewHeight { get; set; }

    [Reactive]
    public float FocalPointX { get; set; }

    [Reactive]
    public float FocalPointY { get; set; }

    [Reactive]
    public float B { get; set; }

    [Reactive]
    public float C { get; set; }

    [Reactive]
    public float IntensityThreshold { get; set; }

    [Reactive]
    public int FilterThreshold { get; set; } = 100;

    [Reactive]
    public int KernelRadius { get; set; } = 2;

    [Reactive]
    public int Sigma { get; set; } = 1;

    [Reactive]
    public float Sharpness { get; set; } = 1;

    [Reactive]
    public bool IsHistogramVisible { get; private set; }

    [Reactive]
    public WriteableBitmap Histogram1 { get; set; }

    [Reactive]
    public WriteableBitmap Histogram2 { get; set; }

    [Reactive]
    public WriteableBitmap Histogram3 { get; set; }

    public CultureInfo InvariantCultureInfo { get; } = CultureInfo.InvariantCulture;

    public async Task OpenPnm()
    {
        try
        {
            var dialog = new OpenFileDialog { Filters = PnmFileFilters, AllowMultiple = false };
            string[] result = await dialog.ShowAsync(_view);

            if (result is null)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result[0], FileMode.Open);

            FileLogger.Log("DBG", $"Stream created at {FileLogger.SharedTimer.Elapsed.TotalSeconds} s");

            _portableBitmap = PnmParser.ReadImage(stream, CurrentColorConverter);
            _portableBitmap.ChangeFirstChannelVisibility(_isFirstChannelVisible);
            _portableBitmap.ChangeSecondChannelVisibility(_isSecondChannelVisible);
            _portableBitmap.ChangeThirdChannelVisibility(_isThirdChannelVisible);

            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Opened in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public async Task OpenPng()
    {
        try
        {
            var dialog = new OpenFileDialog { Filters = PngFileFilters, AllowMultiple = false };
            string[] result = await dialog.ShowAsync(_view);

            if (result is null)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result[0], FileMode.Open);

            FileLogger.Log("DBG", $"Stream created at {FileLogger.SharedTimer.Elapsed.TotalSeconds} s");

            _portableBitmap = PngConverter.ReadPng(stream);
            _portableBitmap.ChangeFirstChannelVisibility(_isFirstChannelVisible);
            _portableBitmap.ChangeSecondChannelVisibility(_isSecondChannelVisible);
            _portableBitmap.ChangeThirdChannelVisibility(_isThirdChannelVisible);

            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Opened in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public async Task SaveRaw()
    {
        try
        {
            var dialog = new SaveFileDialog { Filters = PnmFileFilters };
            string result = await dialog.ShowAsync(_view);

            if (result is null)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.SaveRaw(stream);

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public async Task SavePlain()
    {
        try
        {
            var dialog = new SaveFileDialog { Filters = PnmFileFilters };
            string result = await dialog.ShowAsync(_view);

            if (result is null)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.SavePlain(stream);

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public async Task SavePng()
    {
        try
        {
            var dialog = new SaveFileDialog { Filters = PngFileFilters };
            string result = await dialog.ShowAsync(_view);

            if (result is null)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            await using var stream = new FileStream(result, FileMode.Create);
            _portableBitmap.WritePng(stream);

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Saved in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ApplyGamma()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            AvaloniaImage = _portableBitmap.ApplyGamma(Gamma).ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Applied in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ConvertToGamma()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.ApplyGamma(Gamma);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Converted in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ToggleFirstChannel()
    {
        try
        {
            _isFirstChannelVisible = !_isFirstChannelVisible;
            if (_portableBitmap is null)
            {
                return;
            }

            if (_portableBitmap.IsFirstChannelVisible == _isFirstChannelVisible)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            _portableBitmap.ChangeFirstChannelVisibility(_isFirstChannelVisible);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Toggled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ToggleSecondChannel()
    {
        try
        {
            _isSecondChannelVisible = !_isSecondChannelVisible;
            if (_portableBitmap is null)
            {
                return;
            }

            if (_portableBitmap.IsSecondChannelVisible == _isSecondChannelVisible)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            _portableBitmap.ChangeSecondChannelVisibility(_isSecondChannelVisible);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Toggled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ToggleThirdChannel()
    {
        try
        {
            _isThirdChannelVisible = !_isThirdChannelVisible;
            if (_portableBitmap is null)
            {
                return;
            }

            if (_portableBitmap.IsThirdChannelVisible == _isThirdChannelVisible)
            {
                return;
            }

            FileLogger.SharedTimer.Restart();

            _portableBitmap.ChangeThirdChannelVisibility(_isThirdChannelVisible);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Toggled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ScaleNearestNeighbor()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.ScaleNearestNeighbor(NewWidth, NewHeight, FocalPointX, FocalPointY);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Scaled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ScaleBilinear()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.ScaleBilinear(NewWidth, NewHeight, FocalPointX, FocalPointY);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Scaled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ScaleLanczos3()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.ScaleLanczos3(NewWidth, NewHeight, FocalPointX, FocalPointY);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Scaled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ScaleBcSpline()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.ScaleBcSpline(NewWidth, NewHeight, FocalPointX, FocalPointY);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Scaled in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void DrawHistograms()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            double[][] histograms = HistogramGenerator.CreateHistograms(_portableBitmap);

            Plot.Clear();
            Plot.AddBar(histograms[0]);
            Histogram1 = Plot.Render().ToAvalonia();

            Plot.Clear();
            Plot.AddBar(histograms[1]);
            Histogram2 = Plot.Render().ToAvalonia();

            Plot.Clear();
            Plot.AddBar(histograms[2]);
            Histogram3 = Plot.Render().ToAvalonia();

            IsHistogramVisible = true;

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Generated in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void CorrectIntensity()
    {
        try
        {
            if (IntensityThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(IntensityThreshold),
                    IntensityThreshold,
                    $"{nameof(IntensityThreshold)} should be non-negative");
            }

            FileLogger.SharedTimer.Restart();

            IntensityCorrector.CorrectIntensity(ref _portableBitmap, IntensityThreshold);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Corrected in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void ThresholdFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            AvaloniaImage = _portableBitmap.ThresholdFilter(FilterThreshold).ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void OtsuFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            AvaloniaImage = _portableBitmap.OtsuFilter().ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void MedianFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.MedianFilter(KernelRadius);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void GaussianFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.GaussianFilter(Sigma);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void BoxBlurFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.BoxBlurFilter(KernelRadius);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void SobelFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            AvaloniaImage = _portableBitmap.SobelFilter().ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }

    public void SharpFilter()
    {
        try
        {
            FileLogger.SharedTimer.Restart();

            _portableBitmap = _portableBitmap.ContrastAdaptiveSharpening(Sharpness);
            AvaloniaImage = _portableBitmap.ToAvalonia();

            FileLogger.SharedTimer.Stop();
            _operationsCount++;
            Message = $"({_operationsCount}) Filtered in {FileLogger.SharedTimer.Elapsed.TotalSeconds} s";
            FileLogger.Log("INF", $"{Message}\n");
        }
        catch (Exception e)
        {
            FileLogger.Log("ERR", $"{e}\n");
        }
    }
}
