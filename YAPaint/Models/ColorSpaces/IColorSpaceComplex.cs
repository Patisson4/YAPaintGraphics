namespace YAPaint.Models.ColorSpaces;

public interface IColorSpaceComplex<TSelf> : IColorSpaceBase<TSelf>, IThreeChannelColorSpace
    where TSelf : IColorSpaceComplex<TSelf> { }
