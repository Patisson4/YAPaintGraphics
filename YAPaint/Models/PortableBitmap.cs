using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;

namespace YAPaint.Models;

public class PortableBitmap
{
    private readonly ColorSpace[,] _map;

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

        IsFirstVisible = isFirstVisible;
        IsSecondVisible = isSecondVisible;
        IsThirdVisible = isThirdVisible;

        Width = map.GetLength(0);
        Height = map.GetLength(1);
        ColorConverter = colorConverter;

        //consider assigning existing array instead of copying it
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

    public IColorBaseConverter ColorConverter { get; private set; }

    public int Width { get; }
    public int Height { get; }

    public bool IsFirstVisible { get; private set; }
    public bool IsSecondVisible { get; private set; }
    public bool IsThirdVisible { get; private set; }

    public ColorSpace GetPixel(int x, int y)
    {
        CustomExceptionHelper.ThrowIfGreaterThan(x, Width);
        CustomExceptionHelper.ThrowIfGreaterThan(y, Height);

        var color = _map[x, y];

        if (IsFirstVisible && IsSecondVisible && IsThirdVisible)
        {
            return color;
        }

        var result = new ColorSpace(
            IsFirstVisible ? color.First : ColorConverter.DefaultValue.First,
            IsSecondVisible ? color.Second : ColorConverter.DefaultValue.Second,
            IsThirdVisible ? color.Third : ColorConverter.DefaultValue.Third);

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
        IsFirstVisible = !IsFirstVisible;
    }

    public void ToggleSecondChannel()
    {
        IsSecondVisible = !IsSecondVisible;
    }

    public void ToggleThirdChannel()
    {
        IsThirdVisible = !IsThirdVisible;
    }

    public void SaveRaw(Stream stream)
    {
        int type = 6;
        if (ColorConverter is BlackAndWhite or GreyScale)
        {
            type = 5;
        }
        else if (IsFirstVisible && !IsSecondVisible && !IsThirdVisible
              || !IsFirstVisible && IsSecondVisible && !IsThirdVisible
              || !IsFirstVisible && !IsSecondVisible && IsThirdVisible)
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
        else if (IsFirstVisible && !IsSecondVisible && !IsThirdVisible
              || !IsFirstVisible && IsSecondVisible && !IsThirdVisible
              || !IsFirstVisible && !IsSecondVisible && IsThirdVisible)
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
            if (ColorConverter is BlackAndWhite or GreyScale || IsFirstVisible)
            {
                stream.WriteByte(bytePixel[0]);
            }
            else if (IsSecondVisible)
            {
                stream.WriteByte(bytePixel[1]);
            }
            else if (IsThirdVisible)
            {
                stream.WriteByte(bytePixel[2]);
            }
            else
            {
                throw new UnreachableException($"Tried to serialize part of pixel: {pixel.ToPlain()}");
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
            if (ColorConverter is BlackAndWhite or GreyScale || IsFirstVisible)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.First)}"));
            }
            else if (IsSecondVisible)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.Second)}"));
            }
            else if (IsThirdVisible)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.Third)}"));
            }
            else
            {
                throw new UnreachableException($"Tried to serialize part of pixel: {pixel.ToPlain()}");
            }
        }
    }
}
