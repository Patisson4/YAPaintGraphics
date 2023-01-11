using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class ImageFilter
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
                var averageColor = Coefficient.Denormalize(bitmap.ColorConverter.GetGrayValue(color));
                if (averageColor >= threshold)
                {
                    filteredMap[x, y] = bitmap.ColorConverter.White;
                }
                else
                {
                    filteredMap[x, y] = bitmap.ColorConverter.Black;
                }
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap OtsuFilter(this PortableBitmap bitmap)
    {
        var histogram = HistogramGenerator.CreateGrayHistogram(bitmap);

        double sum = 0;
        for (int i = 0; i < 256; i++)
        {
            sum += i * histogram[i];
        }

        double sumB = 0;
        double wB = 0;
        double maxVariance = 0;
        int threshold = 0;
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
                var neighbors = bitmap.GetNeighbors(x, y, kernelRadius);
                filteredMap[x, y] = FindMedian(neighbors);
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap GaussianFilter(this PortableBitmap bitmap, float sigma)
    {
        if (sigma < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sigma), sigma, "Sigma must be positive");
        }

        var kernelRadius = (int)(3 * sigma);
        var kernelSize = kernelRadius * 2 + 1;
        var kernel = new float[kernelSize, kernelSize];
        var sum = 0f;
        for (int x = 0; x < kernelSize; x++)

        for (int y = 0; y < kernelSize; y++)
        {
            var dx = x - kernelRadius;
            var dy = y - kernelRadius;
            kernel[x, y] = float.Exp(-.5f * (dx * dx + dy * dy) / (sigma * sigma)) / (2 * float.Pi * sigma * sigma);
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
                var neighbors = bitmap.GetNeighbors(x, y, kernelRadius);
                var color1 = 0f;
                var color2 = 0f;
                var color3 = 0f;
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

                filteredMap[x, y] = new ColorSpace
                {
                    First = float.Clamp(color1, 0, 1),
                    Second = float.Clamp(color2, 0, 1),
                    Third = float.Clamp(color3, 0, 1),
                };
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap BoxBlurFilter(this PortableBitmap bitmap, int kernelRadius)
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
                var neighbors = bitmap.GetNeighbors(x, y, kernelRadius);
                var color1 = 0f;
                var color2 = 0f;
                var color3 = 0f;
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
                filteredMap[x, y] = new ColorSpace { First = color1, Second = color2, Third = color3 };
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap SobelFilter(this PortableBitmap bitmap)
    {
        _kernelBuffer = new ColorSpace[3, 3];
        var filteredMap = new ColorSpace[bitmap.Width, bitmap.Height];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var neighbors = bitmap.GetNeighbors(x, y, 1);
                var gx = bitmap.ColorConverter.GetGrayValue(neighbors[0, 0]) * -1
                       + bitmap.ColorConverter.GetGrayValue(neighbors[0, 2]) * 1
                       + bitmap.ColorConverter.GetGrayValue(neighbors[1, 0]) * -2
                       + bitmap.ColorConverter.GetGrayValue(neighbors[1, 2]) * 2
                       + bitmap.ColorConverter.GetGrayValue(neighbors[2, 0]) * -1
                       + bitmap.ColorConverter.GetGrayValue(neighbors[2, 2]) * 1;
                var gy = bitmap.ColorConverter.GetGrayValue(neighbors[0, 0]) * -1
                       + bitmap.ColorConverter.GetGrayValue(neighbors[2, 0]) * 1
                       + bitmap.ColorConverter.GetGrayValue(neighbors[0, 1]) * -2
                       + bitmap.ColorConverter.GetGrayValue(neighbors[2, 1]) * 2
                       + bitmap.ColorConverter.GetGrayValue(neighbors[0, 2]) * -1
                       + bitmap.ColorConverter.GetGrayValue(neighbors[2, 2]) * 1;

                var g = float.Clamp(float.Sqrt(gx * gx + gy * gy), 0, 1);
                filteredMap[x, y] = new ColorSpace { First = g, Second = g, Third = g };
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
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
                var neighbors = bitmap.GetNeighbors(x, y, 1);
                float minR = float.MaxValue;
                float maxR = float.MinValue;
                float minG = float.MaxValue;
                float maxG = float.MinValue;
                float minB = float.MaxValue;
                float maxB = float.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (i is 0 or 2 && j is 0 or 2)
                        {
                            continue;
                        }

                        minR = float.Min(minR, neighbors[i, j].First);
                        maxR = float.Max(maxR, neighbors[i, j].First);
                        minG = float.Min(minG, neighbors[i, j].Second);
                        maxG = float.Max(maxG, neighbors[i, j].Second);
                        minB = float.Min(minB, neighbors[i, j].Third);
                        maxB = float.Max(maxB, neighbors[i, j].Third);
                    }
                }

                float knob = -.125f * (1 - sharpness) + -.2f * sharpness;
                float wR = float.Sqrt(float.Min(0 + minR, 1 - maxR) / maxR) * knob;
                float wG = float.Sqrt(float.Min(0 + minG, 1 - maxG) / maxG) * knob;
                float wB = float.Sqrt(float.Min(0 + minB, 1 - maxB) / maxB) * knob;

                weights[0, x, y] = wR;
                weights[1, x, y] = wG;
                weights[2, x, y] = wB;
            }
        }

        _kernelBuffer = new ColorSpace[5, 5];
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                var wfR = weights[0, x, y];
                var wfG = weights[1, x, y];
                var wfB = weights[2, x, y];
                var wgR = weights[0, int.Clamp(x + 1, 0, bitmap.Width - 1), y];
                var wgG = weights[1, int.Clamp(x + 1, 0, bitmap.Width - 1), y];
                var wgB = weights[2, int.Clamp(x + 1, 0, bitmap.Width - 1), y];
                var wjR = weights[0, x, int.Clamp(y + 1, 0, bitmap.Height - 1)];
                var wjG = weights[1, x, int.Clamp(y + 1, 0, bitmap.Height - 1)];
                var wjB = weights[2, x, int.Clamp(y + 1, 0, bitmap.Height - 1)];
                var wkR = weights[0, int.Clamp(x + 1, 0, bitmap.Width - 1), int.Clamp(y + 1, 0, bitmap.Height - 1)];
                var wkG = weights[1, int.Clamp(x + 1, 0, bitmap.Width - 1), int.Clamp(y + 1, 0, bitmap.Height - 1)];
                var wkB = weights[2, int.Clamp(x + 1, 0, bitmap.Width - 1), int.Clamp(y + 1, 0, bitmap.Height - 1)];

                float s = (1f - (float)x / bitmap.Width) * (1f - (float)y / bitmap.Height);
                float t = (float)x / bitmap.Width * (1f - (float)y / bitmap.Height);
                float u = (1f - (float)x / bitmap.Width) * (float)y / bitmap.Height;
                float v = (float)x / bitmap.Width * (float)y / bitmap.Height;

                var neighbors = bitmap.GetNeighbors(x, y, 2);
                float minR = float.MaxValue;
                float maxR = float.MinValue;
                float minG = float.MaxValue;
                float maxG = float.MinValue;
                float minB = float.MaxValue;
                float maxB = float.MinValue;
                for (int i = 1; i < 4; i++)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        if (i == 2 && j == 1
                         || i == 1 && j == 2
                         || i == 2 && j == 3
                         || i == 3 && j == 2
                         || i == 2 && j == 2)
                        {
                            minR = float.Min(minR, neighbors[i, j].First);
                            maxR = float.Max(maxR, neighbors[i, j].First);
                            minG = float.Min(minG, neighbors[i, j].Second);
                            maxG = float.Max(maxG, neighbors[i, j].Second);
                            minB = float.Min(minB, neighbors[i, j].Third);
                            maxB = float.Max(maxB, neighbors[i, j].Third);
                        }
                    }
                }

                float sR = s / (1 / 32f + maxR - minR);
                float sG = s / (1 / 32f + maxG - minG);
                float sB = s / (1 / 32f + maxB - minB);

                for (int i = 2; i < 5; i++)
                {
                    for (int j = 1; j < 4; j++)
                    {
                        if (i == 3 && j == 1
                         || i == 2 && j == 2
                         || i == 3 && j == 3
                         || i == 4 && j == 2
                         || i == 3 && j == 2)
                        {
                            minR = float.Min(minR, neighbors[i, j].First);
                            maxR = float.Max(maxR, neighbors[i, j].First);
                            minG = float.Min(minG, neighbors[i, j].Second);
                            maxG = float.Max(maxG, neighbors[i, j].Second);
                            minB = float.Min(minB, neighbors[i, j].Third);
                            maxB = float.Max(maxB, neighbors[i, j].Third);
                        }
                    }
                }

                float tR = t / (1 / 32f + maxR - minR);
                float tG = t / (1 / 32f + maxG - minG);
                float tB = t / (1 / 32f + maxB - minB);

                for (int i = 1; i < 4; i++)
                {
                    for (int j = 2; j < 5; j++)
                    {
                        if (i == 2 && j == 2
                         || i == 1 && j == 3
                         || i == 2 && j == 4
                         || i == 3 && j == 3
                         || i == 2 && j == 3)
                        {
                            minR = float.Min(minR, neighbors[i, j].First);
                            maxR = float.Max(maxR, neighbors[i, j].First);
                            minG = float.Min(minG, neighbors[i, j].Second);
                            maxG = float.Max(maxG, neighbors[i, j].Second);
                            minB = float.Min(minB, neighbors[i, j].Third);
                            maxB = float.Max(maxB, neighbors[i, j].Third);
                        }
                    }
                }

                float uR = u / (1 / 32f + maxR - minR);
                float uG = u / (1 / 32f + maxG - minG);
                float uB = u / (1 / 32f + maxB - minB);

                for (int i = 2; i < 5; i++)
                {
                    for (int j = 2; j < 5; j++)
                    {
                        if (i == 3 && j == 2
                         || i == 2 && j == 3
                         || i == 3 && j == 4
                         || i == 4 && j == 3
                         || i == 3 && j == 3)
                        {
                            minR = float.Min(minR, neighbors[i, j].First);
                            maxR = float.Max(maxR, neighbors[i, j].First);
                            minG = float.Min(minG, neighbors[i, j].Second);
                            maxG = float.Max(maxG, neighbors[i, j].Second);
                            minB = float.Min(minB, neighbors[i, j].Third);
                            maxB = float.Max(maxB, neighbors[i, j].Third);
                        }
                    }
                }

                float vR = v / (1 / 32f + maxR - minR);
                float vG = v / (1 / 32f + maxG - minG);
                float vB = v / (1 / 32f + maxB - minB);

                float sharpenedColorR =
                    (neighbors[2, 1].First * wfR * sR
                   + neighbors[1, 2].First * wfR * sR
                   + neighbors[3, 1].First * wgR * tR
                   + neighbors[4, 2].First * wgR * tR
                   + neighbors[1, 3].First * wjR * uR
                   + neighbors[2, 4].First * wjR * uR
                   + neighbors[4, 3].First * wkR * vR
                   + neighbors[3, 4].First * wkR * vR
                   + neighbors[2, 2].First * (wgR * tR + wjR * uR + sR)
                   + neighbors[3, 2].First * (wfR * sR + wkR * vR + tR)
                   + neighbors[2, 3].First * (wfR * sR + wkR * vR + uR)
                   + neighbors[3, 3].First * (wgR * tR + wjR * uR + vR))
                  / (2f * wfR * sR
                   + 2f * wgR * tR
                   + 2f * wjR * uR
                   + 2f * wkR * vR
                   + wgR * tR
                   + wjR * uR
                   + sR
                   + wfR * sR
                   + wkR * vR
                   + tR
                   + wfR * sR
                   + wkR * vR
                   + uR
                   + wgR * tR
                   + wjR * uR
                   + vR);

                float sharpenedColorG =
                    (neighbors[2, 1].Second * wfG * sG
                   + neighbors[1, 2].Second * wfG * sG
                   + neighbors[3, 1].Second * wgG * tG
                   + neighbors[4, 2].Second * wgG * tG
                   + neighbors[1, 3].Second * wjG * uG
                   + neighbors[2, 4].Second * wjG * uG
                   + neighbors[4, 3].Second * wkG * vG
                   + neighbors[3, 4].Second * wkG * vG
                   + neighbors[2, 2].Second * (wgG * tG + wjG * uG + sG)
                   + neighbors[3, 2].Second * (wfG * sG + wkG * vG + tG)
                   + neighbors[2, 3].Second * (wfG * sG + wkG * vG + uG)
                   + neighbors[3, 3].Second * (wgG * tG + wjG * uG + vG))
                  / (2f * wfG * sG
                   + 2f * wgG * tG
                   + 2f * wjG * uG
                   + 2f * wkG * vG
                   + wgG * tG
                   + wjG * uG
                   + sG
                   + wfG * sG
                   + wkG * vG
                   + tG
                   + wfG * sG
                   + wkG * vG
                   + uG
                   + wgG * tG
                   + wjG * uG
                   + vG);

                float sharpenedColorB =
                    (neighbors[2, 1].Third * wfB * sB
                   + neighbors[1, 2].Third * wfB * sB
                   + neighbors[3, 1].Third * wgB * tB
                   + neighbors[4, 2].Third * wgB * tB
                   + neighbors[1, 3].Third * wjB * uB
                   + neighbors[2, 4].Third * wjB * uB
                   + neighbors[4, 3].Third * wkB * vB
                   + neighbors[3, 4].Third * wkB * vB
                   + neighbors[2, 2].Third * (wgB * tB + wjB * uB + sB)
                   + neighbors[3, 2].Third * (wfB * sB + wkB * vB + tB)
                   + neighbors[2, 3].Third * (wfB * sB + wkB * vB + uB)
                   + neighbors[3, 3].Third * (wgB * tB + wjB * uB + vB))
                  / (2f * wfB * sB
                   + 2f * wgB * tB
                   + 2f * wjB * uB
                   + 2f * wkB * vB
                   + wgB * tB
                   + wjB * uB
                   + sB
                   + wfB * sB
                   + wkB * vB
                   + tB
                   + wfB * sB
                   + wkB * vB
                   + uB
                   + wgB * tB
                   + wjB * uB
                   + vB);

                filteredMap[x, y] = new ColorSpace
                {
                    First = float.Clamp(sharpenedColorR, 0, 1),
                    Second = float.Clamp(sharpenedColorG, 0, 1),
                    Third = float.Clamp(sharpenedColorB, 0, 1),
                };
            }
        }

        return new PortableBitmap(
            filteredMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
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

    /// <summary>
    /// Internal buffer to contain sliding-window neighbors of given point <p/>
    /// <b>MUST</b> be initialized before calling <see cref="GetNeighbors"/>
    /// </summary>
    private static ColorSpace[,] _kernelBuffer;

    /// <summary>
    /// IMPORTANT – <b><see cref="_kernelBuffer"/> = new ColorSpace[kernelRadius * 2 + 1, kernelRadius * 2 + 1]</b> before calling this method
    /// </summary>
    private static ColorSpace[,] GetNeighbors(this PortableBitmap bitmap, int x, int y, int kernelRadius)
    {
        for (int i = x - kernelRadius; i <= x + kernelRadius; i++)
        {
            for (int j = y - kernelRadius; j <= y + kernelRadius; j++)
            {
                _kernelBuffer[i - (x - kernelRadius), j - (y - kernelRadius)] = bitmap.GetPixel(
                    int.Clamp(i, 0, bitmap.Width - 1),
                    int.Clamp(j, 0, bitmap.Height - 1));
            }
        }

        return _kernelBuffer;
    }
}
