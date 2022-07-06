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


    public abstract void Clear(Vector4D<float> color);
    public abstract void Render(RenderQueue queue);

    public abstract void Dispose();
}
