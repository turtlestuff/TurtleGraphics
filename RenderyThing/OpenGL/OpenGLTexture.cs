namespace RenderyThing.OpenGL;

public sealed class OpenGLTexture : Texture
{
    public OpenGLTexture(Vector2D<int> size) : base(size)
    {
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}
