namespace TurtleGraphics;

public enum ScalingType
{
    NearestNeighbor,
    Linear
}

public sealed class TextureOptions 
{
    public ScalingType ScalingType { get; init; } = ScalingType.Linear;
}

public abstract class Texture : IDisposable
{
    public TextureOptions Options { get; }
    public Vector2D<int> Size { get; protected set; }
    internal Texture(TextureOptions options)
    {
        Options = options;
        Size = Vector2D<int>.Zero;
    }

    public abstract void Dispose();
}
