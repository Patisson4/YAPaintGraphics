namespace YAPaint.Models.ColorSpaces;

public interface IColorSpace
{
    byte[] ToRaw();
    string ToPlain();
}
