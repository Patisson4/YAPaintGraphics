namespace YAPaint.Models.ExtraColorSpaces;

public class YCoCg : IColorConverter
{
    private YCoCg() { }
    public static IColorConverter Instance { get; } = new YCoCg();

    public ColorSpace DefaultValue { get; } = new ColorSpace(0.5f, 0.5f, 0.5f);

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace(
            Coefficient.Truncate(color.First + color.Second - color.Third),
            Coefficient.Truncate(color.First + color.Third - 0.5f),
            Coefficient.Truncate(color.First - color.Second - color.Third + 1f));
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        return new ColorSpace(
            0.25f * color.First + 0.5f * color.Second + 0.25f * color.Third,
            0.5f + 0.5f * color.First - 0.5f * color.Third,
            0.5f - 0.25f * color.First + 0.5f * color.Second - 0.25f * color.Third);
    }
}
