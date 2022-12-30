using System;
using System.Drawing;
using YAPaint.Models;
using Point = Avalonia.Point;

namespace YAPaint.Tools;

public static class ImageScaler
{
    public static PortableBitmap ScaleNearestNeighbor(this PortableBitmap bitmap, float newWidth, float newHeight, float focalPointX, float focalPointY)
    {
        var newBitmap = new PortableBitmap(new ColorSpace[(int)(newWidth * bitmap.Width), (int)(newHeight *
                bitmap.Height)],
            bitmap.ColorConverter,
            true, true, true);
        
        var denormalizedNewWidth = Coefficient.Denormalize(newWidth);
        var denormalizedNewHeight = Coefficient.Denormalize(newHeight);
        
        var scaleX = bitmap.Width / denormalizedNewWidth;
        var scaleY = bitmap.Height / denormalizedNewHeight;

        var denormalizedFocalPointX = focalPointX * bitmap.Width;
        var denormalizedFocalPointY = focalPointY * bitmap.Height;
        
        for (var j = 0; j < bitmap.Height; j++)
        {
            for (var i = 0; i < bitmap.Width; i++)
            {
                var x = (int)((i - denormalizedFocalPointX) * scaleX + denormalizedFocalPointX);
                var y = (int)((j - denormalizedFocalPointY) * scaleY + denormalizedFocalPointY);

                x = Math.Clamp(x, 0, denormalizedNewWidth - 1);
                y = Math.Clamp(y, 0, denormalizedNewHeight - 1);

                newBitmap.SetPixel(i, j, bitmap.GetPixel(x, y));
            }
        }

        return newBitmap;
    }


    public static PortableBitmap ScaleBilinear(this PortableBitmap bitmap, float newWidth, float newHeight,
        Point focalPoint)
    {
        var newBitmap = new PortableBitmap(new ColorSpace[(int)newWidth * bitmap.Width, (int)newHeight * bitmap.Height],
            bitmap.ColorConverter,
            true, true, true);

        var scaleX = (float)newWidth / bitmap.Width;
        var scaleY = (float)newHeight / bitmap.Height;

        var newFocalPoint = new Point((float)(focalPoint.X * scaleX), (float)(focalPoint.Y * scaleY));

        for (var x = 0; x < newWidth; x++)
        {
            for (var y = 0; y < newHeight; y++)
            {
                var srcX = (float)((x - newFocalPoint.X) / scaleX + focalPoint.X);
                var srcY = (float)((x - newFocalPoint.X) / scaleX + focalPoint.X);
                var srcX1 = srcX;
                var srcX2 = srcX1 + 1;
                var srcY1 = srcY;
                var srcY2 = srcY1 + 1;

                if (srcX2 >= bitmap.Width || srcY2 >= bitmap.Height)
                {
                    continue;
                }

                var c1 = bitmap.GetPixel((int)(srcX1 * bitmap.Width), (int)srcY1 * bitmap.Height);
                var c2 = bitmap.GetPixel((int)(srcX2 * bitmap.Width), (int)srcY1 * bitmap.Height);
                var c3 = bitmap.GetPixel((int)(srcX1 * bitmap.Width), (int)srcY2 * bitmap.Height);
                var c4 = bitmap.GetPixel((int)(srcX2 * bitmap.Width), (int)srcY2 * bitmap.Height);

                var w1 = (srcX2 - srcX) * (srcY2 - srcY);
                var w2 = (srcX - srcX1) * (srcY2 - srcY);
                var w3 = (srcX2 - srcX) * (srcY - srcY1);
                var w4 = (srcX - srcX1) * (srcY - srcY1);

                var r = c1.First * w1 + c2.First * w2 + c3.First * w3 + c4.First * w4;
                var g = c1.Second * w1 + c2.Second * w2 + c3.Second * w3 + c4.Second * w4;
                var b = c1.Third * w1 + c2.Third * w2 + c3.Third * w3 + c4.Third * w4;

                newBitmap.SetPixel(x, y,
                    new ColorSpace(Coefficient.Normalize((int)r), Coefficient.Normalize((int)g),
                        Coefficient.Normalize((int)b)));
            }
        }

        return newBitmap;
    }

    public static PortableBitmap ScaleLanczos3(this PortableBitmap bitmap, float newWidth, float newHeight,
        Point focalPoint)
    {
        var newBitmap = new PortableBitmap(new ColorSpace[(int)newWidth * bitmap.Width, (int)newHeight * bitmap.Height],
            bitmap.ColorConverter,
            true, true, true);

        var xScale = (float)newWidth / bitmap.Width;
        var yScale = (float)newHeight / bitmap.Height;

        var focusX = (float)(focalPoint.X * xScale);
        var focusY = (float)(focalPoint.Y * yScale);

        for (var y = 0; y < newHeight; y++)
        {
            for (var x = 0; x < newWidth; x++)
            {
                var sourceX = x / xScale;
                var sourceY = y / yScale;

                var distance =
                    Math.Sqrt((sourceX - focusX) * (sourceX - focusX) + (sourceY - focusY) * (sourceY - focusY));

                if (!(distance < 3))
                    continue;
                var filter = Lanczos3(distance);

                var sourceColor = bitmap.GetPixel((int)sourceX, (int)sourceY);

                var currentColor = newBitmap.GetPixel(x, y);

                var newR = (float)Math.Clamp(
                    Coefficient.Denormalize(currentColor.First) + Coefficient.Denormalize(sourceColor.First) * filter,
                    0, 255);
                var newG = (float)Math.Clamp(
                    Coefficient.Denormalize(currentColor.Second) + Coefficient.Denormalize(sourceColor.Second) * filter,
                    0, 255);
                var newB = (float)Math.Clamp(
                    Coefficient.Denormalize(currentColor.Third) + Coefficient.Denormalize(sourceColor.Third) * filter,
                    0, 255);

                newBitmap.SetPixel(x, y, new ColorSpace(newR, newG, newB));
            }
        }

        return newBitmap;
    }

    private static double Lanczos3(double x)
    {
        return x switch
        {
            0 => 1,
            > 3 => 0,
            _ => Math.Sin(Math.PI * x) * Math.Sin(Math.PI * x / 3) / (Math.PI * Math.PI * x * x / 3)
        };
    }
}