using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public class Rgb : IThreeChannelColorSpace, IColorSpace
{
    public Rgb(Coefficient red, Coefficient green, Coefficient blue)
    {
        FirstChannel = new ColorChannel(red);
        SecondChannel = new ColorChannel(green);
        ThirdChannel = new ColorChannel(blue);
    }

    /// <summary>
    /// Red part of RGB space
    /// </summary>
    public ColorChannel FirstChannel { get; }

    /// <summary>
    /// Green part of RGB space
    /// </summary>
    public ColorChannel SecondChannel { get; }

    /// <summary>
    /// Blue part of RGB space
    /// </summary>
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
        return $"{Coefficient.Denormalize(FirstChannel.Value)} {Coefficient.Denormalize(SecondChannel.Value)} {Coefficient.Denormalize(ThirdChannel.Value)}";
    }

    public Color ToSystemColor()
    {
        return Color.FromArgb(
            Coefficient.Denormalize(FirstChannel.Value),
            Coefficient.Denormalize(SecondChannel.Value),
            Coefficient.Denormalize(ThirdChannel.Value));
    }

    public static IColorSpace FromSystemColor(Color color)
    {
        return new Rgb(
            Coefficient.Normalize(color.R),
            Coefficient.Normalize(color.G),
            Coefficient.Normalize(color.B));
    }
}
