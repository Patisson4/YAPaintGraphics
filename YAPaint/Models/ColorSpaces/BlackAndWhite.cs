using System;
using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public class BlackAndWhite : IColorSpace, IColorConvertable<BlackAndWhite>,
                             IThreeCoefficientConstructable<BlackAndWhite>
{
    private const bool WhiteCode = false; // 0
    private const bool BlackCode = true; // 1

    private readonly bool _value;
    private byte ValueAsByte => _value ? (byte)1 : byte.MinValue;

    private BlackAndWhite(bool value)
    {
        _value = value;
    }

    public static BlackAndWhite FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return FromSystemColor(
            Color.FromArgb(
                Coefficient.Denormalize(first),
                Coefficient.Denormalize(second),
                Coefficient.Denormalize(third)));
    }

    public static BlackAndWhite Black { get; } = new BlackAndWhite(BlackCode);
    public static BlackAndWhite White { get; } = new BlackAndWhite(WhiteCode);

    public byte[] ToRaw()
    {
        return new[] { ValueAsByte };
    }

    public string ToPlain()
    {
        return _value ? "1" : "0";
    }

    public static Color ToSystemColor(BlackAndWhite color)
    {
        return Color.FromArgb(color.ValueAsByte, color.ValueAsByte, color.ValueAsByte);
    }

    public static BlackAndWhite FromSystemColor(Color color)
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
