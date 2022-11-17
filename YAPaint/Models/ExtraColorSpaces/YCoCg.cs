using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class YCoCg : IThreeChannelColorSpace, IColorSpace
{
    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public YCoCg(Coefficient y, Coefficient cb, Coefficient cr)
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
            FirstChannel.Value + SecondChannel.Value - ThirdChannel.Value,
            FirstChannel.Value + ThirdChannel.Value,
            FirstChannel.Value - SecondChannel.Value - ThirdChannel.Value);
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        return new YCoCg( 
            0.25f * color.FirstChannel.Value + 0.5f * color.SecondChannel.Value + 0.25f * color.ThirdChannel.Value, 
            0.5f + 0.5f * color.FirstChannel.Value - 0.5f * color.ThirdChannel.Value, 
            0.5f - 0.25f * color.FirstChannel.Value + 0.5f * color.SecondChannel.Value - 0.25f * color.ThirdChannel.Value);
    }
}