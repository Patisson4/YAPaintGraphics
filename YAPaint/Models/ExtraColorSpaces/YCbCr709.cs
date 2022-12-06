namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr709 : IColorConverter
{
    private YCbCr709() { }
    public static IColorConverter Instance { get; } = new YCbCr709();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace(
            Coefficient.Truncate(color.First + 1.5748f * (color.Third - 0.5f)),
            Coefficient.Truncate(
                color.First
              - 0.2126f * 1.5748f / 0.7152f * (color.Third - 0.5f)
              - 0.0722f * 1.8556f / 0.7152f * (color.Second - 0.5f)),
            Coefficient.Truncate(color.First + 1.8556f * (color.Second - 0.5f)));
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var y = 0.2126f * color.First + 0.7152f * color.Second + 0.0722f * color.Third;
        var cb = 0.5f + (color.Third - y) / 1.8556f;
        var cr = 0.5f + (color.First - y) / 1.5748f;
        return new ColorSpace(Coefficient.Truncate(y), Coefficient.Truncate(cb), Coefficient.Truncate(cr));
    }
}
