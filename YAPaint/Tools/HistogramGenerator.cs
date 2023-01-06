using YAPaint.Models;

namespace YAPaint.Tools;

public static class HistogramGenerator
{
    public static double[][] CreateHistograms(PortableBitmap bitmap)
    {
        var histograms = new double[3][];
        for (var i = 0; i < 3; i++)
        {
            histograms[i] = new double[256];
        }

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                histograms[0][Coefficient.Denormalize(pixel.First)]++;
                histograms[1][Coefficient.Denormalize(pixel.Second)]++;
                histograms[2][Coefficient.Denormalize(pixel.Third)]++;
            }
        }

        return histograms;
    }

    public static int[] CreateGrayHistogram(PortableBitmap bitmap)
    {
        var histogram = new int[256];

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                histogram[(int)bitmap.ColorConverter.GetGrayValue(pixel)]++;
            }
        }

        return histogram;
    }
}
