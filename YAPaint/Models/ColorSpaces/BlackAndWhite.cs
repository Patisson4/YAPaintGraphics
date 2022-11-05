using System;
using System.Drawing;

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

    private byte ValueAsByte => _value ? byte.MaxValue : byte.MinValue;

    public byte[] ToRaw()
    {
        return new[] { ValueAsByte };
    }

    public string ToPlain()
    {
        return _value == WhiteCode ? "0" : "1";
    }

    public Color ToSystemColor()
    {
        return Color.FromArgb(ValueAsByte, ValueAsByte, ValueAsByte);
    }

    public static IColorSpace FromSystemColor(Color color)
    {
        if (color.R == 0 && color.G == 0 && color.B == 0)
        {
            return Black;
        }

        if (color.R == 255 && color.G == 255 && color.B == 255)
        {
            return White;
        }

        throw new ArgumentOutOfRangeException(
            nameof(color),
            color,
            "Unsupported value: color should be either black or white");
    }
}
