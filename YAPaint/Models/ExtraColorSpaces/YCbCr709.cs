using System.Drawing;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr709 : IColorSpace, IColorConvertable<YCbCr709>, IThreeChannelColorSpace,
                        IThreeCoefficientConstructable<YCbCr709>
{
    public YCbCr709(Coefficient y, Coefficient cb, Coefficient cr)
    {
        FirstChannel = new ColorChannel(y);
        SecondChannel = new ColorChannel(cb);
        ThirdChannel = new ColorChannel(cr);
    }

    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public static YCbCr709 FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return new YCbCr709(first, second, third);
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

    public static Color ToSystemColor(YCbCr709 color)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(color.FirstChannel.Value + 1.5748f * color.ThirdChannel.Value),
            Coefficient.Denormalize(
                color.FirstChannel.Value
              - (0.2126f * 1.5748f / 0.7152f) * color.ThirdChannel.Value
              - (0.0722f * 1.8556f / 0.7152f) * color.SecondChannel.Value),
            Coefficient.Denormalize(color.FirstChannel.Value + 1.8556f * color.SecondChannel.Value));
    }

    public static YCbCr709 FromSystemColor(Color color)
    {
        var y = 0.2126f * color.R + 0.7152f * color.G + 0.0722f * color.B;
        var cb = 0.5f - (color.B - y) / 1.8556f;
        var cr = 0.5f + (color.R - y) / 1.5748f;
        return new YCbCr709(y, cb, cr);
    }
}
