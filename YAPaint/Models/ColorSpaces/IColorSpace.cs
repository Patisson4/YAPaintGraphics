namespace YAPaint.Models.ColorSpaces;

public interface IColorSpace
{
    byte[] ToRaw();
    string ToPlain();

    Rgb ToRgb();
    static abstract IColorSpace FromRgb(Rgb color);
}
