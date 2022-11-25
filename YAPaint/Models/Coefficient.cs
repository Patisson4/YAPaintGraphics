using YAPaint.Tools;

namespace YAPaint.Models;

public readonly record struct Coefficient
{
    private readonly float _value;

    public Coefficient(float value)
    {
        CustomExceptionHelper.ThrowIfNotBetween(value, 0f, 1f);

        _value = value;
    }

    public static Coefficient Zero { get; } = new Coefficient(0f);
    
    public static float Normalize(byte value)
    {
        return (float)value / byte.MaxValue;
    }
    
    public static float Normalize(int value)
    {
        return (float)value / byte.MaxValue;
    }

    public static byte Denormalize(float value)
    {
        return (byte)(value * byte.MaxValue);
    }

    public static implicit operator Coefficient(float value)
    {
        return new Coefficient(value);
    }

    public static implicit operator float(Coefficient coefficient)
    {
        return coefficient._value;
    }
}
