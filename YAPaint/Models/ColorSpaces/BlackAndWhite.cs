using System;

namespace YAPaint.Models.ColorSpaces;

public class BlackAndWhite : IColorBaseConverter
{
    private BlackAndWhite() { }
    public static IColorBaseConverter Instance { get; } = new BlackAndWhite();

    public ColorSpace DefaultValue { get; } = new ColorSpace(0f, 0f, 0f);

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return color;
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        if (color.First == 0.0 && color.Second == 0.0 && color.Third == 0.0)
        {
            return color;
        }

        if (float.Abs(color.First - 1.0f) < float.Epsilon
         && float.Abs(color.Second - 1.0f) < float.Epsilon
         && float.Abs(color.Third - 1.0f) < float.Epsilon)
        {
            return color;
        }

        throw new ArgumentOutOfRangeException(
            nameof(color),
            color.ToPlain(),
            "Unsupported value: color should be either black or white");
    }
}
