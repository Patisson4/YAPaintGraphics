using System;
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
        float gamma,
        bool isFirstChannelVisible = true,
        bool isSecondChannelVisible = true,
        bool isThirdChannelVisible = true)
    {
        Width = map.GetLength(0);
        Height = map.GetLength(1);

        if (Width <= 0 || Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(map), map, "Bitmap cannot be empty");
        }

        IsFirstChannelVisible = isFirstChannelVisible;
        IsSecondChannelVisible = isSecondChannelVisible;
        IsThirdChannelVisible = isThirdChannelVisible;

        ColorConverter = colorConverter;
        Gamma = gamma;

        _map = new ColorSpace[Width, Height];
        Array.Copy(map, _map, map.Length);

        FileLogger.Log("DBG", $"Object created at {FileLogger.SharedTimer.Elapsed.TotalSeconds} s");
    }

    public IColorBaseConverter ColorConverter { get; private set; }
    public float Gamma { get; }

    public int Width { get; }
    public int Height { get; }

    public bool IsFirstChannelVisible { get; private set; }
    public bool IsSecondChannelVisible { get; private set; }
    public bool IsThirdChannelVisible { get; private set; }

    public bool IsInSingleChannelMode =>
        ColorConverter is BlackAndWhite or GreyScale
     || IsFirstChannelVisible && !IsSecondChannelVisible && !IsThirdChannelVisible
     || IsFirstChannelVisible && IsSecondChannelVisible && !IsThirdChannelVisible
     || !IsFirstChannelVisible && !IsSecondChannelVisible && IsThirdChannelVisible;

    public ColorSpace GetPixel(int x, int y)
    {
        ExceptionHelper.ThrowIfGreaterThan(x, Width - 1);
        ExceptionHelper.ThrowIfGreaterThan(y, Height - 1);

        var color = _map[x, y];
        if (IsFirstChannelVisible && IsSecondChannelVisible && IsThirdChannelVisible)
        {
            return color;
        }

        var result = new ColorSpace
        {
            First = IsFirstChannelVisible ? color.First : ColorConverter.Default.First,
            Second = IsSecondChannelVisible ? color.Second : ColorConverter.Default.Second,
            Third = IsThirdChannelVisible ? color.Third : ColorConverter.Default.Third,
        };

        return result;
    }

    public void SetPixel(int x, int y, ColorSpace color)
    {
        ExceptionHelper.ThrowIfGreaterThan(x, Width - 1);
        ExceptionHelper.ThrowIfGreaterThan(y, Height - 1);

        _map[x, y] = color;
    }

    public void ConvertTo(IColorBaseConverter colorConverter)
    {
        // WRN: consider more sophisticated check in the future; for now it's just comparing references of singleton instances
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

    public void ChangeFirstChannelVisibility(bool visibility)
    {
        IsFirstChannelVisible = visibility;
    }

    public void ChangeSecondChannelVisibility(bool visibility)
    {
        IsSecondChannelVisible = visibility;
    }

    public void ChangeThirdChannelVisibility(bool visibility)
    {
        IsThirdChannelVisible = visibility;
    }

    public void SaveRaw(Stream stream)
    {
        int type = 6;
        if (IsInSingleChannelMode)
        {
            type = 5;
        }

        WriteHeader(stream, type, byte.MaxValue);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                WriteBytePixel(stream, GetPixel(x, y));
            }
        }
    }

    public void SavePlain(Stream stream)
    {
        int type = 3;
        if (IsInSingleChannelMode)
        {
            type = 2;
        }

        WriteHeader(stream, type, byte.MaxValue);

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width - 1; x++)
            {
                WritePlainPixel(stream, GetPixel(x, y));
                stream.Write(" "u8);
            }

            WritePlainPixel(stream, GetPixel(Width - 1, y));
            stream.Write("\n"u8);
        }
    }

    private void WriteHeader(Stream stream, int type, int depth = 0)
    {
        stream.Write(Encoding.ASCII.GetBytes($"P{type}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{Width} {Height}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{depth}\n"));
    }

    private void WriteBytePixel(Stream stream, ColorSpace pixel)
    {
        if (ColorConverter is BlackAndWhite or GreyScale
         || IsFirstChannelVisible && !IsSecondChannelVisible && !IsThirdChannelVisible)
        {
            stream.WriteByte(Coefficient.Denormalize(pixel.First));
        }
        else if (!IsFirstChannelVisible && IsSecondChannelVisible && !IsThirdChannelVisible)
        {
            stream.WriteByte(Coefficient.Denormalize(pixel.Second));
        }
        else if (!IsFirstChannelVisible && !IsSecondChannelVisible && IsThirdChannelVisible)
        {
            stream.WriteByte(Coefficient.Denormalize(pixel.Third));
        }
        else
        {
            stream.WriteByte(Coefficient.Denormalize(pixel.First));
            stream.WriteByte(Coefficient.Denormalize(pixel.Second));
            stream.WriteByte(Coefficient.Denormalize(pixel.Third));
        }
    }

    private void WritePlainPixel(Stream stream, ColorSpace pixel)
    {
        if (ColorConverter is BlackAndWhite or GreyScale
         || IsFirstChannelVisible && !IsSecondChannelVisible && !IsThirdChannelVisible)
        {
            stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.First)}"));
        }
        else if (!IsFirstChannelVisible && IsSecondChannelVisible && !IsThirdChannelVisible)
        {
            stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.Second)}"));
        }
        else if (!IsFirstChannelVisible && !IsSecondChannelVisible && IsThirdChannelVisible)
        {
            stream.Write(Encoding.ASCII.GetBytes($"{Coefficient.Denormalize(pixel.Third)}"));
        }
        else
        {
            stream.Write(Encoding.ASCII.GetBytes(pixel.ToString()));
        }
    }
}
