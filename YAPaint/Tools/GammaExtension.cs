using System;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Tools;

public static class GammaExtension
{
    public static PortableBitmap ApplyGamma(this PortableBitmap bitmap, float value)
    {
        if (bitmap.GetPixel(0, 0) is not Rgb)
        {
            throw new NotImplementedException();
        }

        var map = new IColorSpace[bitmap.Width, bitmap.Height];

        for (int j = 0; j < bitmap.Height; j++)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                map[i, j] = bitmap.GetPixel(i, j).ToRgb().WithGamma(value);
            }
        }

        return new PortableBitmap(map);
    }

    private static Rgb WithGamma(this Rgb color, float value)
    {
        return new Rgb(
            float.Pow(color.FirstChannel.Value, value),
            float.Pow(color.SecondChannel.Value, value),
            float.Pow(color.ThirdChannel.Value, value));
    }
}
