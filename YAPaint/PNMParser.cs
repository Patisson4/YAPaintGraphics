using System.Drawing;
using System.IO;

namespace YAPaint;

public class PnmParser
{
    public Image ReadImage(string path)
    {
        using var reader = new BinaryReader(new FileStream(path, FileMode.Open));
        if (reader.ReadChar() != 'P') return null;
        var c = reader.ReadChar();

        switch (c)
        {
            case '1':
                return ReadTextBitmapImage(reader);
            case '2':
                return ReadTextGreyscaleImage(reader);
            case '3':
                return ReadTextPixelImage(reader);
            case '4': 
                return ReadBinaryBitmapImage(reader);
            case '5':
                return ReadBinaryGreyscaleImage(reader);
            case '6':
                return ReadBinaryPixelImage(reader);
        }

        return null;
    }

    private Image ReadTextBitmapImage(BinaryReader reader)
    {
        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);

        var bitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var bit = GetNextTextValue(reader) == 0 ? 255 : 0;

                bitmap.SetPixel(x, y, Color.FromArgb(bit, bit, bit));
            }
        }

        return bitmap;
    }

    private Image ReadTextGreyscaleImage(BinaryReader reader)
    {
        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);
        var scale = GetNextHeaderValue(reader);

        var bitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var grey = GetNextTextValue(reader) * 255 / scale;

                bitmap.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
            }
        }

        return bitmap;
    }

    private Image ReadTextPixelImage(BinaryReader reader)
    {
        char c;

        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);
        var scale = GetNextHeaderValue(reader);

        var bitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var red = GetNextTextValue(reader) * 255 / scale;
                var green = GetNextTextValue(reader) * 255 / scale;
                var blue = GetNextTextValue(reader) * 255 / scale;

                bitmap.SetPixel(x, y, Color.FromArgb(red, green, blue));
            }
        }

        return bitmap;
    }

    private Image ReadBinaryBitmapImage(BinaryReader reader)
    {
        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);

        var bitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var bit = reader.ReadByte() == 0 ? 255 : 0;

                bitmap.SetPixel(x, y, Color.FromArgb(bit, bit, bit));
            }
        }

        return bitmap;
    }

    private Image ReadBinaryGreyscaleImage(BinaryReader reader)
    {
        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);
        var scale = GetNextHeaderValue(reader);

        var bitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var grey = reader.ReadByte() * 255 / scale;

                bitmap.SetPixel(x, y, Color.FromArgb(grey, grey, grey));
            }
        }

        return bitmap;
    }

    private Image ReadBinaryPixelImage(BinaryReader reader)
    {
        var width = GetNextHeaderValue(reader);
        var height = GetNextHeaderValue(reader);
        var scale = GetNextHeaderValue(reader);

        var bitmap = new Bitmap(width, height);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var red = reader.ReadByte() * 255 / scale;
                var green = reader.ReadByte() * 255 / scale;
                var blue = reader.ReadByte() * 255 / scale;

                bitmap.SetPixel(x, y, Color.FromArgb(red, green, blue));
            }
        }

        return bitmap;
    }

    private int GetNextHeaderValue(BinaryReader reader)
    {
        var hasValue = false;
        var value = string.Empty;
        char c;
        var comment = false;

        do
        {
            c = reader.ReadChar();

            if (c == '#')
            {
                comment = true;
            }

            if (comment)
            {
                if (c == '\n')
                {
                    comment = false;
                }

                continue;
            }

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

    private int GetNextTextValue(BinaryReader reader)
    {
        var value = string.Empty;
        var c = reader.ReadChar();

        do
        {
            value += c;

            c = reader.ReadChar();
        } while (!(c is '\n' or ' ' or '\t') || value.Length == 0);

        return int.Parse(value);
    }
}
