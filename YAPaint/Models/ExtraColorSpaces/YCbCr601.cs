namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr601 : IColorConverter
{
    private YCbCr601() { }
    public static IColorConverter Instance { get; } = new YCbCr601();

    public ColorSpace Black { get; } = new ColorSpace { First = 0f, Second = .5f, Third = .5f };
    public ColorSpace White { get; } = new ColorSpace { First = 1f, Second = .5f, Third = .5f };
    public ColorSpace Default { get; } = new ColorSpace { First = .5f, Second = .5f, Third = .5f };

    public string FirstChannelName => "Luma";
    public string SecondChannelName => "Blue";
    public string ThirdChannelName => "Red";

    public float GetGrayValue(ColorSpace color)
    {
        return color.First;
    }

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        var r = color.First + 1.402f * (color.Third - 0.5f);
        var g = color.First
              - 0.299f * 1.402f / 0.587f * (color.Third - 0.5f)
              - 0.114f * 1.772f / 0.587f * (color.Second - 0.5f);
        var b = color.First + 1.772f * (color.Second - 0.5f);

        return new ColorSpace
        {
            First = float.Clamp(r, 0, 1),
            Second = float.Clamp(g, 0, 1),
            Third = float.Clamp(b, 0, 1),
        };
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var y = 0.299f * color.First + 0.587f * color.Second + 0.114f * color.Third;
        var cb = 0.5f + (color.Third - y) / 1.772f;
        var cr = 0.5f + (color.First - y) / 1.402f;

        return new ColorSpace
        {
            First = float.Clamp(y, 0, 1),
            Second = float.Clamp(cb, 0, 1),
            Third = float.Clamp(cr, 0, 1),
        };
    }
}
