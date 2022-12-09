namespace YAPaint.Models.ExtraColorSpaces;

public class Cmy : IColorConverter
{
    private Cmy() { }
    public static IColorConverter Instance { get; } = new Cmy();

    public ColorSpace DefaultValue { get; } = new ColorSpace(0f, 0f, 0f);

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace(1f - color.First, 1f - color.Second, 1f - color.Third);
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return new ColorSpace(1f - color.First, 1f - color.Second, 1f - color.Third);
    }
}
