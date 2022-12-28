using System;
using System.Globalization;
using System.IO;
using YAPaint.Models;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.Tools;

public static class PnmParser
{
    public static ColorSpace[,] ReadImage(Stream stream)
    {
        using var bufferedStream = new BufferedStream(stream);
        using var reader = new BinaryReader(bufferedStream);

        if (reader.ReadChar() is not 'P')
        {
            throw new NotSupportedException("Unknown format specification");
        }

        char type = reader.ReadChar();

        var width = GetNextTextValue(reader);
        var height = GetNextTextValue(reader);
        var scale = 1;

        if (type is not ('1' or '4'))
        {
            scale = GetNextTextValue(reader);
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

    private static int GetNextTextValue(BinaryReader reader)
    {
        bool hasValue = false;
        string value = string.Empty;

        while (!hasValue)
        {
            if (reader.PeekChar() == -1)
            {
                break;
            }

            var c = reader.ReadChar();

            switch (c)
            {
                case '\n' or '\r' or ' ' or '\t' when value.Length != 0:
                    hasValue = true;
                    break;
                case '\n' or '\r' or ' ' or '\t':
                    break;
                default:
                    value += c;
                    break;
            }
        }

        return int.Parse(value, CultureInfo.InvariantCulture);
    }
}
