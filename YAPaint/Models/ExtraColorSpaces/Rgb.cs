namespace YAPaint.Models.ExtraColorSpaces;

public class Rgb : IColorConverter
{
    private Rgb() { }
    public static IColorConverter Instance { get; } = new Rgb();

    public ColorSpace DefaultValue { get; } = new ColorSpace(0f, 0f, 0f);

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return color;
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return color;
    }
}
