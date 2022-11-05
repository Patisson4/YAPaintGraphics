namespace YAPaint.Models.ColorSpaces;

public interface IThreeChannelColorSpace
{
    public ColorChannel FirstChannel { get; }
    public ColorChannel SecondChannel { get; }
    public ColorChannel ThirdChannel { get; }
}
