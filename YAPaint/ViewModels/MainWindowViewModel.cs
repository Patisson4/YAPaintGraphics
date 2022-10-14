﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using YAPaint.Models.Parsers;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _message = "Nothing here";

    private readonly List<FileDialogFilter> _fileFilters = new List<FileDialogFilter>
    {
        new FileDialogFilter { Name = "Image", Extensions = { "jpg", "bmp", "png", "pnm", "pbm", "pgm", "ppm" } },
    };

    private AvaloniaBitmap _bitmapImage = PnmParser.ReadImage(@"..\..\..\Assets\LAX.jpg").ConvertToAvaloniaBitmap();

    public AvaloniaBitmap BitmapImage
    {
        get => _bitmapImage;
        set => this.RaiseAndSetIfChanged(ref _bitmapImage, value);
    }

    public string Message
    {
        get => _message;
        set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    public async Task Open()
    {
        try
        {
            var dialog = new OpenFileDialog { Filters = _fileFilters, AllowMultiple = false };
            string[] result = await dialog.ShowAsync(new Window());

            if (result is not null)
            {
                BitmapImage = PnmParser.ReadImage(result[0]).ConvertToAvaloniaBitmap();
            }
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }

    public async Task Save()
    {
        try
        {
            var dialog = new SaveFileDialog { Filters = _fileFilters };
            string result = await dialog.ShowAsync(new Window());

            if (result is not null)
            {
                await BitmapImage.ConvertToSystemBitmap().WriteRawImage(result);
            }
        }
        catch (Exception e)
        {
            Message = e.ToString();
        }
    }
}
