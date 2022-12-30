using System;
using YAPaint.Models;

namespace YAPaint.Tools;

public static class ImageScaler
{
    public static PortableBitmap ScaleNearestNeighbor(this PortableBitmap bitmap, float scaleX, float scaleY,
        float focalPointX, float focalPointY)
    {
        var newBitmap = new PortableBitmap(new ColorSpace[(int)(scaleX * bitmap.Width), (int)(scaleY *
                bitmap.Height)],
            bitmap.ColorConverter,
            true, true, true);

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
        // Вычисляем новые размеры изображения
        int newWidth = (int)(bitmap.Width * scaleX);
        int newHeight = (int)(bitmap.Height * scaleY);

        // Создаем новое изображение с новыми размерами
        var newBitmap = new PortableBitmap(new ColorSpace[newWidth, newHeight],
            bitmap.ColorConverter,
            true, true, true);

        // Смещение центра масштабирования
        float offsetX = focalPointX * scaleX * bitmap.Width;
        float offsetY = focalPointY * scaleY * bitmap.Height;

        // Масштабируем каждый пиксель в новое изображение
        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // Вычисляем координаты исходного пикселя
                float srcX = (x + offsetX) / scaleX;
                float srcY = (y + offsetY) / scaleY;

                // Округляем координаты до целых чисел
                int srcX1 = (int)Math.Floor(srcX);
                int srcY1 = (int)Math.Floor(srcY);
                int srcX2 = (int)Math.Floor(srcX) + 1;
                int srcY2 = (int)Math.Floor(srcY) + 1;

                // Проверяем, что координаты не выходят за границы изображения
                srcX1 = Math.Max(0, Math.Min(srcX1, bitmap.Width - 1));
                srcY1 = Math.Max(0, Math.Min(srcY1, bitmap.Height - 1));
                srcX2 = Math.Max(0, Math.Min(srcX2, bitmap.Width - 1));
                srcY2 = Math.Max(0, Math.Min(srcY2, bitmap.Height - 1));

                // Интерполируем цвет пикселя
                ColorSpace interpolatedColor = Interpolate(bitmap, srcX, srcY, srcX1, srcX2, srcY1, srcY2);

                // Устанавливаем цвет пикселя в новом изображении
                newBitmap.SetPixel(x, y, interpolatedColor);
            }
        }

        return newBitmap;
    }

    private static ColorSpace Interpolate(PortableBitmap bitmap, float srcX, float srcY, int srcX1, int srcX2,
        int srcY1, int srcY2)
    {
        // Получаем цвета пикселей
        ColorSpace c1 = bitmap.GetPixel(srcX1, srcY1);
        ColorSpace c2 = bitmap.GetPixel(srcX2, srcY1);
        ColorSpace c3 = bitmap.GetPixel(srcX1, srcY2);
        ColorSpace c4 = bitmap.GetPixel(srcX2, srcY2);

        // Вычисляем веса
        float w1 = (srcX2 - srcX) * (srcY2 - srcY);
        float w2 = (srcX - srcX1) * (srcY2 - srcY);
        float w3 = (srcX2 - srcX) * (srcY - srcY1);
        float w4 = (srcX - srcX1) * (srcY - srcY1);

        // Билинейное интерполирование цвета
        float r = c1.First * w1 + c2.First * w2 + c3.First * w3 + c4.First * w4;
        float g = c1.Second * w1 + c2.Second * w2 + c3.Second * w3 + c4.Second * w4;
        float b = c1.Third * w1 + c2.Third * w2 + c3.Third * w3 + c4.Third * w4;

        // Возвращаем результат
        return new ColorSpace(Math.Clamp(r, 0, 1), Math.Clamp(g, 0, 1), Math.Clamp(b, 0, 1));
    }

    public static PortableBitmap ScaleLanczos3(this PortableBitmap bitmap, float xScale, float yScale,
        float focalPointX, float focalPointY)
    {
        var newBitmap = new PortableBitmap(new ColorSpace[(int)(xScale * bitmap.Width), (int)(yScale * bitmap.Height)],
            bitmap.ColorConverter,
            true, true, true);

        var focusX = focalPointX * xScale;
        var focusY = focalPointY * yScale;

        for (var y = 0; y < newBitmap.Height; y++)
        {
            for (var x = 0; x < newBitmap.Width; x++)
            {
                var sourceX = x / xScale;
                var sourceY = y / yScale;

                var distance =
                    Math.Sqrt((sourceX - focusX) * (sourceX - focusX) + (sourceY - focusY) * (sourceY - focusY));

                if (!(distance < 3))
                    continue;
                var filter = Lanczos3(distance);

                var sourceColor = bitmap.GetPixel((int)sourceX, (int)sourceY);

                var newR = (float)Math.Clamp(sourceColor.First * (filter + 1),
                    0, 1);
                var newG = (float)Math.Clamp(sourceColor.Second * (filter + 1),
                    0, 1);
                var newB = (float)Math.Clamp(sourceColor.Third * (filter + 1),
                    0, 1);

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