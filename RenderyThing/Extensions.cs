namespace RenderyThing;

public static class VectorExtensions
{
    public static Vector2 Normal(this Vector2 vec) => new(-vec.Y, vec.X);

    public static Vector2 ToSystemF(this Vector2D<int> vec) => (Vector2) vec;

    public static Vector2D<float> ToF(this Vector2D<int> vec) => (Vector2D<float>) vec;
}