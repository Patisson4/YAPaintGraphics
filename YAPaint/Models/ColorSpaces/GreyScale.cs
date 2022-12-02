using System;

namespace YAPaint.Models.ColorSpaces;

public class GreyScale : IColorBaseConverter
{
    private GreyScale() { }
    public static IColorBaseConverter Instance { get; } = new GreyScale();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return color;
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        if (color.First != color.Second || color.Second != color.Third)
        {
            throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                "Unsupported value: color should be a shadow of grey");
        }

        return color;
    }
}
