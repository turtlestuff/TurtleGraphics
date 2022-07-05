using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

namespace RenderyThing;

public abstract class RenderyThing
{
    public static RenderyThing GetApi(IWindow window)
    {
        throw new NotImplementedException("haha");
    }

    protected IWindow _window;

    public RenderyThing(IWindow window)
    {
        _window = window;
    }

    public abstract void Initialize();

    public abstract void Render(RenderQueue queue);
}