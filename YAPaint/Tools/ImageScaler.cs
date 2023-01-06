using YAPaint.Models;

namespace YAPaint.Tools;

public static class ImageScaler
{
    public static PortableBitmap ScaleNearestNeighbor(
        this PortableBitmap bitmap,
        float scaleX,
        float scaleY,
        float focusX,
        float focusY)
    {
        int scaledWidth = (int)(scaleX * bitmap.Width);
        int scaledHeight = (int)(scaleY * bitmap.Height);
        var scaledMap = new ColorSpace[scaledWidth, scaledHeight];

        float offsetX = focusX * scaleX * bitmap.Width;
        float offsetY = focusY * scaleY * bitmap.Height;

        for (int j = 0; j < scaledHeight; j++)
        {
            for (int i = 0; i < scaledWidth; i++)
            {
                int x = (int)((i + offsetX) / scaleX);
                int y = (int)((j + offsetY) / scaleY);

                x = int.Clamp(x, 0, bitmap.Width - 1);
                y = int.Clamp(y, 0, bitmap.Height - 1);

                scaledMap[i, j] = bitmap.GetPixel(x, y);
            }
        }

        return new PortableBitmap(
            scaledMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap ScaleBilinear(
        this PortableBitmap bitmap,
        float scaleX,
        float scaleY,
        float focusX,
        float focusY)
    {
        int scaledWidth = (int)(bitmap.Width * scaleX);
        int scaledHeight = (int)(bitmap.Height * scaleY);

        var scaledMap = new ColorSpace[scaledWidth, scaledHeight];

        float offsetX = focusX * scaleX * bitmap.Width;
        float offsetY = focusY * scaleY * bitmap.Height;

        for (int y = 0; y < scaledHeight; y++)
        {
            for (int x = 0; x < scaledWidth; x++)
            {
                float srcX = (x + offsetX) / scaleX;
                float srcY = (y + offsetY) / scaleY;

                int srcX1 = (int)float.Floor(srcX);
                int srcY1 = (int)float.Floor(srcY);
                int srcX2 = (int)float.Floor(srcX) + 1;
                int srcY2 = (int)float.Floor(srcY) + 1;

                srcX1 = int.Clamp(srcX1, 0, bitmap.Width - 1);
                srcY1 = int.Clamp(srcY1, 0, bitmap.Height - 1);
                srcX2 = int.Clamp(srcX2, 0, bitmap.Width - 1);
                srcY2 = int.Clamp(srcY2, 0, bitmap.Height - 1);

                scaledMap[x, y] = Interpolate(bitmap, srcX, srcY, srcX1, srcX2, srcY1, srcY2);
            }
        }

        return new PortableBitmap(
            scaledMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    private static ColorSpace Interpolate(
        PortableBitmap bitmap,
        float srcX,
        float srcY,
        int srcX1,
        int srcX2,
        int srcY1,
        int srcY2)
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

        return new ColorSpace
        {
            First = float.Clamp(r, 0, 1),
            Second = float.Clamp(g, 0, 1),
            Third = float.Clamp(b, 0, 1),
        };
    }

    public static PortableBitmap ScaleLanczos3(
        this PortableBitmap bitmap,
        double scaleX,
        double scaleY,
        double focusX,
        double focusY)
    {
        int scaledWidth = (int)(bitmap.Width * scaleX);
        int scaledHeight = (int)(bitmap.Height * scaleY);
        var scaledMap = new ColorSpace[scaledWidth, scaledHeight];

        double offsetX = focusX * scaledWidth;
        double offsetY = focusY * scaledHeight;

        for (int y = 0; y < scaledHeight; y++)
        {
            for (int x = 0; x < scaledWidth; x++)
            {
                double xCoord = (x + offsetX) / scaleX;
                double yCoord = (y + offsetY) / scaleY;

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

                scaledMap[x, y] = new ColorSpace
                {
                    First = float.Clamp(red, 0, 1),
                    Second = float.Clamp(green, 0, 1),
                    Third = float.Clamp(blue, 0, 1),
                };
            }
        }

        return new PortableBitmap(
            scaledMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    public static PortableBitmap ScaleBcSpline(
        this PortableBitmap bitmap,
        float scaleX,
        float scaleY,
        float focusX,
        float focusY,
        float B = 0,
        float C = 0.5f)
    {
        int scaledWidth = (int)(bitmap.Width * scaleX);
        int scaledHeight = (int)(bitmap.Height * scaleY);
        var scaledMap = new ColorSpace[scaledWidth, scaledHeight];

        float offsetX = focusX * scaledWidth;
        float offsetY = focusY * scaledHeight;

        for (int y = 0; y < scaledHeight; y++)
        {
            for (int x = 0; x < scaledWidth; x++)
            {
                float xCoord = (x + offsetX) / scaleX;
                float yCoord = (y + offsetY) / scaleY;

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

                        float kernelValue = BcSplineKernel(i, B, C) * BcSplineKernel(j, B, C);

                        ColorSpace color = bitmap.GetPixel(x2, y2);

                        red += kernelValue * color.First;
                        green += kernelValue * color.Second;
                        blue += kernelValue * color.Third;
                    }
                }

                scaledMap[x, y] = new ColorSpace
                {
                    First = float.Clamp(red, 0, 1),
                    Second = float.Clamp(green, 0, 1),
                    Third = float.Clamp(blue, 0, 1),
                };
            }
        }

        return new PortableBitmap(
            scaledMap,
            bitmap.ColorConverter,
            bitmap.Gamma,
            bitmap.IsFirstChannelVisible,
            bitmap.IsSecondChannelVisible,
            bitmap.IsThirdChannelVisible);
    }

    private static float BcSplineKernel(float x, float B, float C)
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
