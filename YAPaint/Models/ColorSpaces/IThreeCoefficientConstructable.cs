namespace YAPaint.Models.ColorSpaces;

public interface IThreeCoefficientConstructable<TSelf> where TSelf : IThreeCoefficientConstructable<TSelf>
{
    static abstract TSelf FromCoefficients(Coefficient first, Coefficient second, Coefficient third);
}
