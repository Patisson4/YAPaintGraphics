using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr709 : IThreeChannelColorSpace, IColorSpace
{
    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public YCbCr709(Coefficient y, Coefficient cb, Coefficient cr)
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
        return
            $"{Coefficient.Denormalize(FirstChannel.Value)} {Coefficient.Denormalize(SecondChannel.Value)} {Coefficient.Denormalize(ThirdChannel.Value)}";
    }

    public Rgb ToRgb()
    {
        return new Rgb(
            FirstChannel.Value + 1.5748f * ThirdChannel.Value,
            FirstChannel.Value - (0.2126f * 1.5748f / 0.7152f) * ThirdChannel.Value -
            (0.0722f * 1.8556f / 0.7152f) * SecondChannel.Value,
            FirstChannel.Value + 1.8556f * SecondChannel.Value);
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        var y = 0.2126f * color.FirstChannel.Value + 0.7152f * color.SecondChannel.Value +
                0.0722f * color.ThirdChannel.Value;
        var cb = 0.5f - (color.ThirdChannel.Value - y) / 1.8556f;
        var cr = 0.5f + (color.FirstChannel.Value - y) / 1.5748f;
        return new YCbCr601( y, cb, cr);
    }
}
