using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;

namespace YAPaint.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _imagePath = string.Empty;

        public string ImagePath
        {
            get => _imagePath;
            set => this.RaiseAndSetIfChanged(ref _imagePath, value);
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
        }
    }
}
