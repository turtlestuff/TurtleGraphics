using System.Numerics;
using Silk.NET.Windowing;

namespace RenderyThing;

public abstract class Renderer : IDisposable
{   
    protected Dictionary<string, Texture> _textures = new();
    public static Renderer GetApi(IWindow window)
    {
        return new OpenGL.OpenGLRenderer(window);
    }
    
    protected IWindow _window;

    public Vector2D<int> Size => _window.FramebufferSize;

    public Renderer(IWindow window)
    {
        _window = window;
    }

    public virtual Texture AddTexture(Stream file, string name, TextureOptions options)
    {
        var tex = CreateTexture(file, options);
        _textures.Add(name, tex);
        return tex;
    }

    protected abstract Texture CreateTexture(Stream file, TextureOptions options);
    public virtual Texture GetTexture(string name) => _textures[name];

    public void RenderSprite(Texture texture, Vector2D<float> position) =>
        RenderSprite(texture, position, Vector2D<float>.One, 0, Vector4D<float>.One);
    public abstract void RenderSprite(Texture texture, Vector2D<float> position, Vector2D<float> scale, float rotation, Vector4D<float> color);
    public abstract void RenderRect(Vector2D<float> position, Vector2D<float> size, float rotation, Vector4D<float> color);
    public abstract void Clear(Vector4D<float> color);
    
    public abstract void Dispose();
}
