﻿using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class ImageScaler
{
    public static PortableBitmap ScaleNearestNeighbor(this PortableBitmap bitmap, float scaleX, float scaleY,
        float focalPointX, float focalPointY)
    {
        var newBitmap = new PortableBitmap(new ColorSpace[(int)(scaleX * bitmap.Width), (int)(scaleY *
                bitmap.Height)],
            bitmap.ColorConverter);

        var denormalizedFocalPointX = focalPointX * scaleX * bitmap.Width;
        var denormalizedFocalPointY = focalPointY * scaleY * bitmap.Height;

        for (var j = 0; j < newBitmap.Height; j++)
        {
            for (var i = 0; i < newBitmap.Width; i++)
            {
                var x = (int)((denormalizedFocalPointX + i) / scaleX);
                var y = (int)((denormalizedFocalPointY + j) / scaleY);

                x = Math.Clamp(x, 0, bitmap.Width - 1);
                y = Math.Clamp(y, 0, bitmap.Height - 1);

                newBitmap.SetPixel(i, j, bitmap.GetPixel(x, y));
            }
        }

        return newBitmap;
    }


    public static PortableBitmap ScaleBilinear(this PortableBitmap bitmap, float scaleX, float scaleY,
        float focalPointX, float focalPointY)
    {
        int newWidth = (int)(bitmap.Width * scaleX);
        int newHeight = (int)(bitmap.Height * scaleY);

        var newBitmap = new PortableBitmap(new ColorSpace[newWidth, newHeight],
            bitmap.ColorConverter,
            true, true, true);

        float offsetX = focalPointX * scaleX * bitmap.Width;
        float offsetY = focalPointY * scaleY * bitmap.Height;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                float srcX = (x + offsetX) / scaleX;
                float srcY = (y + offsetY) / scaleY;

                int srcX1 = (int)Math.Floor(srcX);
                int srcY1 = (int)Math.Floor(srcY);
                int srcX2 = (int)Math.Floor(srcX) + 1;
                int srcY2 = (int)Math.Floor(srcY) + 1;

                srcX1 = Math.Max(0, Math.Min(srcX1, bitmap.Width - 1));
                srcY1 = Math.Max(0, Math.Min(srcY1, bitmap.Height - 1));
                srcX2 = Math.Max(0, Math.Min(srcX2, bitmap.Width - 1));
                srcY2 = Math.Max(0, Math.Min(srcY2, bitmap.Height - 1));

                ColorSpace interpolatedColor = Interpolate(bitmap, srcX, srcY, srcX1, srcX2, srcY1, srcY2);

                newBitmap.SetPixel(x, y, interpolatedColor);
            }
        }

        return newBitmap;
    }

    private static ColorSpace Interpolate(PortableBitmap bitmap, float srcX, float srcY, int srcX1, int srcX2,
        int srcY1, int srcY2)
    {
        ColorSpace c1 = bitmap.GetPixel(srcX1, srcY1);
        ColorSpace c2 = bitmap.GetPixel(srcX2, srcY1);
        ColorSpace c3 = bitmap.GetPixel(srcX1, srcY2);
        ColorSpace c4 = bitmap.GetPixel(srcX2, srcY2);

        float w1 = (srcX2 - srcX) * (srcY2 - srcY);
        float w2 = (srcX - srcX1) * (srcY2 - srcY);
        float w3 = (srcX2 - srcX) * (srcY - srcY1);
        float w4 = (srcX - srcX1) * (srcY - srcY1);

        float r = c1.First * w1 + c2.First * w2 + c3.First * w3 + c4.First * w4;
        float g = c1.Second * w1 + c2.Second * w2 + c3.Second * w3 + c4.Second * w4;
        float b = c1.Third * w1 + c2.Third * w2 + c3.Third * w3 + c4.Third * w4;

        return new ColorSpace(Math.Clamp(r, 0, 1), Math.Clamp(g, 0, 1), Math.Clamp(b, 0, 1));
    }

    public static PortableBitmap ScaleLanczos3(
        this PortableBitmap bitmap,
        double scaleX,
        double scaleY,
        double focusX,
        double focusY)
    {
        int newWidth = (int)(bitmap.Width * scaleX);
        int newHeight = (int)(bitmap.Height * scaleY);

        var newMap = new ColorSpace[newWidth, newHeight];

        double focusOffsetX = focusX * newWidth;
        double focusOffsetY = focusY * newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                double xCoord = (x - focusOffsetX) / scaleX;
                double yCoord = (y - focusOffsetY) / scaleY;

                float red = 0;
                float green = 0;
                float blue = 0;

                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        int x2 = (int)(xCoord + i);
                        int y2 = (int)(yCoord + j);

                        if (x2 < 0 || x2 >= bitmap.Width || y2 < 0 || y2 >= bitmap.Height)
                        {
                            continue;
                        }

                        float kernelValue = Lanczos3Kernel(i) * Lanczos3Kernel(j);

                        ColorSpace color = bitmap.GetPixel(x2, y2);

                        red += kernelValue * color.First;
                        green += kernelValue * color.Second;
                        blue += kernelValue * color.Third;
                    }
                }

                newMap[x, y] = new ColorSpace(float.Clamp(red, 0, 1), float.Clamp(green, 0, 1),
                    float.Clamp(blue, 0, 1));
            }
        }

        return new PortableBitmap(newMap, bitmap.ColorConverter);
    }

    public static PortableBitmap ScaleBSpline(this
            PortableBitmap bitmap,
        float scaleX,
        float scaleY,
        float focusX,
        float focusY,
        float B = 0,
        float C = 0.5f)
    {
        int newWidth = (int)(bitmap.Width * scaleX);
        int newHeight = (int)(bitmap.Height * scaleY);

        var newMap = new ColorSpace[newWidth, newHeight];

        float focusOffsetX = focusX * newWidth;
        float focusOffsetY = focusY * newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                float xCoord = (x - focusOffsetX) / scaleX;
                float yCoord = (y - focusOffsetY) / scaleY;

                float red = 0;
                float green = 0;
                float blue = 0;

                for (int j = -1; j <= 2; j++)
                {
                    for (int i = -1; i <= 2; i++)
                    {
                        int x2 = (int)(xCoord + i);
                        int y2 = (int)(yCoord + j);

                        if (x2 < 0 || x2 >= bitmap.Width || y2 < 0 || y2 >= bitmap.Height)
                        {
                            continue;
                        }

                        float kernelValue = BSplineKernel(i, B, C) * BSplineKernel(j, B, C);

                        ColorSpace color = bitmap.GetPixel(x2, y2);

                        red += kernelValue * color.First;
                        green += kernelValue * color.Second;
                        blue += kernelValue * color.Third;
                    }
                }

                newMap[x, y] = new ColorSpace(
                    float.Clamp(red, 0, 1),
                    float.Clamp(green, 0, 1),
                    float.Clamp(blue, 0, 1));
            }
        }

        return new PortableBitmap(newMap, bitmap.ColorConverter);
    }

    private static float BSplineKernel(float x, float B, float C)
    {
        if (x < 0)
        {
            x = -x;
        }

        return x switch
        {
            < 1 => ((12 - 9 * B - 6 * C) * x * x * x + (-18 + 12 * B + 6 * C) * x * x + (6 - 2 * B)) / 6,
            < 2 => ((-B - 6 * C) * x * x * x + (6 * B + 30 * C) * x * x + (-12 * B - 48 * C) * x + (8 * B + 24 * C))
                   / 6,
            _ => 0,
        };
    }

    private static float Lanczos3Kernel(float x)
    {
        return x switch
        {
            0 => 1,
            > 3 => 0,
            _ => float.Sin(float.Pi * x) * float.Sin(float.Pi * x / 3f) / (float.Pi * float.Pi * x * x / 3f),
        };
    }
}