namespace YAPaint.Models.ExtraColorSpaces;

public class Hsv : IColorConverter
{
    private Hsv() { }
    public static IColorConverter Instance { get; } = new Hsv();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        var C = color.Third * color.Second;
        var H = color.First * 6f;
        var X = C * (1 - float.Abs(H % 2 - 1));
        var m = color.Third - C;
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

        return new ColorSpace(R1 + m, G1 + m, B1 + m);
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var M = float.Max(float.Max(color.First, color.Second), color.Third);
        var m = float.Min(float.Min(color.First, color.Second), color.Third);
        var C = M - m;
        float H = 0, S = 0, V = M;
        if (C == 0)
        {
            H = 0;
        }
        else if (M == color.First)
        {
            H = (((color.Second - color.Third) / C) % 6) / 6;
        }
        else if (M == color.Second)
        {
            H = ((color.Third - color.First) / C + 2) / 6;
        }
        else
        {
            H = ((color.First - color.Second) / C + 4) / 6;
        }

        if (V != 0)
        {
            S = C / V;
        }

        return new ColorSpace(H, S, V);
    }
}
