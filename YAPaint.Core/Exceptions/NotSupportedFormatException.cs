﻿namespace YAPaint.Core.Exceptions;

public class NotSupportedFormatException : Exception
{
    public NotSupportedFormatException() { }

    public NotSupportedFormatException(string message)
        : base(message) { }

    public NotSupportedFormatException(string message, Exception innerException)
        : base(message, innerException) { }
}
