namespace YAPaint.Models.ExtraColorSpaces;

public class Cmy : IColorConverter
{
    private Cmy() { }
    public static IColorConverter Instance { get; } = new Cmy();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace(1f - color.First, 1f - color.Second, 1f - color.Third);
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return new ColorSpace(1f - color.First, 1f - color.Second, 1f - color.Third);
    }
}
