using YAPaint.Models;

namespace YAPaint.Tools;

public static class GammaExtension
{
    public static ColorSpace[,] ApplyGamma(this PortableBitmap bitmap, float value)
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

        return map;
    }

    private static ColorSpace WithGamma(this ref ColorSpace color, float value)
    {
        if (float.IsPositiveInfinity(value))
        {
            return new ColorSpace(
                ComputeSRgbGamma(color.First),
                ComputeSRgbGamma(color.Second),
                ComputeSRgbGamma(color.Third));
        }

        if (value == 0)
        {
            return new ColorSpace(
                ComputeInverseSRgbGamma(color.First),
                ComputeInverseSRgbGamma(color.Second),
                ComputeInverseSRgbGamma(color.Third));
            
        }

        return new ColorSpace(
            float.Pow(color.First, value),
            float.Pow(color.Second, value),
            float.Pow(color.Third, value));
    }

    private static float ComputeSRgbGamma(float value)
    {
        if (value > 0.0031308f)
        {
            return 1.055f * float.Pow(value, 1 / 2.4f) - 0.055f;
        }

        return value * 12.92f;
    }

    private static float ComputeInverseSRgbGamma(float value)
    {
        if (value > 0.04045f)
        {
            return float.Pow((value + 0.055f) / 1.055f, 2.4f);
        }

        return value / 12.92f;
    }
}
