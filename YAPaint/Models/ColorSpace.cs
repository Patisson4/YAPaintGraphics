using System;

namespace YAPaint.Models;

public readonly record struct ColorSpace
{
    private readonly float _first;
    private readonly float _second;
    private readonly float _third;

    public required float First
    {
        get => _first;
        init
        {
            if (value is < 0f or > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(First), value, "Value exceeds operating range");
            }
            
            _first = value;
        }
    }

    public required float Second
    {
        get => _second;
        init
        {
            if (value is < 0f or > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(Second), value, "Value exceeds operating range");
            }
            _second = value;
        }
    }

    public required float Third
    {
        get => _third;
        init
        {
            if (value is < 0f or > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(Third), value, "Value exceeds operating range");
            }
            _third = value;
        }
    }

    public override string ToString()
    {
        return $"{Coefficient.Denormalize(First)} {Coefficient.Denormalize(Second)} {Coefficient.Denormalize(Third)}";
    }
}
