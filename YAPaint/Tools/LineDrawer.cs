using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class LineDrawer
{
    public static PortableBitmap DrawLine(
        this PortableBitmap image,
        ColorSpace color,
        float thickness,
        float transparency,
        (int x, int y) start,
        (int x, int y) end)
    {
        int dx = end.x - start.x;
        int dy = end.y - start.y;
        
        int x1 = start.x;
        int y1 = start.y;
        int x2 = end.x;
        int y2 = end.y;

        bool steep = Math.Abs(dy) > Math.Abs(dx);
        if (steep)
        {
            int t = x1;
            x1 = y1;
            y1 = t;
            t = x2;
            x2 = y2;
            y2 = t;
        }

        if (x1 > x2)
        {
            int t = x1;
            x1 = x2;
            x2 = t;
            t = y1;
            y1 = y2;
            y2 = t;
        }

        int dx1 = x2 - x1;
        int dy1 = Math.Abs(y2 - y1);
        int err = dx1 / 2;
        int ystep = y1 < y2 ? 1 : -1;
        int y = y1;

        for (int x = x1; x <= x2; x++)
        {
            int centerX = steep ? y : x;
            int centerY = steep ? x : y;
            DrawCircle(image, color, thickness, transparency, centerX, centerY);

            err -= dy1;
            if (err < 0)
            {
                y += ystep;
                err += dx1;
            }
        }

        return image;
    }
    
    private static void DrawCircle(
        PortableBitmap image,
        ColorSpace color,
        float thickness,
        float transparency,
        int centerX,
        int centerY)
    {
        int radius = (int)thickness / 2;
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                {
                    ColorSpace pixelColor = image.GetPixel(x, y);
                    byte[] pixelRaw = pixelColor.ToRaw();
                    byte[] colorRaw = color.ToRaw();
                    byte[] finalRaw = new byte[3];
                    for (int i = 0; i < 3; i++)
                    {
                        finalRaw[i] = (byte)((1 - transparency) * pixelRaw[i] + transparency * colorRaw[i]);
                    }
                    ColorSpace finalColor = new ColorSpace(finalRaw[0], finalRaw[1], finalRaw[2]);
                }
            }
        }
    }
}