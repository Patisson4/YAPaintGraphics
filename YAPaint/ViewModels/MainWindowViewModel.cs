using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using YAPaint.Models.Parsers;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly List<FileDialogFilter> _fileFilters = new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "Image", Extensions = { "jpg", "bmp", "png", "pnm", "pbm", "pgm", "ppm" } },
    };

    private AvaloniaBitmap _bitmapImage = new Bitmap(@"..\..\..\Assets\LAX.jpg").ConvertToAvaloniaBitmap_MS();

    public AvaloniaBitmap BitmapImage
    {
        get => _bitmapImage;
        set => this.RaiseAndSetIfChanged(ref _bitmapImage, value);
    }

    public async Task Open()
    {
        var dialog = new OpenFileDialog { Filters = _fileFilters, AllowMultiple = false };
        string[] result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            BitmapImage = PnmParser.ReadImage(result[0]).ConvertToAvaloniaBitmap_LB();
        }
    }

    public async Task Save()
    {
        var dialog = new SaveFileDialog { Filters = _fileFilters };
        string result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            BitmapImage.Save(result);
        }
    }
}
