using System;
using YAPaint.Models;
using YAPaint.Models.ExtraColorSpaces;

namespace YAPaint.Tools;

public class BarGrapher
{
    static int[][] CreateBarGraphs(PortableBitmap bitmap)
    {
        // Create an array to hold the histogram data for each color channel
        var histograms = new int[3][];
        for (var i = 0; i < 3; i++)
        {
            histograms[i] = new int[256];
        }

        // Generate the histogram data
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                histograms[0][Coefficient.Denormalize(pixel.First)]++;
                histograms[1][Coefficient.Denormalize(pixel.First)]++;
                histograms[2][Coefficient.Denormalize(pixel.First)]++;
            }
        }

        return histograms;

    }

}