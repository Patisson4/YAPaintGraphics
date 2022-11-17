using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class Cmy : IThreeChannelColorSpace, IColorSpace
{
    public Cmy(Coefficient cyan, Coefficient magenta, Coefficient yellow)
    {
        FirstChannel = new ColorChannel(cyan);
        SecondChannel = new ColorChannel(magenta);
        ThirdChannel = new ColorChannel(yellow);
    }

    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

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
        return new Rgb(1f - FirstChannel.Value,
            1f - SecondChannel.Value,
            1f - ThirdChannel.Value);
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        return new Cmy(1f - color.FirstChannel.Value,
            1f - color.SecondChannel.Value,
            1f - color.ThirdChannel.Value);
    }
}