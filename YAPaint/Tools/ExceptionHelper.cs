using System;
using System.Runtime.CompilerServices;

namespace YAPaint.Tools;

public static class ExceptionHelper
{
    public static void ThrowIfGreaterThan(
        int value,
        int upperBound,
        [CallerArgumentExpression("value")]
        string paramName = null)
    {
        if (value > upperBound)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value exceeds operating range");
        }
    }
}
