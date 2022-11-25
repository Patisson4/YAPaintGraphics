namespace YAPaint.Models.ColorSpaces;

public interface IThreeChannelColorSpace
{
    ColorChannel FirstChannel { get; }
    ColorChannel SecondChannel { get; }
    ColorChannel ThirdChannel { get; }
}
