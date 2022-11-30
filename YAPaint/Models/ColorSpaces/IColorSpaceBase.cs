namespace YAPaint.Models.ColorSpaces;

public interface IColorSpaceBase<TSelf> : IColorSpace, IColorConvertable<TSelf>, IThreeCoefficientConstructable<TSelf>
    where TSelf : IColorSpaceBase<TSelf> { }
