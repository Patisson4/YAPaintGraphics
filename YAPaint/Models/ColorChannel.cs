namespace YAPaint.Models;

public class ColorChannel
{
    private readonly Coefficient _value;

    public ColorChannel(Coefficient value)
    {
        _value = value;
    }

    public bool IsVisible { get; set; } = true;
    public Coefficient Value => IsVisible ? _value : Coefficient.Zero;
}
