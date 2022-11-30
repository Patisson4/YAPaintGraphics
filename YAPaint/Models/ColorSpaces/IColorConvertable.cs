using System.Drawing;

namespace YAPaint.Models.ColorSpaces;

public interface IColorConvertable<TSelf> where TSelf : IColorConvertable<TSelf>
{
    public static abstract Color ToSystemColor(TSelf color);
    public static abstract TSelf FromSystemColor(Color color);
}
