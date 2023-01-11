namespace YAPaint.Models.ExtraColorSpaces;

public class Hsv : IColorConverter
{
    private Hsv() { }
    public static IColorConverter Instance { get; } = new Hsv();

    public ColorSpace Black { get; } = new ColorSpace { First = 0f, Second = 0f, Third = 0f };
    public ColorSpace White { get; } = new ColorSpace { First = 0f, Second = 0f, Third = 1f };
    public ColorSpace Default { get; } = new ColorSpace { First = 0f, Second = 1f, Third = 1f };

    public string FirstChannelName => "Hue";
    public string SecondChannelName => "Saturation";
    public string ThirdChannelName => "Value";

    public float GetGrayValue(ColorSpace color)
    {
        return color.First;
    }

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

        var red = red1 + min;
        var green = green1 + min;
        var blue = blue1 + min;

        return new ColorSpace
        {
            First = float.Clamp(red, 0, 1),
            Second = float.Clamp(green, 0, 1),
            Third = float.Clamp(blue, 0, 1),
        };
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
        else if (float.Abs(value - color.First) < float.Epsilon)
        {
            hue = ((color.Second - color.Third) / chroma + 6) % 6;
        }
        else if (float.Abs(value - color.Second) < float.Epsilon)
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

        return new ColorSpace
        {
            First = float.Clamp(hue / 6, 0, 1),
            Second = float.Clamp(saturation, 0, 1),
            Third = float.Clamp(value, 0, 1),
        };
    }
}
