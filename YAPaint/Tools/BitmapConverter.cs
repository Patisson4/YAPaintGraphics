using Avalonia;
using Avalonia.Media.Imaging;
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
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul);

        using var bitmapLock = writeableBitmap.Lock();

        for (int j = 0; j < portableBitmap.Height; j++)
        {
            for (int i = 0; i < portableBitmap.Width; i++)
            {
                var color = portableBitmap.GetPixel(i, j);
                var rgbColor = portableBitmap.ColorConverter.ToRgb(ref color);
                var rawColor = rgbColor.ToRaw();
                var pointer = (byte*)bitmapLock.Address.ToPointer();

                pointer[(j * portableBitmap.Width + i) * 4] = rawColor[2];
                pointer[(j * portableBitmap.Width + i) * 4 + 1] = rawColor[1];
                pointer[(j * portableBitmap.Width + i) * 4 + 2] = rawColor[0];
                pointer[(j * portableBitmap.Width + i) * 4 + 3] = byte.MaxValue;
            }
        }

        MyFileLogger.Log("DBG", $"Converted to AvaloniaBitmap at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s");

        return writeableBitmap;
    }
}
