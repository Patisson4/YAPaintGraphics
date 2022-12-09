namespace YAPaint.Models.ExtraColorSpaces;

public class Hsv : IColorConverter
{
    private Hsv() { }
    public static IColorConverter Instance { get; } = new Hsv();

    public ColorSpace DefaultValue { get; } = new ColorSpace(0f, 1f, 1f);

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        var chroma = color.Third * color.Second;
        var hue = color.First * 6f;
        var x = chroma * (1 - float.Abs(hue % 2 - 1));
        var min = color.Third - chroma;

        float red1, green1, blue1;
        if (hue is >= 0 and < 1)
        {
            red1 = chroma;
            green1 = x;
            blue1 = 0;
        }
        else if (hue is >= 1 and < 2)
        {
            red1 = x;
            green1 = chroma;
            blue1 = 0;
        }
        else if (hue is >= 2 and < 3)
        {
            red1 = 0;
            green1 = chroma;
            blue1 = x;
        }
        else if (hue is >= 3 and < 4)
        {
            red1 = 0;
            green1 = x;
            blue1 = chroma;
        }
        else if (hue is >= 4 and < 5)
        {
            red1 = x;
            green1 = 0;
            blue1 = chroma;
        }
        else
        {
            red1 = chroma;
            green1 = 0;
            blue1 = x;
        }

        return new ColorSpace(red1 + min, green1 + min, blue1 + min);
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var value = float.Max(float.Max(color.First, color.Second), color.Third);
        var min = float.Min(float.Min(color.First, color.Second), color.Third);
        var chroma = value - min;

        float hue, saturation = 0;
        if (chroma == 0)
        {
            hue = 0;
        }
        else if (value == color.First)
        {
            hue = ((color.Second - color.Third) / chroma + 6) % 6;
        }
        else if (value == color.Second)
        {
            hue = (color.Third - color.First) / chroma + 2;
        }
        else
        {
            hue = (color.First - color.Second) / chroma + 4;
        }

        if (value != 0)
        {
            saturation = chroma / value;
        }

        return new ColorSpace(hue / 6, saturation, value);
    }
}
