using System;
using DynamicData.Aggregation;
using YAPaint.Models.ColorSpaces;

namespace YAPaint.Models.ExtraColorSpaces;

public class Hsl : IThreeChannelColorSpace, IColorSpace
{
    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }

    public Hsl(Coefficient h, Coefficient s, Coefficient l)
    {
        FirstChannel = new ColorChannel(h);
        SecondChannel = new ColorChannel(s);
        ThirdChannel = new ColorChannel(l);
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

    public Rgb ToRgb()
    {
        var C = (1 - Math.Abs(2f * ThirdChannel.Value - 1)) * SecondChannel.Value;
        var H = FirstChannel.Value * 6f;
        var X = C * (1 - Math.Abs(H % 2 - 1));
        var m = ThirdChannel.Value - C / 2;
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
        
        return new Rgb(R1 + m, G1 + m, B1 + m);
    }

    public static IColorSpace FromRgb(Rgb color)
    {
        var M = float.Max(float.Max(color.FirstChannel.Value, color.SecondChannel.Value), color.ThirdChannel.Value);
        var m = float.Min(float.Min(color.FirstChannel.Value, color.SecondChannel.Value), color.ThirdChannel.Value);
        var C = M - m;
        float H = 0, S = 0, L = 0.5f * (M + m);
        if (C == 0)
        {
            H = 0;
        }
        else if (M == color.FirstChannel.Value)
        {
            H = (((color.SecondChannel.Value - color.ThirdChannel.Value) / C) % 6) / 6;
        }
        else if (M == color.SecondChannel.Value)
        {
            H = ((color.ThirdChannel.Value - color.FirstChannel.Value) / C + 2) / 6;
        }
        else
        {
            H = ((color.FirstChannel.Value - color.SecondChannel.Value) / C + 4) / 6;
        }

        if (L != 1 && L != 0)
        {
            S = C / (1 - Math.Abs(2f * L - 1));
        }
        return new Hsl(H, S, L);
    }
}
