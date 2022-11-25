using System;
using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public class Rgb : IColorSpace, IColorConvertable<Rgb>, IThreeChannelColorSpace, IThreeCoefficientConstructable<Rgb>
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

    public static Rgb FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return new Rgb(first, second, third);
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

    public static Color ToSystemColor(Rgb color)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(color.FirstChannel.Value),
            Coefficient.Denormalize(color.SecondChannel.Value),
            Coefficient.Denormalize(color.ThirdChannel.Value));
    }

    public static Rgb FromSystemColor(Color color)
    {
        return new Rgb(
            Coefficient.Normalize(color.R),
            Coefficient.Normalize(color.G),
            Coefficient.Normalize(color.B));
    }

    public override bool Equals(object obj)
    {
        if (obj is not Rgb rgb)
        {
            return false;
        }

        return float.Abs(rgb.FirstChannel.Value - FirstChannel.Value) < float.Epsilon
            && float.Abs(rgb.SecondChannel.Value - SecondChannel.Value) < float.Epsilon
            && float.Abs(rgb.ThirdChannel.Value - ThirdChannel.Value) < float.Epsilon;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FirstChannel, SecondChannel, ThirdChannel);
    }
}
