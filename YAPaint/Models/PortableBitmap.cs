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

    public IColorBaseConverter ColorConverter { get; private set; }
    public int Width { get; }
    public int Height { get; }

    public PortableBitmap(ColorSpace[,] map, IColorBaseConverter colorConverter)
    {
        if (map.Length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(map), map, "Bitmap cannot be empty");
        }

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

    public static PortableBitmap FromStream(Stream stream, IColorBaseConverter colorConvert)
    {
        return PnmParser.ReadImage(stream, colorConvert);
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
