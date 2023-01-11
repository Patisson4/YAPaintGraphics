using System;

namespace YAPaint.Models.ColorSpaces;

public class GreyScale : IColorBaseConverter
{
    private GreyScale() { }
    public static IColorBaseConverter Instance { get; } = new GreyScale();

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
        if (float.Abs(color.First - color.Second) > float.Epsilon
         || float.Abs(color.Second - color.Third) > float.Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(color),
                color.ToString(),
                "Unsupported value: color should be a shadow of grey");
        }

        return color;
    }
}
