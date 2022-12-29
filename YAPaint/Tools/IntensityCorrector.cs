using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public class IntensityCorrector
{
    public static PortableBitmap CorrectIntensity(PortableBitmap bitmap, double ignoreProportion, double[][] histograms)
    {
        // Determine the minimum and maximum values in each histogram, ignoring the specified proportion of the brightest and darkest pixels
        var minValues = new double[3];
        var maxValues = new double[3];

        for (var c = 0; c < 3; c++)
        {
            // Set the minimum and maximum values to the maximum and minimum possible values, respectively
            minValues[c] = 255;
            maxValues[c] = 0;

            // Calculate the threshold based on the number of pixels to ignore
            var threshold = ignoreProportion * bitmap.Width * bitmap.Height;

            // Find the minimum and maximum values above and below the threshold
            var countAboveThreshold = 0d;
            var countBelowThreshold = 0d;
            for (var v = 0; v < 256; v++)
            {
                if (histograms[c][v] <= 0) continue;
                if (countAboveThreshold < threshold)
                {
                    minValues[c] = v;
                    countAboveThreshold += histograms[c][v];
                }

                if (countBelowThreshold < threshold)
                {
                    maxValues[c] = v;
                    countBelowThreshold += histograms[c][v];
                }
            }
        }

// Calculate the scaling and shifting factors for each color channel
        var scalingFactors = new double[3];
        var shiftingFactors = new double[3];

        for (int c = 0; c < 3; c++)
        {
            scalingFactors[c] = (maxValues[c] - minValues[c]) / 255.0;
            shiftingFactors[c] = minValues[c];
        }

// Apply the scaling and shifting factors to each pixel in the image
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                var newPixelFirst = Coefficient.Normalize((byte)Math.Min(
                    Math.Max((Coefficient.Denormalize(pixel.First) * scalingFactors[0]) + shiftingFactors[0], 0), 255));
                var newPixelSecond = Coefficient.Normalize((byte)Math.Min(
                    Math.Max((Coefficient.Denormalize(pixel.Second) * scalingFactors[1]) + shiftingFactors[1], 0),
                    255));
                var newPixelThird = Coefficient.Normalize((byte)Math.Min(
                    Math.Max((Coefficient.Denormalize(pixel.Third) * scalingFactors[2]) + shiftingFactors[2], 0), 255));
                var newPixel = new ColorSpace(newPixelFirst, newPixelSecond, newPixelThird);
                bitmap.SetPixel(x, y, newPixel);
            }
        }

        return bitmap;
    }


}