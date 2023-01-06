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
        { 42, 26, 38, 22, 41, 25, 37, 21 }
    };
    public static PortableBitmap DitherOrdered(this PortableBitmap bitmap, int bitDepth)
    {
        
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                // Get the current pixel and its color values
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                // Add a dither value based on the position in the 8x8 grid
                r += OrderedDitherValues[x % 8, y % 8];
                g += OrderedDitherValues[x % 8, y % 8];
                b += OrderedDitherValues[x % 8, y % 8];

                r = Math.Clamp(r, 0, 255);
                g = Math.Clamp(g, 0, 255);
                b = Math.Clamp(b, 0, 255);

                // Quantize the color values to the specified bit depth
                int quantizedR = (int)Math.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)Math.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)Math.Round(b / (float)((1 << bitDepth) - 1));

                // Calculate the error between the original and quantized values
                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                // Propagate the error to the neighboring pixels
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
        Random random = new Random();

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                // Get the current pixel and its color values
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                // Add a random value to the color values
                r += random.Next(-128, 128);
                g += random.Next(-128, 128);
                b += random.Next(-128, 128);
                
                r = Math.Clamp(r, 0, 255);
                g = Math.Clamp(g, 0, 255);
                b = Math.Clamp(b, 0, 255);

                // Quantize the color values to the specified bit depth
                int quantizedR = (int)Math.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)Math.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)Math.Round(b / (float)((1 << bitDepth) - 1));

                // Calculate the error between the original and quantized values
                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                // Propagate the error to the neighboring pixels
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
                // Get the current pixel and its color values
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                // Quantize the color values to the specified bit depth
                int quantizedR = (int)Math.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)Math.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)Math.Round(b / (float)((1 << bitDepth) - 1));

                // Calculate the error between the original and quantized values
                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                // Propagate the error to the neighboring pixels
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
                // Get the current pixel and its color values
                ColorSpace currentPixel = bitmap.GetPixel(x, y);
                int r = Coefficient.Denormalize(currentPixel.First);
                int g = Coefficient.Denormalize(currentPixel.Second);
                int b = Coefficient.Denormalize(currentPixel.Third);

                // Quantize the color values to the specified bit depth
                int quantizedR = (int)Math.Round(r / (float)((1 << bitDepth) - 1));
                int quantizedG = (int)Math.Round(g / (float)((1 << bitDepth) - 1));
                int quantizedB = (int)Math.Round(b / (float)((1 << bitDepth) - 1));

                // Calculate the error between the original and quantized values
                int rError = r - quantizedR;
                int gError = g - quantizedG;
                int bError = b - quantizedB;

                // Propagate the error to the neighboring pixels
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
        PortableBitmap bitmap, int x, int y,
        int rError, int gError, int bError,
        float factor)
    {
        if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
        {
            ColorSpace newPixel = new ColorSpace( Coefficient.Normalize((int)Math.Round(rError * factor)), Coefficient.Normalize((int)Math.Round(gError * factor)), Coefficient.Normalize((int)Math.Round(bError * factor)));
            bitmap.SetPixel(x, y, newPixel);
        }
    }
}