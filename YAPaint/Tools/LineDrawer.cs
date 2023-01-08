using YAPaint.Models;

namespace YAPaint.Tools;

public static class LineDrawer
{
    public static PortableBitmap DrawLine(
        this PortableBitmap bitmap,
        ColorSpace color,
        int thickness,
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

        bool steep = float.Abs(dy) > float.Abs(dx);
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
        int dy1 = int.Abs(y2 - y1);
        int err = dx1 / 2;
        int step = y1 < y2 ? 1 : -1;
        int y = y1;

        for (int x = x1; x <= x2; x++)
        {
            int centerX = steep ? y : x;
            int centerY = steep ? x : y;
            bitmap = DrawCircle(bitmap, color, thickness, transparency, centerX, centerY);

            err -= dy1;
            if (err >= 0)
            {
                continue;
            }

            y += step;
            err += dx1;
        }

        return bitmap;
    }

    private static PortableBitmap DrawCircle(
        PortableBitmap bitmap,
        ColorSpace color,
        int thickness,
        float transparency,
        int centerX,
        int centerY)
    {
        int radius = thickness / 2;
        for (int x = centerX - radius; x <= centerX + radius; x++)
        {
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height)
                {
                    continue;
                }

                ColorSpace pixelColor = bitmap.GetPixel(x, y);
                var finalColor = new ColorSpace
                {
                    First = (1 - transparency) * pixelColor.First + transparency * color.First,
                    Second = (1 - transparency) * pixelColor.Second + transparency * color.Second,
                    Third = (1 - transparency) * pixelColor.Third + transparency * color.Third,
                };

                bitmap.SetPixel(x, y, finalColor);
            }
        }

        return bitmap;
    }
}
