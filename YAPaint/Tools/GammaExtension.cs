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
        return new ColorSpace(
            float.Pow(color.First, value),
            float.Pow(color.Second, value),
            float.Pow(color.Third, value));
    }
}
