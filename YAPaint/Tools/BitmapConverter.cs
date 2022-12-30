using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class BitmapConverter
{
    private static readonly Vector Dpi96 = new Vector(96, 96);

    public static unsafe WriteableBitmap ToAvalonia(this PortableBitmap portableBitmap)
    {
        var writeableBitmap = new WriteableBitmap(
            new PixelSize(portableBitmap.Width, portableBitmap.Height),
            Dpi96,
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using var bitmapLock = writeableBitmap.Lock();
        int* pointer = (int*)bitmapLock.Address.ToPointer();

        for (int j = 0; j < portableBitmap.Height; j++)
        {
            for (int i = 0; i < portableBitmap.Width; i++)
            {
                var color = portableBitmap.GetPixel(i, j);
                var rgbColor = portableBitmap.ColorConverter.ToRgb(ref color);

                // opposite left shifts because of reverse endianness
                pointer[j * portableBitmap.Width + i] = Coefficient.Denormalize(rgbColor.First)
                                                      + (Coefficient.Denormalize(rgbColor.Second) << 8)
                                                      + (Coefficient.Denormalize(rgbColor.Third) << 16)
                                                      + (byte.MaxValue << 24);
            }
        }

        MyFileLogger.Log("DBG", $"Converted to AvaloniaBitmap at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s");

        return writeableBitmap;
    }

    public static unsafe WriteableBitmap ToAvalonia(this System.Drawing.Bitmap bitmap)
    {
        var writeableBitmap = new WriteableBitmap(
            new PixelSize(bitmap.Width, bitmap.Height),
            Dpi96,
            PixelFormat.Rgba8888,
            AlphaFormat.Unpremul);

        using var bitmapLock = writeableBitmap.Lock();
        int* pointer = (int*)bitmapLock.Address.ToPointer();

        for (int j = 0; j < bitmap.Height; j++)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                var color = bitmap.GetPixel(i, j);

                // opposite left shifts because of reverse endianness
                pointer[j * bitmap.Width + i] = color.R + (color.G << 8) + (color.B << 16) + (byte.MaxValue << 24);
            }
        }

        MyFileLogger.Log("DBG", $"Converted to AvaloniaBitmap at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s");

        return writeableBitmap;
    }
}
