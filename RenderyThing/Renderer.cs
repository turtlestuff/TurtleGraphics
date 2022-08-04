using Silk.NET.Windowing;

namespace RenderyThing;

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


    public void RenderSprite(Texture texture, Vector2 position) =>
        RenderSprite(texture, position, Vector2.One, 0, Vector4.One);
    public abstract void RenderSprite(Texture texture, Vector2 position, Vector2 scale, float rotation, Vector4 color);
    public abstract void RenderRect(Vector2 position, Vector2 size, float rotation, Vector4 color);
    public abstract void RenderLine(Vector2 from, Vector2 to, float width, Vector4 color);
    public abstract void RenderLines(Vector2[] lines, bool loop, float width, Vector4 color);
    public abstract void RenderText(string text, Vector2 position, Font font, float size, Vector4 color);

    public abstract void Clear(Vector4 color);
    
    public abstract void Dispose();
}
