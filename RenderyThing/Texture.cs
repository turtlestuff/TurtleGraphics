namespace RenderyThing;

public abstract class Texture : IDisposable
{
    public Vector2D<int> Size { get; }
    internal Texture(Vector2D<int> size)
    {
        Size = size;
    }

    public abstract void Dispose();
}
