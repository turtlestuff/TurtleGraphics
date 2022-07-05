using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace RenderyThing;

public abstract class Renderer
{   
    public static Renderer GetApi(IWindow window)
    {
        return new OpenGLRenderer(window);
    }

    protected IWindow _window;

    public Vector2D<int> Size => _window.FramebufferSize;

    public Renderer(IWindow window)
    {
        _window = window;
    }

    public abstract void Render(RenderQueue queue);
}