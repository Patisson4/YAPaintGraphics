﻿using System;
using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public class GreyScale : IColorSpace, IColorConvertable<GreyScale>
{
    public GreyScale(byte grey)
    {
        Grey = grey;
    }

    private byte Grey { get; }

    public byte[] ToRaw()
    {
        return new[] { Grey };
    }

    public string ToPlain()
    {
        return $"{Grey}";
    }

    public static Color ToSystemColor(GreyScale color)
    {
        return Color.FromArgb(color.Grey, color.Grey, color.Grey);
    }

    public static GreyScale FromSystemColor(Color color)
    {
        if (color.R != color.G || color.G != color.B)
        {
            throw new ArgumentOutOfRangeException(
                nameof(color),
                color,
                "Unsupported value: color should be a shadow of grey");
        }

        return new GreyScale(color.R);
    }
}
