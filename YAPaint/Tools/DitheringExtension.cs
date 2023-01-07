using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class DitheringExtension
{
    private static readonly int[,] OrderedDitherValues =
    {
        { 0, 48, 12, 60, 3, 51, 15, 63 },
        { 32, 16, 44, 28, 35, 19, 47, 31 },
        { 8, 56, 4, 52, 11, 59, 7, 55 },
        { 40, 24, 36, 20, 43, 27, 39, 23 },
        { 2, 50, 14, 62, 1, 49, 13, 61 },
        { 34, 18, 46, 30, 33, 17, 45, 29 },
        { 10, 58, 6, 54, 9, 57, 5, 53 },
        { 42, 26, 38, 22, 41, 25, 37, 21 },
    };

    //WRN: doesn't work; TODO: fix
    public static PortableBitmap DitherOrdered(this PortableBitmap bitmap, int bitDepth)
    {
        var oldBitmap = bitmap;
        var newBitmap = bitmap;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace currentPixel = oldBitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                r += OrderedDitherValues[x % 8, y % 8];
                g += OrderedDitherValues[x % 8, y % 8];
                b += OrderedDitherValues[x % 8, y % 8];

                r = int.Clamp(r, 0, 255);
                g = int.Clamp(g, 0, 255);
                b = int.Clamp(b, 0, 255);

                int quantizedR = (int)float.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)float.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)float.Round(b / (float)((1 << bitDepth) - 1));

                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                PropagateError(newBitmap, x + 1, y, rError, gError, bError, 7 / 16.0f);
                PropagateError(newBitmap, x - 1, y + 1, rError, gError, bError, 3 / 16.0f);
                PropagateError(newBitmap, x, y + 1, rError, gError, bError, 5 / 16.0f);
                PropagateError(newBitmap, x + 1, y + 1, rError, gError, bError, 1 / 16.0f);
            }
        }

        return newBitmap;
    }

    public static PortableBitmap DitherRandom(this PortableBitmap bitmap)
    {
        var newMap = new ColorSpace[bitmap.Width, bitmap.Height];

        for (int i = 0; i < bitmap.Width; i++)
        {
            for (int j = 0; j < bitmap.Height; j++)
            {
                newMap[i, j] = bitmap.GetPixel(i, j);
            }
        }

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace color = bitmap.GetPixel(x, y);
                double grayscale = bitmap.ColorConverter.GetGrayValue(color);
                float threshold = Random.Shared.NextSingle();

                if (grayscale < threshold)
                {
                    newMap[x, y] = bitmap.ColorConverter.Black;
                }
                else
                {
                    newMap[x, y] = bitmap.ColorConverter.White;
                }
            }
        }

        return new PortableBitmap(newMap, bitmap.ColorConverter);
    }

    public static PortableBitmap DitherFloydSteinberg(this PortableBitmap bitmap, int bitDepth)
    {
        var newMap = new ColorSpace[bitmap.Width, bitmap.Height];
        var bufferMap = new (float, float, float)[bitmap.Width, bitmap.Height];

        for (int i = 0; i < bitmap.Width; i++)
        {
            for (int j = 0; j < bitmap.Height; j++)
            {
                ColorSpace pixel = bitmap.GetPixel(i, j);
                bufferMap[i, j] = (pixel.First, pixel.Second, pixel.Third);
            }
        }

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                newMap[x, y] = Quantize(ref bufferMap[x, y], (1 << bitDepth) - 1);

                float errorFirst = bufferMap[x, y].Item1 - newMap[x, y].First;
                float errorSecond = bufferMap[x, y].Item2 - newMap[x, y].Second;
                float errorThird = bufferMap[x, y].Item3 - newMap[x, y].Third;

                if (x + 1 < bitmap.Width)
                {
                    bufferMap[x + 1, y].Item1 += 7 / 16f * errorFirst;
                    bufferMap[x + 1, y].Item2 += 7 / 16f * errorSecond;
                    bufferMap[x + 1, y].Item3 += 7 / 16f * errorThird;
                }

                if (y + 1 >= bitmap.Height)
                {
                    continue;
                }

                if (x - 1 >= 0)
                {
                    bufferMap[x - 1, y + 1].Item1 += 3 / 16f * errorFirst;
                    bufferMap[x - 1, y + 1].Item2 += 3 / 16f * errorSecond;
                    bufferMap[x - 1, y + 1].Item3 += 3 / 16f * errorThird;
                }

                bufferMap[x, y + 1].Item1 += 5 / 16f * errorFirst;
                bufferMap[x, y + 1].Item2 += 5 / 16f * errorSecond;
                bufferMap[x, y + 1].Item3 += 5 / 16f * errorThird;

                if (x + 1 < bitmap.Width)
                {
                    bufferMap[x + 1, y + 1].Item1 += 1 / 16f * errorFirst;
                    bufferMap[x + 1, y + 1].Item2 += 1 / 16f * errorSecond;
                    bufferMap[x + 1, y + 1].Item3 += 1 / 16f * errorThird;
                }
            }
        }

        return new PortableBitmap(newMap, bitmap.ColorConverter);
    }

    //WRN: doesn't work; TODO: fix
    public static PortableBitmap DitherAtkinson(this PortableBitmap bitmap, int bitDepth)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var oldBitmap = bitmap;
                ColorSpace currentPixel = oldBitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                int quantizedR = (int)float.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)float.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)float.Round(b / (float)((1 << bitDepth) - 1));

                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                PropagateError(bitmap, x + 1, y, rError, gError, bError, 1 / 8.0f);
                PropagateError(bitmap, x + 2, y, rError, gError, bError, 1 / 8.0f);
                PropagateError(bitmap, x - 1, y + 1, rError, gError, bError, 1 / 8.0f);
                PropagateError(bitmap, x, y + 1, rError, gError, bError, 1 / 8.0f);
                PropagateError(bitmap, x + 1, y + 1, rError, gError, bError, 1 / 8.0f);
                PropagateError(bitmap, x, y + 2, rError, gError, bError, 1 / 8.0f);
            }
        }

        return bitmap;
    }

    private static ColorSpace Quantize(ref (float, float, float) color, int maxValue)
    {
        return new ColorSpace
        {
            First = float.Round(color.Item1 * maxValue) / maxValue,
            Second = float.Round(color.Item2 * maxValue) / maxValue,
            Third = float.Round(color.Item3 * maxValue) / maxValue,
        };
    }

    [Obsolete("Implement your own boundary check in-place")]
    private static void PropagateError(
        PortableBitmap bitmap,
        int x,
        int y,
        float rError,
        float gError,
        float bError,
        float factor)
    {
        if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
        {
            return;
        }

        var oldPixel = bitmap.GetPixel(x, y);
        var newPixel = new ColorSpace
        {
            First = Math.Clamp(oldPixel.First + rError * factor, 0, 1),
            Second = Math.Clamp(oldPixel.Second + gError * factor, 0, 1),
            Third = Math.Clamp(oldPixel.Third + bError * factor, 0, 1),
        };

        bitmap.SetPixel(x, y, newPixel);
    }
}
