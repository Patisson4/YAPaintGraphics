namespace YAPaint.Models.ExtraColorSpaces;

public class Rgb : IColorConverter
{
    private Rgb() { }
    public static IColorConverter Instance { get; } = new Rgb();

    public ColorSpace Black { get; } = new ColorSpace { First = 0f, Second = 0f, Third = 0f };
    public ColorSpace White { get; } = new ColorSpace { First = 1f, Second = 1f, Third = 1f };
    public ColorSpace Default => Black;

    public string FirstChannelName => "Red";
    public string SecondChannelName => "Green";
    public string ThirdChannelName => "Blue";

    public float GetGrayValue(ColorSpace color)
    {
        return (color.First + color.Second + color.Third) / 3;
    }

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return color;
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return color;
    }
}
