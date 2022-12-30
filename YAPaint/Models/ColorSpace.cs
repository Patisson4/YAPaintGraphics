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

    public override string ToString()
    {
        return ToPlain();
    }
}
