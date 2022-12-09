namespace YAPaint.Models.ExtraColorSpaces;

public class Hsl : IColorConverter
{
    private Hsl() { }
    public static IColorConverter Instance { get; } = new Hsl();

    public ColorSpace DefaultValue { get; } = new ColorSpace(0f, 1f, 0.5f);

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        var chroma = (1 - float.Abs(2f * color.Third - 1)) * color.Second;
        var hue = color.First * 6f;
        var x = chroma * (1 - float.Abs(hue % 2 - 1));
        var min = color.Third - chroma / 2;

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

        var red = red1 + min;
        var green = green1 + min;
        var blue = blue1 + min;

        return new ColorSpace(Coefficient.Truncate(red), Coefficient.Truncate(green), Coefficient.Truncate(blue));
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var max = float.Max(float.Max(color.First, color.Second), color.Third);
        var min = float.Min(float.Min(color.First, color.Second), color.Third);
        var chroma = max - min;

        float hue, saturation = 0f, lightness = (max + min) / 2;
        if (chroma == 0)
        {
            hue = 0;
        }
        else if (max == color.First)
        {
            hue = ((color.Second - color.Third) / chroma + 6) % 6;
        }
        else if (max == color.Second)
        {
            hue = (color.Third - color.First) / chroma + 2;
        }
        else
        {
            hue = (color.First - color.Second) / chroma + 4;
        }

        if (float.Abs(max - min) < float.Epsilon || lightness == 0f)
        {
            return new ColorSpace(hue / 6, saturation, lightness);
        }

        //avoiding floating point error
        if (min == 0f || float.Abs(max - 1f) < float.Epsilon)
        {
            saturation = 1f;
        }
        else
        {
            saturation = chroma / (1f - float.Abs(max + min - 1f));
        }

        return new ColorSpace(hue / 6, saturation, lightness);
    }
}
