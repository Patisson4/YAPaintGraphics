using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using YAPaint.Models;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;
using Bitmap = System.Drawing.Bitmap;

namespace YAPaint.Tools;

public static class BitmapConverter
{
    public static PortableBitmap ToPortable(this AvaloniaBitmap bitmap)
    {
        Bitmap systemBitmap = bitmap.ConvertToSystemBitmap();
        var map = new ColorSpace[systemBitmap.Width, systemBitmap.Height];

        for (int j = 0; j < bitmap.PixelSize.Height; j++)
        {
            for (int i = 0; i < bitmap.PixelSize.Width; i++)
            {
                var pixel = systemBitmap.GetPixel(i, j);
                map[i, j] = new ColorSpace(
                    Coefficient.Normalize(pixel.R),
                    Coefficient.Normalize(pixel.G),
                    Coefficient.Normalize(pixel.B));
            }
        }

        return new PortableBitmap(map);
    }

    public static unsafe AvaloniaBitmap ToAvalonia(this PortableBitmap bitmap)
    {
        using var systemBitmap = new Bitmap(bitmap.Width, bitmap.Height);

        MyFileLogger.Log("DBG", $"System Bitmap created at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s\n");

        var bitmapData = systemBitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);

        var arr = (byte*)bitmapData.Scan0.ToPointer();

        for (int j = 0; j < bitmap.Height; j++)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                var rawColor = bitmap.GetPixel(i, j).ToRaw();
                arr[(j * bitmap.Width + i) * 4] = rawColor[2];
                arr[(j * bitmap.Width + i) * 4 + 1] = rawColor[1];
                arr[(j * bitmap.Width + i) * 4 + 2] = rawColor[0];
                arr[(j * bitmap.Width + i) * 4 + 3] = byte.MaxValue;
            }
        }

        MyFileLogger.Log("DBG", $"Pixels assigned at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s\n");

        var avaloniaBitmap = new AvaloniaBitmap(
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul,
            bitmapData.Scan0,
            new Avalonia.PixelSize(bitmapData.Width, bitmapData.Height),
            new Avalonia.Vector(96, 96),
            bitmapData.Stride);

        systemBitmap.UnlockBits(bitmapData);

        MyFileLogger.Log("DBG", $"Converted to AvaloniaBitmap at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s\n");

        return avaloniaBitmap;
    }

    private static Bitmap ConvertToSystemBitmap(this AvaloniaBitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        stream.Position = 0;
        return new Bitmap(stream);
    }
}
