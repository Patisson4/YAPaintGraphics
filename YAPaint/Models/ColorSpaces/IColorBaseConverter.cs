namespace YAPaint.Models.ColorSpaces;

public interface IColorBaseConverter
{
    ColorSpace ToRgb(ref ColorSpace color);
    ColorSpace FromRgb(ref ColorSpace color);
}
