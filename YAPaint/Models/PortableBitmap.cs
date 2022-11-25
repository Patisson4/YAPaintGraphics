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

        if (map.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(map), map, "Bitmap cannot be empty");
        }

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

    public static PortableBitmap FromStream<T>(Stream stream) where T : IColorSpace, IColorConvertable<T>
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

    public void ToggleFirstChannel()
    {
        if (_map[0, 0] is not IThreeChannelColorSpace)
        {
            throw new ArgumentOutOfRangeException(nameof(_map), _map, "Unsupported operation for current IColorSpace");
        }

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                var threeChannelColorSpace = (IThreeChannelColorSpace)_map[i, j];
                threeChannelColorSpace.FirstChannel.IsVisible = !threeChannelColorSpace.FirstChannel.IsVisible;
                _map[i, j] = (IColorSpace)threeChannelColorSpace;
            }
        }
    }

    public void ToggleSecondChannel()
    {
        if (_map[0, 0] is not IThreeChannelColorSpace)
        {
            throw new ArgumentOutOfRangeException(nameof(_map), _map, "Unsupported operation for current IColorSpace");
        }

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                var threeChannelColorSpace = (IThreeChannelColorSpace)_map[i, j];
                threeChannelColorSpace.SecondChannel.IsVisible = !threeChannelColorSpace.SecondChannel.IsVisible;
                _map[i, j] = (IColorSpace)threeChannelColorSpace;
            }
        }
    }

    public void ToggleThirdChannel()
    {
        if (_map[0, 0] is not IThreeChannelColorSpace)
        {
            throw new ArgumentOutOfRangeException(nameof(_map), _map, "Unsupported operation for current IColorSpace");
        }

        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                var threeChannelColorSpace = (IThreeChannelColorSpace)_map[i, j];
                threeChannelColorSpace.ThirdChannel.IsVisible = !threeChannelColorSpace.ThirdChannel.IsVisible;
                _map[i, j] = (IColorSpace)threeChannelColorSpace;
            }
        }
    }

    public void SaveRaw(Stream stream)
    {
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
