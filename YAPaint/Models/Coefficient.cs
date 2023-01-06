namespace YAPaint.Models;

public static class Coefficient
{
    public static float Normalize(int value)
    {
        return (float)value / byte.MaxValue;
    }

    public static byte Denormalize(float value)
    {
        return (byte)(value * byte.MaxValue);
    }
}
