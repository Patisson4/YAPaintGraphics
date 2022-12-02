namespace YAPaint.Models.ExtraColorSpaces;

public class Hsl : IColorConverter
{
    private Hsl() { }
    public static IColorConverter Instance { get; } = new Hsl();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        var C = (1 - float.Abs(2f * color.Third - 1)) * color.Second;
        var H = color.First * 6f;
        var X = C * (1 - float.Abs(H % 2 - 1));
        var m = color.Third - C / 2;
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
        float H = 0, S = 0, L = 0.5f * (M + m);
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

        if (L != 1 && L != 0)
        {
            S = C / (1 - float.Abs(2f * L - 1));
        }

        return new ColorSpace(H, S, L);
    }
}
