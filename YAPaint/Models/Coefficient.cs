namespace YAPaint.Models;

public static class Coefficient
{
    public static byte Denormalize(float value)
    {
        return (byte)(value * byte.MaxValue);
    }
}
