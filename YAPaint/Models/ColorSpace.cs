using System.Drawing;

namespace YAPaint.Models;

public readonly record struct ColorSpace(Coefficient First, Coefficient Second, Coefficient Third)
{
    public byte[] ToRaw()
    {
        return new[]
        {
            Coefficient.Denormalize(First),
            Coefficient.Denormalize(Second),
            Coefficient.Denormalize(Third),
        };
    }

    public string ToPlain()
    {
        return $"{Coefficient.Denormalize(First)} {Coefficient.Denormalize(Second)} {Coefficient.Denormalize(Third)}";
    }

    public static implicit operator Color(ColorSpace rgb)
    {
        return Color.FromArgb(
            Coefficient.Denormalize(rgb.First),
            Coefficient.Denormalize(rgb.Second),
            Coefficient.Denormalize(rgb.Third));
    }

    public static implicit operator ColorSpace(Color color)
    {
        return new ColorSpace(
            Coefficient.Normalize(color.R),
            Coefficient.Normalize(color.G),
            Coefficient.Normalize(color.B));
    }
}
