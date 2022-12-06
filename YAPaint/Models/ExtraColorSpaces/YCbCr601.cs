namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr601 : IColorConverter
{
    private YCbCr601() { }
    public static IColorConverter Instance { get; } = new YCbCr601();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace(
            color.First + 1.402f * color.Third,
            color.First - 0.299f * 1.402f / 0.587f * color.Third - 0.114f * 1.772f / 0.587f * color.Second,
            color.First + 1.772f * color.Second);
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var y = 0.299f * color.First + 0.587f * color.Second + 0.114f * color.Third;
        var cb = 0.5f + (color.Third - y) / 1.772f;
        var cr = 0.5f + (color.First - y) / 1.402f;
        return new ColorSpace(y, cb, cr);
    }
}
