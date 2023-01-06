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

    public static PortableBitmap DitherOrdered(this PortableBitmap bitmap, int bitDepth)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
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

                PropagateError(bitmap, x + 1, y, rError, gError, bError, 7 / 16.0f);
                PropagateError(bitmap, x - 1, y + 1, rError, gError, bError, 3 / 16.0f);
                PropagateError(bitmap, x, y + 1, rError, gError, bError, 5 / 16.0f);
                PropagateError(bitmap, x + 1, y + 1, rError, gError, bError, 1 / 16.0f);
            }
        }

        return bitmap;
    }

    public static PortableBitmap DitherRandom(this PortableBitmap bitmap, int bitDepth)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                r += Random.Shared.Next(-128, 128);
                g += Random.Shared.Next(-128, 128);
                b += Random.Shared.Next(-128, 128);

                r = int.Clamp(r, 0, 255);
                g = int.Clamp(g, 0, 255);
                b = int.Clamp(b, 0, 255);

                int quantizedR = (int)Math.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)Math.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)Math.Round(b / (float)((1 << bitDepth) - 1));

                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                PropagateError(bitmap, x + 1, y, rError, gError, bError, 7 / 16.0f);
                PropagateError(bitmap, x - 1, y + 1, rError, gError, bError, 3 / 16.0f);
                PropagateError(bitmap, x, y + 1, rError, gError, bError, 5 / 16.0f);
                PropagateError(bitmap, x + 1, y + 1, rError, gError, bError, 1 / 16.0f);
            }
        }

        return bitmap;
    }

    public static PortableBitmap DitherFloydSteinberg(this PortableBitmap bitmap, int bitDepth)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                int quantizedR = (int)float.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)float.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)float.Round(b / (float)((1 << bitDepth) - 1));

                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                PropagateError(bitmap, x + 1, y, rError, gError, bError, 7 / 16.0f);
                PropagateError(bitmap, x - 1, y + 1, rError, gError, bError, 3 / 16.0f);
                PropagateError(bitmap, x, y + 1, rError, gError, bError, 5 / 16.0f);
                PropagateError(bitmap, x + 1, y + 1, rError, gError, bError, 1 / 16.0f);
            }
        }

        return bitmap;
    }

    public static PortableBitmap DitherAtkinson(this PortableBitmap bitmap, int bitDepth)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
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

    private static void PropagateError(
        PortableBitmap bitmap,
        int x,
        int y,
        int rError,
        int gError,
        int bError,
        float factor)
    {
        if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
        {
            return;
        }

        var newPixel = new ColorSpace
        {
            First = Coefficient.Normalize((int)Math.Round(rError * factor)),
            Second = Coefficient.Normalize((int)Math.Round(gError * factor)),
            Third = Coefficient.Normalize((int)Math.Round(bError * factor)),
        };

        bitmap.SetPixel(x, y, newPixel);
    }
}
