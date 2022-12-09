namespace YAPaint.Models.ExtraColorSpaces;

public class Rgb : IColorConverter
{
    private Rgb() { }
    public static IColorConverter Instance { get; } = new Rgb();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return color;
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return color;
    }
}