namespace YAPaint.Models.ExtraColorSpaces;

public class Cmy : IColorConverter
{
    private Cmy() { }
    public static IColorConverter Instance { get; } = new Cmy();

    public ColorSpace Black { get; } = new ColorSpace { First = 1f, Second = 1f, Third = 1f };
    public ColorSpace White { get; } = new ColorSpace { First = 0f, Second = 0f, Third = 0f };
    public ColorSpace Default => White;

    public string FirstChannelName => "Cyan";
    public string SecondChannelName => "Magenta";
    public string ThirdChannelName => "Yellow";

    public float GetGrayValue(ColorSpace color)
    {
        return (color.First + color.Second + color.Third) / 3;
    }

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace { First = 1f - color.First, Second = 1f - color.Second, Third = 1f - color.Third };
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return new ColorSpace { First = 1f - color.First, Second = 1f - color.Second, Third = 1f - color.Third };
    }
}
