using System;
using System.IO;
using System.Text;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;

namespace YAPaint.Models;

public class PortableBitmap
{
    private readonly ColorSpace[,] _map;

    private bool _isFirstVisible;
    private bool _isSecondVisible;
    private bool _isThirdVisible;

    public IColorBaseConverter ColorConverter { get; private set; }
    public int Width { get; }
    public int Height { get; }

    public PortableBitmap(
        ColorSpace[,] map,
        IColorBaseConverter colorConverter,
        bool isFirstVisible,
        bool isSecondVisible,
        bool isThirdVisible)
    {
        if (map.Length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(map), map, "Bitmap cannot be empty");
        }

        _isFirstVisible = isFirstVisible;
        _isSecondVisible = isSecondVisible;
        _isThirdVisible = isThirdVisible;

        Width = map.GetLength(0);
        Height = map.GetLength(1);
        ColorConverter = colorConverter;

        _map = new ColorSpace[Width, Height];

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                _map[i, j] = map[i, j];
            }
        }

        MyFileLogger.Log("DBG", $"Object created at {MyFileLogger.SharedTimer.Elapsed.TotalSeconds} s");
    }

    public ColorSpace GetPixel(int x, int y)
    {
        CustomExceptionHelper.ThrowIfGreaterThan(x, Width);
        CustomExceptionHelper.ThrowIfGreaterThan(y, Height);

        var color = _map[x, y];

        if (_isFirstVisible && _isSecondVisible && _isThirdVisible)
        {
            return color;
        }

        var result = new ColorSpace(
            _isFirstVisible ? color.First : 0f,
            _isSecondVisible ? color.Second : 0f,
            _isThirdVisible ? color.Third : 0f);

        return result;
    }

    public void SetPixel(int x, int y, ColorSpace color)
    {
        CustomExceptionHelper.ThrowIfGreaterThan(x, Width);
        CustomExceptionHelper.ThrowIfGreaterThan(y, Height);

        _map[x, y] = color;
    }

    public void ConvertTo(IColorBaseConverter colorConverter)
    {
        //consider more sophisticated check in the future
        if (ColorConverter == colorConverter)
        {
            return;
        }

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                var rgbColor = ColorConverter.ToRgb(ref _map[i, j]);
                _map[i, j] = colorConverter.FromRgb(ref rgbColor);
            }
        }

        ColorConverter = colorConverter;
    }

    public void ToggleFirstChannel()
    {
        _isFirstVisible = !_isFirstVisible;
    }

    public void ToggleSecondChannel()
    {
        _isSecondVisible = !_isSecondVisible;
    }

    public void ToggleThirdChannel()
    {
        _isThirdVisible = !_isThirdVisible;
    }

    public void SaveRaw(Stream stream)
    {
        int type = 6;
        if (ColorConverter is BlackAndWhite or GreyScale)
        {
            type = 5;
        }
        else if (_isFirstVisible && !_isSecondVisible && !_isThirdVisible
              || !_isFirstVisible && _isSecondVisible && !_isThirdVisible
              || !_isFirstVisible && !_isSecondVisible && _isThirdVisible)
        {
            type = 5;
        }

        bool byPart = type == 5;
        WriteHeader(stream, type, byte.MaxValue);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                WriteBytePixel(stream, GetPixel(x, y), byPart);
            }
        }
    }

    public void SavePlain(Stream stream)
    {
        int type = 3;
        if (ColorConverter is BlackAndWhite or GreyScale)
        {
            type = 2;
        }
        else if (_isFirstVisible && !_isSecondVisible && !_isThirdVisible
              || !_isFirstVisible && _isSecondVisible && !_isThirdVisible
              || !_isFirstVisible && !_isSecondVisible && _isThirdVisible)
        {
            type = 2;
        }

        bool byPart = type == 2;
        WriteHeader(stream, type, byte.MaxValue);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width - 1; x++)
            {
                WritePlainPixel(stream, GetPixel(x, y), byPart);
                stream.Write(Encoding.ASCII.GetBytes(" "));
            }

            WritePlainPixel(stream, GetPixel(Width - 1, y), byPart);
            stream.Write(Encoding.ASCII.GetBytes("\n"));
        }
    }

    private void WriteHeader(Stream stream, int type, int depth = 0)
    {
        stream.Write(Encoding.ASCII.GetBytes($"P{type}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{Width} {Height}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{depth}\n"));
    }

    private void WriteBytePixel(Stream stream, ColorSpace pixel, bool byPart = true)
    {
        var bytePixel = pixel.ToRaw();
        if (!byPart)
        {
            stream.Write(bytePixel);
        }
        else
        {
            if (ColorConverter is BlackAndWhite or GreyScale || _isFirstVisible)
            {
                stream.WriteByte(bytePixel[0]);
            }
            else if (_isSecondVisible)
            {
                stream.WriteByte(bytePixel[1]);
            }
            else
            {
                stream.WriteByte(bytePixel[2]);
            }
        }
    }

    private void WritePlainPixel(Stream stream, ColorSpace pixel, bool byPart = true)
    {
        if (!byPart)
        {
            stream.Write(Encoding.ASCII.GetBytes($"{pixel.ToPlain()}"));
        }
        else
        {
            if (ColorConverter is BlackAndWhite or GreyScale || _isFirstVisible)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.First)}"));
            }
            else if (_isSecondVisible)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.Second)}"));
            }
            else
            {
                stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.Third)}"));
            }
        }
    }
}
