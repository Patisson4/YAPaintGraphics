using System;

namespace YAPaint.Models.ColorSpaces;

public class GreyScale : IColorSpace
{
    public GreyScale(byte grey)
    {
        Grey = grey;
    }

    public static IColorSpace Black { get; } = new GreyScale(0);
    public static IColorSpace White { get; } = new GreyScale(1);

    private byte Grey { get; }

    public byte[] ToRaw()
    {
        return new[] { Grey };
    }

    public string ToPlain()
    {
        return $"{Grey}";
    }

    public Rgb ToRgb()
    {
        return new Rgb(
            Coefficient.Normalize(Grey),
            Coefficient.Normalize(Grey),
            Coefficient.Normalize(Grey));
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        if (float.Abs(color.FirstChannel.Value - color.SecondChannel.Value) < float.Epsilon
         && float.Abs(color.ThirdChannel.Value - color.SecondChannel.Value) < float.Epsilon)
        {
            return new GreyScale(Coefficient.Denormalize(color.FirstChannel.Value));
        }

        throw new ArgumentOutOfRangeException(
            nameof(color),
            color,
            "Unsupported value: color should be a shadow of grey");
    }
}
