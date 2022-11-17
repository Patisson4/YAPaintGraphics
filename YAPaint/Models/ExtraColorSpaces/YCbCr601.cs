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
        throw new System.NotImplementedException();
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        return new YCbCr601(16 + (65.738f * color.FirstChannel.Value + 129.057f * color.SecondChannel.Value +
                            25.064f * color.ThirdChannel.Value) / 256f, 
            128 + ( - 37.945f * color.FirstChannel.Value -74.494f * color.SecondChannel.Value +
            112.439f * color.ThirdChannel.Value) / 256f, 
            128 + (112.439f * color.FirstChannel.Value -94.154f * color.SecondChannel.Value -18.285f 
            * color.ThirdChannel.Value) / 256f );
    }
}