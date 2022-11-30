using System.Drawing;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class Cmy : IColorSpaceComplex<Cmy>
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

    public static Cmy FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return new Cmy(first, second, third);
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

    public static Color ToSystemColor(Cmy color)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(1f - color.FirstChannel.Value),
            Coefficient.Denormalize(1f - color.SecondChannel.Value),
            Coefficient.Denormalize(1f - color.ThirdChannel.Value));
    }

    public static Cmy FromSystemColor(Color color)
    {
        return new Cmy(1f - color.R, 1f - color.G, 1f - color.B);
    }
}
