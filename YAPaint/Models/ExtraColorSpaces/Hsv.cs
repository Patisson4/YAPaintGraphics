using System;
using System.Drawing;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class Hsv : IColorSpace, IColorConvertable<Hsv>, IThreeChannelColorSpace, IThreeCoefficientConstructable<Hsv>
{
    public Hsv(Coefficient h, Coefficient s, Coefficient v)
    {
        FirstChannel = new ColorChannel(h);
        SecondChannel = new ColorChannel(s);
        ThirdChannel = new ColorChannel(v);
    }

    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public static Hsv FromCoefficients(Coefficient first, Coefficient second, Coefficient third)
    {
        return new Hsv(first, second, third);
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

    public static Color ToSystemColor(Hsv color)
    {
        var C = color.ThirdChannel.Value * color.SecondChannel.Value;
        var H = color.FirstChannel.Value * 6f;
        var X = C * (1 - Math.Abs(H % 2 - 1));
        var m = color.ThirdChannel.Value - C;
        float R1 = 0, G1 = 0, B1 = 0;
        if (H is >= 0 and < 1)
        {
            R1 = C;
            G1 = X;
            B1 = 0;
        }
        else if (H is >= 1 and < 2)
        {
            R1 = X;
            G1 = C;
            B1 = 0;
        }
        else if (H is >= 2 and < 3)
        {
            R1 = 0;
            G1 = C;
            B1 = X;
        }
        else if (H is >= 3 and < 4)
        {
            R1 = 0;
            G1 = X;
            B1 = C;
        }
        else if (H is >= 4 and < 5)
        {
            R1 = X;
            G1 = 0;
            B1 = C;
        }
        else
        {
            R1 = C;
            G1 = 0;
            B1 = X;
        }

        return Color.FromArgb(
            Coefficient.Denormalize(R1 + m),
            Coefficient.Denormalize(G1 + m),
            Coefficient.Denormalize(B1 + m));
    }

    public static Hsv FromSystemColor(Color color)
    {
        var M = float.Max(float.Max(color.R, color.G), color.B);
        var m = float.Min(float.Min(color.R, color.G), color.B);
        var C = M - m;
        float H = 0, S = 0, V = M;
        if (C == 0)
        {
            H = 0;
        }
        else if (M == color.R)
        {
            H = (((color.G - color.B) / C) % 6) / 6;
        }
        else if (M == color.G)
        {
            H = ((color.B - color.R) / C + 2) / 6;
        }
        else
        {
            H = ((color.R - color.G) / C + 4) / 6;
        }

        if (V != 0)
        {
            S = C / V;
        }

        return new Hsv(H, S, V);
    }
}
