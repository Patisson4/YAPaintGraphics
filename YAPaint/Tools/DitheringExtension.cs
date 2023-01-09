using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class DitheringExtension
{
    private static readonly float[,] OrderedDitherValues =
    {
        { -0.5f, 0.25f, -0.3125f, 0.4375f, -0.453125f, 0.296875f, -0.265625f, 0.484375f },
        { 0f, -0.25f, 0.1875f, -0.0625f, 0.046875f, -0.203125f, 0.234375f, -0.015625f },
        { -0.375f, 0.375f, -0.4375f, 0.3125f, -0.328125f, 0.421875f, -0.390625f, 0.359375f },
        { 0.125f, -0.125f, 0.0625f, -0.1875f, 0.171875f, -0.078125f, 0.109375f, -0.140625f },
        { -0.46875f, 0.28125f, -0.28125f, 0.46875f, -0.484375f, 0.265625f, -0.296875f, 0.453125f },
        { 0.03125f, -0.21875f, 0.21875f, -0.03125f, 0.015625f, -0.234375f, 0.203125f, -0.046875f },
        { -0.34375f, 0.40625f, -0.40625f, 0.34375f, -0.359375f, 0.390625f, -0.421875f, 0.328125f },
        { 0.15625f, -0.09375f, 0.09375f, -0.15625f, 0.140625f, -0.109375f, 0.078125f, -0.171875f },
    };

    public static PortableBitmap DitherOrdered(this PortableBitmap bitmap, int bitDepth)
    {
        var newMap = new ColorSpace[bitmap.Width, bitmap.Height];

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                ColorSpace pixel = bitmap.GetPixel(x, y);
                (float First, float Second, float Third) buffer = (pixel.First, pixel.Second, pixel.Third);

                buffer.First += OrderedDitherValues[x % 8, y % 8] / bitDepth;
                buffer.Second += OrderedDitherValues[x % 8, y % 8] / bitDepth;
                buffer.Third += OrderedDitherValues[x % 8, y % 8] / bitDepth;

                newMap[x, y] = Quantize(ref buffer, (1 << bitDepth) - 1);
            }
        }

        return new PortableBitmap(
            newMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
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

        return new PortableBitmap(
            newMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap DitherFloydSteinberg(this PortableBitmap bitmap, int bitDepth)
    {
        var newMap = new ColorSpace[bitmap.Width, bitmap.Height];
        var bufferMap = new (float First, float Second, float Third)[bitmap.Width, bitmap.Height];

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

                float errorFirst = bufferMap[x, y].First - newMap[x, y].First;
                float errorSecond = bufferMap[x, y].Second - newMap[x, y].Second;
                float errorThird = bufferMap[x, y].Third - newMap[x, y].Third;

                PropagateError(bufferMap, x + 1, y, errorFirst, errorSecond, errorThird, 7 / 16.0f);
                PropagateError(bufferMap, x - 1, y + 1, errorFirst, errorSecond, errorThird, 3 / 16.0f);
                PropagateError(bufferMap, x, y + 1, errorFirst, errorSecond, errorThird, 5 / 16.0f);
                PropagateError(bufferMap, x + 1, y + 1, errorFirst, errorSecond, errorThird, 1 / 16.0f);
            }
        }

        return new PortableBitmap(
            newMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap DitherAtkinson(this PortableBitmap bitmap, int bitDepth)
    {
        var newMap = new ColorSpace[bitmap.Width, bitmap.Height];
        var bufferMap = new (float First, float Second, float Third)[bitmap.Width, bitmap.Height];

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

                float errorFirst = bufferMap[x, y].First - newMap[x, y].First;
                float errorSecond = bufferMap[x, y].Second - newMap[x, y].Second;
                float errorThird = bufferMap[x, y].Third - newMap[x, y].Third;

                PropagateError(bufferMap, x + 1, y, errorFirst, errorSecond, errorThird, 1 / 8.0f);
                PropagateError(bufferMap, x + 2, y, errorFirst, errorSecond, errorThird, 1 / 8.0f);
                PropagateError(bufferMap, x - 1, y + 1, errorFirst, errorSecond, errorThird, 1 / 8.0f);
                PropagateError(bufferMap, x, y + 1, errorFirst, errorSecond, errorThird, 1 / 8.0f);
                PropagateError(bufferMap, x + 1, y + 1, errorFirst, errorSecond, errorThird, 1 / 8.0f);
                PropagateError(bufferMap, x, y + 2, errorFirst, errorSecond, errorThird, 1 / 8.0f);
            }
        }

        return new PortableBitmap(
            newMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    private static ColorSpace Quantize(ref (float First, float Second, float Third) color, int maxValue)
    {
        return new ColorSpace
        {
            First = float.Round(color.First * maxValue) / maxValue,
            Second = float.Round(color.Second * maxValue) / maxValue,
            Third = float.Round(color.Third * maxValue) / maxValue,
        };
    }

    private static void PropagateError(
        (float First, float Second, float Third)[,] bitmap,
        int x,
        int y,
        float rError,
        float gError,
        float bError,
        float factor)
    {
        if (x < 0 || x >= bitmap.GetLength(0) || y < 0 || y >= bitmap.GetLength(1))
        {
            return;
        }

        bitmap[x, y].First += rError * factor;
        bitmap[x, y].Second += gError * factor;
        bitmap[x, y].Third += bError * factor;
    }
}
