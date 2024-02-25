namespace ShinierTextures;

public static class Utils
{
    public static float ConvertRange(
        float originalStart, float originalEnd, // original range
        float newStart, float newEnd, // desired range
        float value) // value to convert
    {
        if (originalStart > value) {
            throw new ArgumentException("Value was smaller than the original range.", nameof(value));
        }
        else if (originalEnd < value)
        {
            throw new ArgumentException("Value was greater than the original range.", nameof(value));
        }

        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return newStart + (value - originalStart) * scale;
    }
}