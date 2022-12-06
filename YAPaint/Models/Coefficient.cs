using System;
using System.Runtime.CompilerServices;

namespace YAPaint.Models;

public readonly record struct Coefficient
{
    private readonly float _value;

    public Coefficient(float value, [CallerArgumentExpression(nameof(value))] string paramName = null)
    {
        if (value is < 0f or > 1f)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value exceeds operating range");
        }

        _value = value;
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
