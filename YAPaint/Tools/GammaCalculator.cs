using YAPaint.Models;

namespace YAPaint.Tools;

public static class GammaCalculator
{
    public static PortableBitmap ApplyGamma(this PortableBitmap bitmap, float value)
    {
        var map = new ColorSpace[bitmap.Width, bitmap.Height];

        for (int j = 0; j < bitmap.Height; j++)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                ColorSpace pixel = bitmap.GetPixel(i, j);
                map[i, j] = pixel.WithGamma(value);
            }
        }

        return new PortableBitmap(
            map,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    private static ColorSpace WithGamma(this ref ColorSpace color, float value)
    {
        if (float.IsPositiveInfinity(value))
        {
            return new ColorSpace
            {
                First = CalculateSRgbGamma(color.First),
                Second = CalculateSRgbGamma(color.Second),
                Third = CalculateSRgbGamma(color.Third),
            };
        }

        if (value == 0)
        {
            return new ColorSpace
            {
                First = CalculateInverseSRgbGamma(color.First),
                Second = CalculateInverseSRgbGamma(color.Second),
                Third = CalculateInverseSRgbGamma(color.Third),
            };
        }

        return new ColorSpace
        {
            First = float.Pow(color.First, value),
            Second = float.Pow(color.Second, value),
            Third = float.Pow(color.Third, value),
        };
    }

    private static float CalculateSRgbGamma(float value)
    {
        if (value > 0.0031308f)
        {
            return 1.055f * float.Pow(value, 1 / 2.4f) - 0.055f;
        }

        return value * 12.92f;
    }

    private static float CalculateInverseSRgbGamma(float value)
    {
        if (value > 0.04045f)
        {
            return float.Pow((value + 0.055f) / 1.055f, 2.4f);
        }

        return value / 12.92f;
    }
}
