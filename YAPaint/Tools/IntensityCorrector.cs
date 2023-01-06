using YAPaint.Models;

namespace YAPaint.Tools;

public static class IntensityCorrector
{
    public static void CorrectIntensity(ref PortableBitmap bitmap, double proportion)
    {
        (float leftEdge, float rightEdge) = FindEdges(bitmap, proportion);

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var newPixelFirst = float.Clamp((pixel.First - leftEdge) / (rightEdge - leftEdge), 0, 1);
                var newPixelSecond = float.Clamp((pixel.Second - leftEdge) / (rightEdge - leftEdge), 0, 1);
                var newPixelThird = float.Clamp((pixel.Third - leftEdge) / (rightEdge - leftEdge), 0, 1);
                var newPixel = new ColorSpace { First = newPixelFirst, Second = newPixelSecond, Third = newPixelThird };
                bitmap.SetPixel(x, y, newPixel);
            }
        }
    }

    private static (float leftEdge, float rightEdge) FindEdges(PortableBitmap bitmap, double proportion)
    {
        var histograms = HistogramGenerator.CreateHistograms(bitmap);
        var currentBack = 0d;
        int i;
        var threshold = proportion * bitmap.Width * bitmap.Height * 3;
        for (i = 0; i < 256; i++)
        {
            for (int c = 0; c < 3; c++)
            {
                currentBack += histograms[c][i];
            }

            if (currentBack >= threshold)
                break;
        }

        if (i >= 256)
            i = 255;
        float leftEdge = i / 255f;

        currentBack = 0;
        for (i = 255; i >= 0; i--)
        {
            for (int c = 0; c < 3; c++)
            {
                currentBack += histograms[c][i];
            }

            if (currentBack >= threshold)
                break;
        }

        if (i < 0)
            i = 0;
        float rightEdge = i / 255f;

        return (leftEdge, rightEdge);
    }
}
