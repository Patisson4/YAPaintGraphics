using System;
using System.IO;
using System.Text;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;

namespace YAPaint.Models;

public class PortableBitmap
{
    private readonly IColorSpace[,] _map;

    public int Width { get; }
    public int Height { get; }

    public PortableBitmap(IColorSpace[,] map)
    {
        Width = map.GetLength(0);
        Height = map.GetLength(1);

        _map = new IColorSpace[Width, Height];

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                _map[i, j] = map[i, j];
            }
        }
    }

    public PortableBitmap(Stream stream)
    {
        // TODO: create enum with different color spaces?
        throw new NotImplementedException();
    }

    public static PortableBitmap FromStream<T>(Stream stream) where T : IColorSpace
    {
        return PnmParser.ReadImage<T>(stream);
    }

    public IColorSpace GetPixel(int x, int y)
    {
        CustomExceptionHelper.ThrowIfGreaterThan(x, Width);
        CustomExceptionHelper.ThrowIfGreaterThan(y, Height);

        return _map[x, y];
    }

    public void SetPixel(int x, int y, IColorSpace color)
    {
        CustomExceptionHelper.ThrowIfGreaterThan(x, Width);
        CustomExceptionHelper.ThrowIfGreaterThan(y, Height);

        _map[x, y] = color;
    }

    public void SaveRaw(Stream stream)
    {
        //TODO: support empty images
        int format = _map[0, 0] switch
        {
            BlackAndWhite => 4,
            GreyScale => 5,
            Rgb => 6,
            _ => throw new ArgumentOutOfRangeException(
                nameof(_map),
                _map,
                "Unsupported color space"),
        };

        stream.Write(Encoding.ASCII.GetBytes($"P{format}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{Width} {Height}\n"));

        if (format != 4)
        {
            stream.Write(Encoding.ASCII.GetBytes($"{byte.MaxValue}\n"));
        }

        foreach (IColorSpace color in _map)
        {
            stream.Write(color.ToRaw());
        }
    }

    public void SavePlain(Stream stream)
    {
        //TODO: support empty images
        int format = _map[0, 0] switch
        {
            BlackAndWhite => 1,
            GreyScale => 2,
            Rgb => 3,
            _ => throw new ArgumentOutOfRangeException(
                nameof(_map),
                _map,
                "Unsupported color space"),
        };

        stream.Write(Encoding.ASCII.GetBytes($"P{format}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{Width} {Height}\n"));

        if (format != 1)
        {
            stream.Write(Encoding.ASCII.GetBytes($"{byte.MaxValue}\n"));
        }

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width - 1; x++)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{_map[x, y].ToPlain()} "));
            }

            stream.Write(Encoding.ASCII.GetBytes($"{_map[Width - 1, y].ToPlain()}\n"));
        }
    }
}
