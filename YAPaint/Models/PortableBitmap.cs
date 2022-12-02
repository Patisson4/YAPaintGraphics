using System;
using System.IO;
using System.Text;
using YAPaint.Models.ColorSpaces;
using YAPaint.Tools;

namespace YAPaint.Models;

public class PortableBitmap
{
    private readonly ColorSpace[,] _map;

    private bool _isFirstVisible = true;
    private bool _isSecondVisible = true;
    private bool _isThirdVisible = true;

    public int Width { get; }
    public int Height { get; }

    public PortableBitmap(ColorSpace[,] map)
    {
        if (map.Length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(map), map, "Bitmap cannot be empty");
        }

        Width = map.GetLength(0);
        Height = map.GetLength(1);

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

    public PortableBitmap(Stream stream)
    {
        // TODO: create enum with different color spaces?
        throw new NotImplementedException();
    }

    public static PortableBitmap FromStream<T>(Stream stream) where T : IColorSpace
    {
        return PnmParser.ReadImage<T>(stream);
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

    //TODO: P5 check
    public void SaveRaw(Stream stream)
    {
        stream.Write(Encoding.ASCII.GetBytes("P6\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{Width} {Height}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{byte.MaxValue}\n"));

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                stream.Write(GetPixel(x, y).ToRaw());
            }
        }
    }

    //TODO: P2 check
    public void SavePlain(Stream stream)
    {
        stream.Write(Encoding.ASCII.GetBytes("P3\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{Width} {Height}\n"));
        stream.Write(Encoding.ASCII.GetBytes($"{byte.MaxValue}\n"));

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width - 1; x++)
            {
                stream.Write(Encoding.ASCII.GetBytes($"{GetPixel(x, y).ToPlain()} "));
            }

            stream.Write(Encoding.ASCII.GetBytes($"{GetPixel(Width - 1, y).ToPlain()}\n"));
        }
    }
}
