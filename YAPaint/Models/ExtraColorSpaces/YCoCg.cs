namespace YAPaint.Models.ExtraColorSpaces;

public class YCoCg : IColorConverter
{
    private YCoCg() { }
    public static IColorConverter Instance { get; } = new YCoCg();

    public ColorSpace Black { get; } = new ColorSpace { First = 0f, Second = .5f, Third = .5f };
    public ColorSpace White { get; } = new ColorSpace { First = 1f, Second = .5f, Third = .5f };
    public ColorSpace Default { get; } = new ColorSpace { First = .5f, Second = .5f, Third = .5f };

    public string FirstChannelName => "Luma";
    public string SecondChannelName => "Green";
    public string ThirdChannelName => "Orange";

    public float GetGrayValue(ColorSpace color)
    {
        return color.First;
    }

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        var r = color.First + color.Second - color.Third;
        var g = color.First + color.Third - 0.5f;
        var b = color.First - color.Second - color.Third + 1;

        return new ColorSpace
        {
            First = float.Clamp(r, 0, 1),
            Second = float.Clamp(g, 0, 1),
            Third = float.Clamp(b, 0, 1),
        };
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var y = 0.25f * color.First + 0.5f * color.Second + 0.25f * color.Third;
        var co = 0.5f + 0.5f * color.First - 0.5f * color.Third;
        var cg = 0.5f - 0.25f * color.First + 0.5f * color.Second - 0.25f * color.Third;

        return new ColorSpace
        {
            First = float.Clamp(y, 0, 1),
            Second = float.Clamp(co, 0, 1),
            Third = float.Clamp(cg, 0, 1),
        };
    }
}
