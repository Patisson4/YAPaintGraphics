using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private AvaloniaBitmap _bitmapImage = LoadImage(@"..\..\..\Assets\IMG_3609.jpg");

        public string ImagePath { get; set; } = string.Empty;

        public AvaloniaBitmap BitmapImage
        {
            get => _bitmapImage;
            set => this.RaiseAndSetIfChanged(ref _bitmapImage, value);
        }

        public async Task Open()
        {
            var dialog = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                    { new FileDialogFilter { Name = "Image", Extensions = { "jpg", "png", "pnm", "bmp" } } },
            };

            string[] result = await dialog.ShowAsync(new Window());

            if (result is not null)
            {
                ImagePath = result[0];
            }

            BitmapImage = LoadImage(ImagePath);
        }

        public static AvaloniaBitmap LoadImage(string imagePath)
        {
            var bitmap = new Bitmap(imagePath);
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);
            stream.Position = 0;
            return new AvaloniaBitmap(stream);
        }
    }
}
