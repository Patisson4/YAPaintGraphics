using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YAPaint.Models;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.Tools;

public static class PnmParser
{
    public static ColorSpace[,] ReadImage(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        if (reader.ReadChar() is not 'P')
        {
            throw new NotSupportedException("Unknown format specification");
        }

        char type = reader.ReadChar();

        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);
        var scale = 1;

        if (type is not ('1' or '4'))
        {
            scale = GetNextHeaderValue(reader);
        }

        var map = new ColorSpace[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = ReadColor(reader, type, scale);
            }
        }

        MyFileLogger.Log("DBG", $"Read file at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s");

        return map;
    }

    public static async Task WriteTextImage(this Bitmap bitmap, string path)
    {
        await using var file = new StreamWriter(path);
        await file.WriteLineAsync($"P3\n{bitmap.Width} {bitmap.Height}\n255");

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width - 1; x++)
            {
                Color color = bitmap.GetPixel(x, y);
                await file.WriteAsync($"{color.R} {color.G} {color.B} ");
            }

            {
                Color color = bitmap.GetPixel(bitmap.Width - 1, y);
                await file.WriteLineAsync($"{color.R} {color.G} {color.B}");
            }
        }
    }

    public static async Task WriteRawImage(this Bitmap bitmap, string path)
    {
        await using FileStream file = File.OpenWrite(path);
        await file.WriteAsync(Encoding.ASCII.GetBytes($"P6\n{bitmap.Width} {bitmap.Height}\n255\n"));

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color color = bitmap.GetPixel(x, y);
                file.WriteByte(color.R);
                file.WriteByte(color.G);
                file.WriteByte(color.B);
            }
        }
    }

    private static ColorSpace ReadColor(BinaryReader reader, char type, int scale)
    {
        return type switch
        {
            '1' => ReadTextBitmapColor(reader),
            '2' => ReadTextGreyscaleColor(reader, scale),
            '3' => ReadTextPixelColor(reader, scale),
            '4' => ReadBinaryBitmapColor(reader),
            '5' => ReadBinaryGreyscaleColor(reader, scale),
            '6' => ReadBinaryPixelImage(reader, scale),
            _ => throw new NotSupportedException("Unknown format specification"),
        };
    }

    private static ColorSpace ReadTextBitmapColor(BinaryReader reader)
    {
        var bit = GetNextTextValue(reader) == 0 ? 255 : 0;
        return new ColorSpace(Coefficient.Normalize(bit), Coefficient.Normalize(bit), Coefficient.Normalize(bit));
    }

    private static ColorSpace ReadTextGreyscaleColor(BinaryReader reader, int scale)
    {
        var grey = GetNextTextValue(reader) * 255 / scale;
        return new ColorSpace(Coefficient.Normalize(grey), Coefficient.Normalize(grey), Coefficient.Normalize(grey));
    }

    private static ColorSpace ReadTextPixelColor(BinaryReader reader, int scale)
    {
        var red = GetNextTextValue(reader) * 255 / scale;
        var green = GetNextTextValue(reader) * 255 / scale;
        var blue = GetNextTextValue(reader) * 255 / scale;

        return new ColorSpace(Coefficient.Normalize(red), Coefficient.Normalize(green), Coefficient.Normalize(blue));
    }

    private static ColorSpace ReadBinaryBitmapColor(BinaryReader reader)
    {
        var bit = reader.ReadByte() == 0 ? 255 : 0;
        return new ColorSpace(Coefficient.Normalize(bit), Coefficient.Normalize(bit), Coefficient.Normalize(bit));
    }

    private static ColorSpace ReadBinaryGreyscaleColor(BinaryReader reader, int scale)
    {
        var grey = reader.ReadByte() * 255 / scale;
        return new ColorSpace(Coefficient.Normalize(grey), Coefficient.Normalize(grey), Coefficient.Normalize(grey));
    }

    private static ColorSpace ReadBinaryPixelImage(BinaryReader reader, int scale)
    {
        var red = reader.ReadByte() * 255 / scale;
        var green = reader.ReadByte() * 255 / scale;
        var blue = reader.ReadByte() * 255 / scale;

        return new ColorSpace(Coefficient.Normalize(red), Coefficient.Normalize(green), Coefficient.Normalize(blue));
    }

    private static int GetNextHeaderValue(BinaryReader reader)
    {
        bool hasValue = false;
        string value = string.Empty;

        do
        {
            var c = reader.ReadChar();

            switch (c)
            {
                case '\n' or '\r' or ' ' or '\t' when value.Length != 0:
                    hasValue = true;
                    break;
                case >= '0' and <= '9':
                    value += c;
                    break;
            }
        } while (!hasValue);

        return int.Parse(value);
    }

    private static int GetNextTextValue(BinaryReader reader)
    {
        string value = string.Empty;
        var c = reader.ReadChar();

        do
        {
            value += c;
            try
            {
                c = reader.ReadChar();
            }
            catch (EndOfStreamException)
            {
                break;
            }
        } while (c is not ('\n' or '\r' or ' ' or '\t') || value.Length == 0);

        return int.Parse(value);
    }
}
