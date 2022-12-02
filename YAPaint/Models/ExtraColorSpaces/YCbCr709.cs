﻿namespace YAPaint.Models.ExtraColorSpaces;

public class YCbCr709 : IColorConverter
{
    private YCbCr709() { }
    public static IColorConverter Instance { get; } = new YCbCr709();

    public ColorSpace ToRgb(ref ColorSpace color)
    {
        return new ColorSpace(
            color.First + 1.5748f * color.Third,
            color.First - 0.2126f * 1.5748f / 0.7152f * color.Third - 0.0722f * 1.8556f / 0.7152f * color.Second,
            color.First + 1.8556f * color.Second);
    }

    public ColorSpace FromRgb(ref ColorSpace color)
    {
        var y = 0.2126f * color.First + 0.7152f * color.Second + 0.0722f * color.Third;
        var cb = 0.5f - (color.Third - y) / 1.8556f;
        var cr = 0.5f + (color.First - y) / 1.5748f;
        return new ColorSpace(y, cb, cr);
    }
}
