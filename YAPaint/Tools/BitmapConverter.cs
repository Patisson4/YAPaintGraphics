using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using YAPaint.Models;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

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
                map[i, j] = systemBitmap.GetPixel(i, j);
            }
        }

        return new PortableBitmap(map);
    }

    public static AvaloniaBitmap ToAvalonia(this PortableBitmap bitmap)
    {
        var systemBitmap = new Bitmap(bitmap.Width, bitmap.Height);
        for (int j = 0; j < bitmap.Height; j++)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                systemBitmap.SetPixel(i, j, bitmap.GetPixel(i, j));
            }
        }

        return systemBitmap.ConvertToAvaloniaBitmap();
    }

    private static Bitmap ConvertToSystemBitmap(this AvaloniaBitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream);
        stream.Position = 0;
        return new Bitmap(stream);
    }

    private static AvaloniaBitmap ConvertToAvaloniaBitmap(this Bitmap bitmap)
    {
        var bitmapTmp = new Bitmap(bitmap);
        var bitmapData = bitmapTmp.LockBits(
            new Rectangle(0, 0, bitmapTmp.Width, bitmapTmp.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);
        var avaloniaBitmap = new AvaloniaBitmap(
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul,
            bitmapData.Scan0,
            new Avalonia.PixelSize(bitmapData.Width, bitmapData.Height),
            new Avalonia.Vector(96, 96),
            bitmapData.Stride);
        bitmapTmp.UnlockBits(bitmapData);
        bitmapTmp.Dispose();
        return avaloniaBitmap;
    }
}
