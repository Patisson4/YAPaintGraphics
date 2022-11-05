namespace YAPaint.Models.ColorSpaces;

public interface IThreeChannelColorSpace : IColorSpace
{
    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }
}
