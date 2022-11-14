using System;
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

    public static Rgb Black { get; } = new Rgb(0, 0, 0);
    public static Rgb White { get; } = new Rgb(1, 1, 1);

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
        return this;
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        return color;
    }

    public static implicit operator Color(Rgb rgb)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(rgb.FirstChannel.Value),
            Coefficient.Denormalize(rgb.SecondChannel.Value),
            Coefficient.Denormalize(rgb.ThirdChannel.Value));
    }

    public static implicit operator Rgb(Color color)
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
