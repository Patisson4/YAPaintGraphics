using System.Drawing;
using YAPaint.Core.Exceptions;

namespace YAPaint.Core.Parsers;

public static class PnmParser
{
    public static Image ReadImage(string path)
    {
        using var stream = new FileStream(path, FileMode.Open);
        using var reader = new BinaryReader(stream);

        if (reader.ReadChar() != 'P')
        {
            throw new NotSupportedFormatException("Unknown format specification");
        }

        char type = reader.ReadChar();

        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);
        var scale = 1;

        if (type is '1' or '4')
        {
            scale = GetNextHeaderValue(reader);
        }

        var bitmap = new Bitmap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = ReadColor(reader, type, scale);
                bitmap.SetPixel(x, y, color);
            }
        }

        return bitmap;
    }

    private static Color ReadColor(BinaryReader reader, char type, int scale)
    {
        return type switch
        {
            '1' => ReadTextBitmapColor(reader),
            '2' => ReadTextGreyscaleColor(reader, scale),
            '3' => ReadTextPixelColor(reader, scale),
            '4' => ReadBinaryBitmapColor(reader),
            '5' => ReadBinaryGreyscaleColor(reader, scale),
            '6' => ReadBinaryPixelImage(reader, scale),
            _ => throw new NotSupportedFormatException("Unknown format specification"),
        };
    }

    private static Color ReadTextBitmapColor(BinaryReader reader)
    {
        int bit = GetNextTextValue(reader) == 0 ? 255 : 0;
        return Color.FromArgb(bit, bit, bit);
    }

    private static Color ReadTextGreyscaleColor(BinaryReader reader, int scale)
    {
        var grey = GetNextTextValue(reader) * 255 / scale;
        return Color.FromArgb(grey, grey, grey);
    }

    private static Color ReadTextPixelColor(BinaryReader reader, int scale)
    {
        var red = GetNextTextValue(reader) * 255 / scale;
        var green = GetNextTextValue(reader) * 255 / scale;
        var blue = GetNextTextValue(reader) * 255 / scale;

        return Color.FromArgb(red, green, blue);
    }

    private static Color ReadBinaryBitmapColor(BinaryReader reader)
    {
        var bit = reader.ReadByte() == 0 ? 255 : 0;
        return Color.FromArgb(bit, bit, bit);
    }

    private static Color ReadBinaryGreyscaleColor(BinaryReader reader, int scale)
    {
        var grey = reader.ReadByte() * 255 / scale;
        return Color.FromArgb(grey, grey, grey);
    }

    private static Color ReadBinaryPixelImage(BinaryReader reader, int scale)
    {
        var red = reader.ReadByte() * 255 / scale;
        var green = reader.ReadByte() * 255 / scale;
        var blue = reader.ReadByte() * 255 / scale;

        return Color.FromArgb(red, green, blue);
    }

    private static int GetNextHeaderValue(BinaryReader reader)
    {
        var hasValue = false;
        var value = string.Empty;

        do
        {
            var c = reader.ReadChar();

            if (hasValue) continue;
            switch (c)
            {
                case '\n' or ' ' or '\t' when value.Length != 0:
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
        var value = string.Empty;
        var c = reader.ReadChar();

        do
        {
            value += c;

            c = reader.ReadChar();
        } while (c is not ('\n' or ' ' or '\t') || value.Length == 0);

        return int.Parse(value);
    }
}
