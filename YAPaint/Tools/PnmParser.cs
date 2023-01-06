using System;
using System.Globalization;
using System.IO;
using YAPaint.Models;
using YAPaint.Models.ColorSpaces;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace YAPaint.Tools;

public static class PnmParser
{
    public static PortableBitmap ReadImage(Stream stream, IColorBaseConverter converter)
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

        FileLogger.Log("DBG", $"Read file at {FileLogger.SharedTimer.Elapsed.TotalSeconds} s");

        return new PortableBitmap(map, converter, -1);
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
        float bit = GetNextTextValue(reader) == 0 ? 1 : 0;
        return new ColorSpace { First = bit, Second = bit, Third = bit };
    }

    private static ColorSpace ReadTextGreyscaleColor(BinaryReader reader, int scale)
    {
        float grey = (float)GetNextTextValue(reader) / scale;
        return new ColorSpace { First = grey, Second = grey, Third = grey };
    }

    private static ColorSpace ReadTextPixelColor(BinaryReader reader, int scale)
    {
        float red = (float)GetNextTextValue(reader) / scale;
        float green = (float)GetNextTextValue(reader) / scale;
        float blue = (float)GetNextTextValue(reader) / scale;

        return new ColorSpace { First = red, Second = green, Third = blue };
    }

    private static ColorSpace ReadBinaryBitmapColor(BinaryReader reader)
    {
        float bit = reader.ReadByte() == 0 ? 1 : 0;
        return new ColorSpace { First = bit, Second = bit, Third = bit };
    }

    private static ColorSpace ReadBinaryGreyscaleColor(BinaryReader reader, int scale)
    {
        float grey = (float)reader.ReadByte() / scale;
        return new ColorSpace { First = grey, Second = grey, Third = grey };
    }

    private static ColorSpace ReadBinaryPixelImage(BinaryReader reader, int scale)
    {
        float red = (float)reader.ReadByte() / scale;
        float green = (float)reader.ReadByte() / scale;
        float blue = (float)reader.ReadByte() / scale;

        return new ColorSpace { First = red, Second = green, Third = blue };
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
