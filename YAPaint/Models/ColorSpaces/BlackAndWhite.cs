using System;

namespace YAPaint.Models.ColorSpaces;

public class BlackAndWhite : IColorBaseConverter
{
    private BlackAndWhite() { }
    public static IColorBaseConverter Instance { get; } = new BlackAndWhite();

    public ColorSpace Black { get; } = new ColorSpace { First = 0f, Second = 0f, Third = 0f };
    public ColorSpace White { get; } = new ColorSpace { First = 1f, Second = 1f, Third = 1f };
    public ColorSpace Default => Black;

    public float GetGrayValue(ColorSpace color)
    {
        return color.First;
    }

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return color;
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        if (color is { First: 0f, Second: 0f, Third: 0f })
        {
            return color;
        }

        if (float.Abs(color.First - 1f) < float.Epsilon
         && float.Abs(color.Second - 1f) < float.Epsilon
         && float.Abs(color.Third - 1f) < float.Epsilon)
        {
            return color;
        }

        throw new ArgumentOutOfRangeException(
            nameof(color),
            color.ToString(),
            "Unsupported value: color should be either black or white");
    }
}
