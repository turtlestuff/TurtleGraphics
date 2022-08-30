using Silk.NET.Windowing;

namespace TurtleGraphics;

public class RendererException : Exception
{
    public RendererException(string message) : base(message) {}
}

public abstract class Renderer : IDisposable
{   
    protected Dictionary<string, Texture> _textures = new();
    public static Renderer GetApi(IWindow window)
    {
        return new OpenGL.OpenGLRenderer(window);
    }
    
    protected IWindow _window;

    float _scale = 1f;
    public float Scale 
    { 
        get => _scale; 
        set
        {
            _scale = value;
            OnCameraPropertyChanged();
        } 
    }

    public Vector2D<int> Size => (Vector2D<int>) ((Vector2D<float>) FramebufferSize / Scale);
    public Vector2D<int> FramebufferSize => _window.FramebufferSize;
    
    public Renderer(IWindow window)
    {
        _window = window;
        window.Resize += _ => OnCameraPropertyChanged();
    }

    protected event Action? CameraPropertyChanged;
    protected virtual void OnCameraPropertyChanged()
    {
        CameraPropertyChanged?.Invoke();
    }

    public virtual Texture AddTexture(Stream file, string name, TextureOptions options)
    {
        var tex = CreateTexture(file, options);
        _textures.Add(name, tex);
        return tex;
    }

    protected abstract Texture CreateTexture(Stream file, TextureOptions options);
    public virtual Texture GetTexture(string name) => _textures[name];
    public abstract Font CreateFont(Stream file);
    public abstract Vector2 MeasureText(string text, Font font, float size);

    public void DrawSprite(Texture texture, Vector2 position) =>
        DrawSprite(texture, position, Vector2.One, 0, Vector4.One);

    public virtual void DrawSprite(Texture texture, Vector2 position, Vector2 scale, float rotation, Vector4 color) =>
        DrawTextureRect(texture, position, texture.Size.ToSystemF() * scale, rotation, color);

    public virtual void DrawTextureRect(Texture texture, Rectangle<float> rect, float rotation, Vector4 color) =>
        DrawTextureRect(texture, rect.Origin.ToSystem(), rect.Origin.ToSystem(), rotation, color);

    public abstract void DrawTextureRect(Texture texture, Vector2 position, Vector2 size, float rotation, Vector4 color);
    public abstract void DrawSolidRect(Vector2 position, Vector2 size, float rotation, Vector4 color);

    public virtual void DrawSolidRect(Rectangle<float> rect, float rotation, Vector4 color) =>
        DrawSolidRect((Vector2) rect.Origin, (Vector2) rect.Size, rotation, color);

    public abstract void DrawSolidCircle(Vector2 position, Vector2 size, float rotation, Vector4 color);

    public virtual void DrawSolidCircle(Rectangle<float> rect, float rotation, Vector4 color) =>
        DrawSolidCircle((Vector2) rect.Origin, (Vector2) rect.Size, rotation, color);

    public virtual void DrawSolidLine(Vector2 from, Vector2 to, float width, Vector4 color)
    {
        Span<Vector2> vtxs = stackalloc Vector2[Shapes.LineVtxCount()];
        Shapes.Line(from, to, width, vtxs);
        DrawSolidVertices(vtxs, color);
    }

    public virtual void DrawSolidLines(ReadOnlySpan<Vector2> points, bool loop, float width, Vector4 color)
    {
        Span<Vector2> vtxs = stackalloc Vector2[Shapes.LinesMiterVtxCount(points.Length, loop)];
        Shapes.LinesMiter(points, width, loop, vtxs);
        DrawSolidVertices(vtxs, color);
    }

    public virtual void DrawSolidConvexPoly(ReadOnlySpan<Vector2> points, Vector4 color)
    {
        Span<Vector2> vtxs = stackalloc Vector2[Shapes.TraingulateConvexVtxCount(points.Length)];
        Shapes.TriangulateConvex(points, vtxs);
        DrawSolidVertices(vtxs, color);
    }
    public virtual void DrawSolidRegularNGon(Vector2 center, float radius, int sides, float rotation, Vector4 color)
    {
        Span<Vector2> vtxs = stackalloc Vector2[Shapes.SolidRegularNGonVtxCount(sides)];
        Shapes.SolidRegularNGon(center, radius, sides, rotation, vtxs);
        DrawSolidVertices(vtxs, color);
    }
    public virtual void DrawRegularNGonOutline(Vector2 center, float radius, int sides, float rotation, float width, Vector4 color)
    {
        Span<Vector2> vtxs = stackalloc Vector2[Shapes.RegularNGonOutlineVtxCount(sides)];
        Shapes.RegularNGonOutline(center, radius, sides, rotation, width, vtxs);
        DrawSolidVertices(vtxs, color);
    }

    public abstract void DrawSolidVertices(ReadOnlySpan<Vector2> triVertices, Vector4 color);
    public abstract void DrawSolidVertices(ReadOnlySpan<Vector2> triVertices, Vector2 translation, Vector2 scale, float rotation, Vector4 color);

    public abstract void DrawTexturedVertices(ReadOnlySpan<Vector2> triVertices, Texture tex, Vector2 translation, Vector2 scale, float rotation, Vector4 color);

    public abstract void DrawText(string text, Vector2 position, Font font, float size, Vector4 color);

    public abstract void Clear(Vector4 color);
    
    public abstract void Dispose();
}
