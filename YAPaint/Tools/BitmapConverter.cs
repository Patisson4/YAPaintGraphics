using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.Tools;

public static class BitmapConverter
{
    public static AvaloniaBitmap ToAvalonia(this PortableBitmap bitmap)
    {
        var systemBitmap = new Bitmap(bitmap.Width, bitmap.Height);
        for (int j = 0; j < bitmap.Height; j++)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                var color = bitmap.GetPixel(i, j);
                
                var dest = color.GetType().Name switch
                {
                    nameof(Rgb) => Rgb.ToSystemColor((Rgb)color),
                    nameof(GreyScale) => GreyScale.ToSystemColor((GreyScale)color),
                    nameof(BlackAndWhite) => BlackAndWhite.ToSystemColor((BlackAndWhite)color),
                    _ => throw new ArgumentException("Unsupported color space"),
                };
                
                systemBitmap.SetPixel(i, j, dest);
            }
        }

        return systemBitmap.ConvertToAvaloniaBitmap();
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
