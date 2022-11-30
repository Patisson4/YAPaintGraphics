using System.Drawing;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr601 : IColorSpaceComplex<YCbCr601>
{
    public YCbCr601(Coefficient y, Coefficient cb, Coefficient cr)
    {
        FirstChannel = new ColorChannel(y);
        SecondChannel = new ColorChannel(cb);
        ThirdChannel = new ColorChannel(cr);
    }

    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public static YCbCr601 FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return new YCbCr601(first, second, third);
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

    public static Color ToSystemColor(YCbCr601 color)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(color.FirstChannel.Value + 1.402f * color.ThirdChannel.Value),
            Coefficient.Denormalize(
                color.FirstChannel.Value
              - (0.299f * 1.402f / 0.587f) * color.ThirdChannel.Value
              - (0.114f * 1.772f / 0.587f) * color.SecondChannel.Value),
            Coefficient.Denormalize(color.FirstChannel.Value + 1.772f * color.SecondChannel.Value));
    }

    public static YCbCr601 FromSystemColor(Color color)
    {
        var y = 0.299f * color.R + 0.587f * color.G + 0.114f * color.B;
        var cb = 0.5f - (color.B - y) / 1.772f;
        var cr = 0.5f + (color.R - y) / 1.402f;
        return new YCbCr601(y, cb, cr);
    }
}
