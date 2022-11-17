using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr601 : IThreeChannelColorSpace, IColorSpace
{
    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public YCbCr601(Coefficient y, Coefficient cb, Coefficient cr)
    {
        FirstChannel = new ColorChannel(y);
        SecondChannel = new ColorChannel(cb);
        ThirdChannel = new ColorChannel(cr);
    }

    public byte[] ToRaw()
    {
        return new[]
        {
            Coefficient.Denormalize(FirstChannel.Value),
            Coefficient.Denormalize(SecondChannel.Value),
            Coefficient.Denormalize(ThirdChannel.Value),
        };
    }

    public string ToPlain()
    {
        return string.Format(
            "{0} {1} {2}",
            Coefficient.Denormalize(FirstChannel.Value),
            Coefficient.Denormalize(SecondChannel.Value),
            Coefficient.Denormalize(ThirdChannel.Value));
    }

    public Rgb ToRgb()
    {
        return new Rgb(
            FirstChannel.Value + 1.402f * ThirdChannel.Value,
            FirstChannel.Value - (0.299f * 1.402f / 0.587f) * ThirdChannel.Value -
            (0.114f * 1.772f / 0.587f) * SecondChannel.Value,
            FirstChannel.Value + 1.772f * SecondChannel.Value);
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        var y = 0.299f * color.FirstChannel.Value + 0.587f * color.SecondChannel.Value +
                 0.114f * color.ThirdChannel.Value;
        var cb = 0.5f - (color.ThirdChannel.Value - y) / 1.772f;
        var cr = 0.5f + (color.FirstChannel.Value - y) / 1.402f;
        return new YCbCr601( y, cb, cr);
    }
}