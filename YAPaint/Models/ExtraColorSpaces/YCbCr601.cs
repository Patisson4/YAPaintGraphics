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
            255f / 219f * (FirstChannel.Value * 255f - 16) + 255f / 224f * 1.402f * (ThirdChannel.Value * 255f - 128),
            255f / 219f * (FirstChannel.Value * 255f - 16) - 255f / 224f * 1.772f * 0.114f / 0.587f * (SecondChannel.Value * 255f - 128) - 255f / 224f * 1.402f * 0.299f / 0.587f * (ThirdChannel.Value * 255f - 128),
            255f / 219f * (FirstChannel.Value * 255f - 16) + 255f / 224f * 1.772f  * (SecondChannel.Value * 255f - 128));
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        return new YCbCr601((16 + 65.481f * color.FirstChannel.Value + 128.553f * color.SecondChannel.Value +
                                   24.966f * color.ThirdChannel.Value) / 255f, 
            (128 - 37.797f * color.FirstChannel.Value -74.203f * color.SecondChannel.Value +
                     112f * color.ThirdChannel.Value) / 255f, 
            (128 + 112f * color.FirstChannel.Value -93.786f * color.SecondChannel.Value -18.214f 
                * color.ThirdChannel.Value) / 255f );
    }
}