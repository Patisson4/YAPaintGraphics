using System;
using System.Runtime.CompilerServices;

namespace YAPaint.Tools;

public static class CustomExceptionHelper
{
    public static void ThrowIfGreaterThan(
        int value,
        int upperBound,
        [CallerArgumentExpression("value")]
        string paramName = null)
    {
        if (value > upperBound)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value exceeds operating range");
        }
    }

    public static void ThrowIfNotBetween(
        float value,
        float lowerBound,
        float upperBound,
        [CallerArgumentExpression("value")]
        string paramName = null)
    {
        if (value < lowerBound || value > upperBound)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value exceeds operating range");
        }
    }
}
