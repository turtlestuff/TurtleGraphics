using Silk.NET.Windowing;

namespace RenderyThing;

public abstract class Renderer : IDisposable
{   
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

    public abstract Texture AddTexture(Stream file, string name);

    public abstract Texture GetTexture(string name);
    public abstract void Render(RenderQueue queue);

    public abstract void Dispose();
}
