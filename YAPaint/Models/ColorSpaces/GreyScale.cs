using System;
using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public class GreyScale : IColorSpace
{
    public GreyScale(byte grey)
    {
        Grey = grey;
    }

    public static IColorSpace Black { get; } = new GreyScale(0);
    public static IColorSpace White { get; } = new GreyScale(1);

    private byte Grey { get; }

    public byte[] ToRaw()
    {
        return new[] { Grey };
    }

    public string ToPlain()
    {
        return $"{Grey}";
    }

    public Color ToSystemColor()
    {
        return Color.FromArgb(Grey, Grey, Grey);
    }

    public static IColorSpace FromSystemColor(Color color)
    {
        if (color.R != color.G || color.G != color.B)
        {
            throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                "Unsupported value: color should be shadow of grey");
        }

        return new GreyScale(color.R);
    }
}
