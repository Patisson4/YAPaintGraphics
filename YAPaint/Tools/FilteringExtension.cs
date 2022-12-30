using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class FilteringExtension
{
    public static PortableBitmap ThresholdFilter(this PortableBitmap bitmap, int threshold)
    {
        if (threshold is < 0 or > 255)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), threshold, "Threshold must be between 0 and 255");
        }

        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var color = bitmap.GetPixel(x, y);
                var averageColor = (Coefficient.Denormalize(color.First)
                                  + Coefficient.Denormalize(color.Second)
                                  + Coefficient.Denormalize(color.Third))
                                 / 3;
                if (averageColor >= threshold)
                {
                    filteredMap[x, y] = new ColorSpace(1, 1, 1); //TODO: use static black&white colors
                }
                else
                {
                    filteredMap[x, y] = new ColorSpace(0, 0, 0);
                }
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    public static PortableBitmap OtsuThresholdFilter(this PortableBitmap bitmap)
    {
        var histogram = BarGrapher.CreateGreyBarGraph(bitmap);

        double sum = 0;
        for (int i = 0; i < 256; i++)
        {
            sum += i * histogram[i];
        }

        double sumB = 0;
        double wB = 0;
        var maxVariance = 0.0;
        var threshold = 0;
        for (int i = 0; i < 256; i++)
        {
            wB += histogram[i];
            if (wB == 0) continue;

            double wF = bitmap.Width * bitmap.Height - wB;
            if (wF == 0) break;

            sumB += i * histogram[i];
            var meanB = sumB / wB;
            var meanF = (sum - sumB) / wF;
            var variance = wB * wF * (meanB - meanF) * (meanB - meanF);

            if (!(variance > maxVariance))
            {
                continue;
            }

            maxVariance = variance;
            threshold = i;
        }

        return bitmap.ThresholdFilter(threshold);
    }

    public static PortableBitmap MedianFilter(this PortableBitmap bitmap, int kernelRadius)
    {
        if (kernelRadius < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kernelRadius),
                kernelRadius,
                "Kernel radius must be non-negative");
        }

        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        _kernelBuffer = new ColorSpace[kernelRadius * 2 + 1, kernelRadius * 2 + 1];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var neighbors = bitmap.GetNeighborsColorSpaces(x, y, kernelRadius);
                var median = FindMedian(neighbors);
                filteredMap[x, y] = new ColorSpace(median.First, median.Second, median.Third);
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    private static ColorSpace FindMedian(ColorSpace[,] matrix)
    {
        Span<ColorSpace> span = stackalloc ColorSpace[matrix.GetLength(0) * matrix.GetLength(1)];

        var index = 0;
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                span[index++] = matrix[i, j];
            }
        }

        span.Sort(
            (color, other) =>
                (Coefficient.Denormalize(color.First)
               + Coefficient.Denormalize(color.Second)
               + Coefficient.Denormalize(color.Third))
              / 3
              - (Coefficient.Denormalize(other.First)
               + Coefficient.Denormalize(other.Second)
               + Coefficient.Denormalize(other.Third))
              / 3);
        return span[span.Length / 2];
    }

    public static PortableBitmap GaussianFilter(this PortableBitmap bitmap, int sigma)
    {
        if (sigma < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Sigma must be positive");
        }

        var kernelRadius = 3 * sigma;
        var kernelSize = kernelRadius * 2 + 1;
        var kernel = new float[kernelSize, kernelSize];
        var sum = 0.0f;
        for (int x = 0; x < kernelSize; x++)

        for (int y = 0; y < kernelSize; y++)
        {
            var dx = x - kernelRadius;
            var dy = y - kernelRadius;
            kernel[x, y] = float.Exp(-0.5f * (dx * dx + dy * dy) / (sigma * sigma)) / (2 * float.Pi * sigma * sigma);
            sum += kernel[x, y];
        }

        for (int x = 0; x < kernelSize; x++)
        {
            for (int y = 0; y < kernelSize; y++)
            {
                kernel[x, y] /= sum;
            }
        }

        _kernelBuffer = new ColorSpace[kernelSize, kernelSize];
        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var neighbors = bitmap.GetNeighborsColorSpaces(x, y, kernelRadius);
                var color1 = 0.0f;
                var color2 = 0.0f;
                var color3 = 0.0f;
                for (int i = 0; i < kernelSize; i++)
                {
                    for (int j = 0; j < kernelSize; j++)
                    {
                        var weight = kernel[i, j];
                        color1 += weight * neighbors[i, j].First;
                        color2 += weight * neighbors[i, j].Second;
                        color3 += weight * neighbors[i, j].Third;
                    }
                }

                filteredMap[x, y] = new ColorSpace(
                    float.Clamp(color1, 0, 1),
                    float.Clamp(color2, 0, 1),
                    float.Clamp(color3, 0, 1));
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    public static PortableBitmap BoxBlur(this PortableBitmap bitmap, int kernelRadius)
    {
        if (kernelRadius < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kernelRadius),
                kernelRadius,
                "Kernel radius must be non-negative");
        }

        var kernelSize = kernelRadius * 2 + 1;
        _kernelBuffer = new ColorSpace[kernelSize, kernelSize];
        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var neighbors = bitmap.GetNeighborsColorSpaces(x, y, kernelRadius);
                var color1 = 0.0f;
                var color2 = 0.0f;
                var color3 = 0.0f;
                for (int i = 0; i < kernelSize; i++)
                {
                    for (int j = 0; j < kernelSize; j++)
                    {
                        color1 += neighbors[i, j].First;
                        color2 += neighbors[i, j].Second;
                        color3 += neighbors[i, j].Third;
                    }
                }

                color1 /= kernelSize * kernelSize;
                color2 /= kernelSize * kernelSize;
                color3 /= kernelSize * kernelSize;
                filteredMap[x, y] = new ColorSpace(color1, color2, color3);
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    public static PortableBitmap SobelFilter(this PortableBitmap bitmap)
    {
        _kernelBuffer = new ColorSpace[3, 3];
        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var neighbors = bitmap.GetNeighborsColorSpaces(x, y, 1);
                var gx = neighbors[0, 0].First * -1
                       + neighbors[0, 2].First * 1
                       + neighbors[1, 0].First * -2
                       + neighbors[1, 2].First * 2
                       + neighbors[2, 0].First * -1
                       + neighbors[2, 2].First * 1;
                var gy = neighbors[0, 0].First * -1
                       + neighbors[2, 0].First * 1
                       + neighbors[0, 1].First * -2
                       + neighbors[2, 1].First * 2
                       + neighbors[0, 2].First * -1
                       + neighbors[2, 2].First * 1;
                var g = float.Sqrt(gx * gx + gy * gy);
                if (g > 1)
                {
                    g = 1;
                }

                filteredMap[x, y] = new ColorSpace(g, g, g);
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    public static PortableBitmap ContrastAdaptiveSharpening(this PortableBitmap bitmap, float sharpness)
    {
        if (sharpness is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sharpness), sharpness, "Sharpness must be between 0 and 1");
        }

        _kernelBuffer = new ColorSpace[3, 3];
        var weights = new float[3, bitmap.Width, bitmap.Height];
        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var neighbors = bitmap.GetNeighborsColorSpaces(x, y, 1);
                float minR = float.MaxValue;
                float maxR = 0.0f;
                float minG = float.MaxValue;
                float maxG = 0.0f;
                float minB = float.MaxValue;
                float maxB = 0.0f;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == 1 && j == 0 || i == 0 && j == 1 || i == 1 && j == 1 || i == 2 && j == 1 ||
                            i == 1 && j == 2)
                        {
                            minR = Math.Min(minR, neighbors[i, j].First);
                            maxR = Math.Max(maxR, neighbors[i, j].First);
                            minG = Math.Min(minG, neighbors[i, j].Second);
                            maxG = Math.Max(maxG, neighbors[i, j].Second);
                            minB = Math.Min(minB, neighbors[i, j].Third);
                            maxB = Math.Max(maxB, neighbors[i, j].Third);
                        }
                    }
                }

                float knob = -0.125f * (1 - sharpness) + -0.2f * sharpness;
                float w_R = float.Sqrt(float.Min(0 + minR, 1 - maxR) / maxR) * knob;
                float w_G = float.Sqrt(float.Min(0 + minG, 1 - maxG) / maxG) * knob;
                float w_B = float.Sqrt(float.Min(0 + minB, 1 - maxB) / maxB) * knob;
                weights[0, x, y] = w_R;
                weights[1, x, y] = w_G;
                weights[2, x, y] = w_B;

                float sharpenedColorR = float.Clamp((w_R * neighbors[1, 0].First + w_R * neighbors[0, 1].First +
                                                     1 * neighbors[1, 1].First + w_R * neighbors[2, 1].First +
                                                     w_R * neighbors[1, 2].First) / (w_R * 4 + 1), 0, 1);
                float sharpenedColorG = float.Clamp((w_G * neighbors[1, 0].Second + w_G * neighbors[0, 1].Second +
                                                     1 * neighbors[1, 1].Second + w_G * neighbors[2, 1].Second +
                                                     w_G * neighbors[1, 2].Second) / (w_G * 4 + 1), 0, 1);
                float sharpenedColorB = float.Clamp((w_B * neighbors[1, 0].Third + w_B * neighbors[0, 1].Third +
                                                     1 * neighbors[1, 1].Third + w_B * neighbors[2, 1].Third +
                                                     w_B * neighbors[1, 2].Third) / (w_B * 4 + 1), 0, 1);

                filteredMap[x, y] = new ColorSpace(sharpenedColorR, sharpenedColorG, sharpenedColorB);
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.IsFirstVisible,
            bitmap.IsSecondVisible,
            bitmap.IsThirdVisible);
    }

    private static ColorSpace[,] _kernelBuffer;

    private static ColorSpace[,] GetNeighborsColorSpaces(this PortableBitmap bitmap, int x, int y, int kernelRadius)
    {
        for (int i = x - kernelRadius; i <= x + kernelRadius; i++)
        {
            for (int j = y - kernelRadius; j <= y + kernelRadius; j++)
            {
                if (i < 0 || i >= bitmap.Width || j < 0 || j >= bitmap.Height)
                {
                    _kernelBuffer[i - (x - kernelRadius), j - (y - kernelRadius)] = bitmap.GetBorderColor(i, j);
                }
                else
                {
                    _kernelBuffer[i - (x - kernelRadius), j - (y - kernelRadius)] = bitmap.GetPixel(i, j);
                }
            }
        }

        return _kernelBuffer;
    }

    private static ColorSpace GetBorderColor(this PortableBitmap bitmap, int x, int y)
    {
        if (x < 0)
        {
            x = 0;
        }
        else if (x >= bitmap.Width)
        {
            x = bitmap.Width - 1;
        }

        if (y < 0)
        {
            y = 0;
        }
        else if (y >= bitmap.Height)
        {
            y = bitmap.Height - 1;
        }

        return bitmap.GetPixel(x, y);
    }
}
