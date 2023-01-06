namespace YAPaint.Models.ColorSpaces;

public interface IColorBaseConverter
{
    ColorSpace Black { get; }
    ColorSpace White { get; }
    ColorSpace Default { get; }

    float GetGrayValue(ColorSpace color);
    ColorSpace ToRgb(ref ColorSpace color);
    ColorSpace FromRgb(ref ColorSpace color);
}
