using System;

namespace YAPaint.Models.ColorSpaces;

public class BlackAndWhite : IColorSpace
{
    private const bool WhiteCode = false; // 0
    private const bool BlackCode = true; // 1

    private readonly bool _value;

    private BlackAndWhite(bool value)
    {
        _value = value;
    }

    public static IColorSpace Black { get; } = new BlackAndWhite(BlackCode);
    public static IColorSpace White { get; } = new BlackAndWhite(WhiteCode);

    public byte[] ToRaw()
    {
        return new[] { _value ? (byte)1 : (byte)0 };
    }

    public string ToPlain()
    {
        return _value ? "1" : "0";
    }

    public Rgb ToRgb()
    {
        return _value ? Rgb.Black : Rgb.White;
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        if (color.Equals(Rgb.Black))
        {
            return Black;
        }

        if (color.Equals(Rgb.White))
        {
            return White;
        }

        throw new ArgumentOutOfRangeException(
            nameof(color),
            color,
            "Unsupported value: color should be either black or white");
    }
}
