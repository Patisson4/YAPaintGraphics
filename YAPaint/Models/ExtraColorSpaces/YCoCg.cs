using System.Drawing;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class YCoCg : IColorSpace, IColorConvertable<YCoCg>, IThreeChannelColorSpace,
                     IThreeCoefficientConstructable<YCoCg>
{
    public YCoCg(Coefficient y, Coefficient co, Coefficient cg)
    {
        FirstChannel = new ColorChannel(y);
        SecondChannel = new ColorChannel(co);
        ThirdChannel = new ColorChannel(cg);
    }

    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public static YCoCg FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return new YCoCg(first, second, third);
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

    public static Color ToSystemColor(YCoCg color)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(color.FirstChannel.Value + color.SecondChannel.Value - color.ThirdChannel.Value),
            Coefficient.Denormalize(color.FirstChannel.Value + color.ThirdChannel.Value),
            Coefficient.Denormalize(color.FirstChannel.Value - color.SecondChannel.Value - color.ThirdChannel.Value));
    }

    public static YCoCg FromSystemColor(Color color)
    {
        return new YCoCg(
            0.25f * color.R + 0.5f * color.G + 0.25f * color.B,
            0.5f + 0.5f * color.R - 0.5f * color.B,
            0.5f - 0.25f * color.R + 0.5f * color.G - 0.25f * color.B);
    }
}
