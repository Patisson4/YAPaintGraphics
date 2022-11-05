using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public interface IColorSpace
{
    byte[] ToRaw();
    string ToPlain();

    Color ToSystemColor();
    static abstract IColorSpace FromSystemColor(Color color);
}
