using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FileDialogFilter _fileFilter = new FileDialogFilter
        { Name = "Image", Extensions = { "jpg", "png", "pnm", "bmp" } };

    private string ImagePath { get; set; } = string.Empty;

    private AvaloniaBitmap _bitmapImage = LoadImage(@"..\..\..\Assets\IMG_3609.jpg");

    public AvaloniaBitmap BitmapImage
    {
        get => _bitmapImage;
        set => this.RaiseAndSetIfChanged(ref _bitmapImage, value);
    }

    public async Task Open()
    {
        var dialog = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter> { _fileFilter },
        };

        string[] result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            ImagePath = result[0];
        }

        BitmapImage = LoadImage(ImagePath);
    }

    public async Task Save()
    {
        var dialog = new SaveFileDialog { Filters = new List<FileDialogFilter> { _fileFilter } };
        string result = await dialog.ShowAsync(new Window());

        if (result is not null)
        {
            BitmapImage.Save(result);
        }
    }
}
