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
            case '5':
                return ReadBinaryGreyscaleImage(reader);
            case '6':
                return ReadBinaryPixelImage(reader);
        }

        return null;
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

}
