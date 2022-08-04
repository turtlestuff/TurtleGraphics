using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RenderyThing.OpenGL;

public unsafe sealed class OpenGLRenderer : Renderer
{   
    static readonly float[] quadVertices =
    {
    //  X     Y
        0.0f, 1.0f,
        1.0f, 0.0f,
        0.0f, 0.0f,
    
        0.0f, 1.0f,
        1.0f, 1.0f,
        1.0f, 0.0f,
    };

    readonly GL _gl;
    readonly GLTextRenderer _textRenderer;

    readonly VertexArrayObject _quadVao;
    readonly VertexBufferObject _quadVbo;

    readonly VertexArrayObject _dynLineVao;
    readonly VertexBufferObject _dynLineVbo;

    readonly ShaderProgram _texQuadProgram;
    readonly ShaderProgram _solidProgram;
    public Matrix4x4 ProjectionMatrix { get; private set; }

    Stream GetResStream(string path) => 
        GetType().Assembly.GetManifestResourceStream($"RenderyThing.OpenGL.{path}") ?? throw new FileNotFoundException($"{path} not found");

    public OpenGLRenderer(IWindow window) : base(window)
    {
        using Stream texQuadVS = GetResStream("Shaders.texQuad.vert"),
            texQuadFS = GetResStream("Shaders.texQuad.frag"),
            solidVS = GetResStream("Shaders.solidColor.vert"),
            solidFS = GetResStream("Shaders.solidColor.frag");

        _gl = GL.GetApi(window);
        _gl.Viewport(FramebufferSize);
        //generate the buffer for the quad
        _quadVbo = new VertexBufferObject(_gl, BufferUsageARB.StaticDraw);
        _quadVao = new VertexArrayObject(_gl, _quadVbo);

        _quadVbo.Bind();
        _quadVbo.BufferData(quadVertices.AsSpan());

        _quadVao.Bind();
        //defines the array as having Vector2, basically
        _quadVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);

        _dynLineVbo = new VertexBufferObject(_gl, BufferUsageARB.DynamicDraw);
        _dynLineVao = new VertexArrayObject(_gl, _quadVbo);

        //create shaders
        _texQuadProgram = new ShaderProgram(_gl, texQuadVS, texQuadFS);
        _solidProgram = new ShaderProgram(_gl, solidVS, solidFS);

        //set some parameters with textures, apparently some drivers need this even if using with only 1 texture
        _texQuadProgram.Use();
        _gl.Uniform1(_gl.GetUniformLocation(_texQuadProgram.Handle, "texture1"), 0);
        _gl.ActiveTexture(TextureUnit.Texture0);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _textRenderer = new(_gl, this);

        _window.Resize += size => 
        {
            _gl.Viewport(FramebufferSize);
        };
        UpdateProjectionMatrix();
        CameraPropertyChanged += UpdateProjectionMatrix;

        _gl.DebugMessageCallback(DebugCallback, null);
    }

    public static void DebugCallback(GLEnum source, GLEnum type, int _, GLEnum severity, int length, nint message, nint __)
    {
        var errorMessage = Marshal.PtrToStringAnsi(message, length);
        Console.Error.WriteLine($"OpenGL debug callback: Source: {Enum.GetName(source)}, Type: {Enum.GetName(type)}, Severity: {Enum.GetName(severity)}");
        Console.Error.WriteLine(errorMessage);
    }

    protected override Texture CreateTexture(Stream file, TextureOptions options)
    {
        return new OpenGLTexture(file, _gl, options);
    }

    public override Font CreateFont(Stream file)
    {
        return new GLStbttFont(_gl, file);
    }

    void UpdateProjectionMatrix()
    {
        var projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(left: 0, right: Size.X, top: 0,  bottom: Size.Y, zNearPlane: -1f, zFarPlane: 1f);

        _texQuadProgram.Use();
        _texQuadProgram.SetProjection(&projectionMatrix);
        _solidProgram.Use();
        _solidProgram.SetProjection(&projectionMatrix);
        ProjectionMatrix = projectionMatrix;
        _textRenderer.UpdateProjectionMatrix(FramebufferSize, Scale); // text renderer renders at pixel resolution
    }

    public override void RenderSprite(Texture texture, Vector2 position, Vector2 scale, float rotation, Vector4 color)
    {
        if (texture is not OpenGLTexture tex)
        {
            throw new Exception($"invalid texture type: expected OpenGLTexture and got {texture.GetType().Name}");
        }
        _texQuadProgram.Use();
        _quadVao.Bind();

        var actualSize = new Vector2(tex.Size.X * scale.X, tex.Size.Y * scale.Y);
        var modelMatrix = GLHelper.ModelMatrix(position, rotation, actualSize);
        _texQuadProgram.SetModel(&modelMatrix);
        _texQuadProgram.SetColor(ref color);
        
        tex.Use();
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);  
    }

    public override void RenderRect(Vector2 position, Vector2 size, float rotation, Vector4 color)
    {
        _solidProgram.Use();
        _quadVao.Bind();

        var modelMatrix = GLHelper.ModelMatrix(position, rotation, size);
        _solidProgram.SetModel(&modelMatrix);
        _solidProgram.SetColor(ref color);
        
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    static Vector2 Normal(Vector2 vec) => new(-vec.Y, vec.X);
    public override void RenderLine(Vector2 from, Vector2 to, float width, Vector4 color)
    {
        var lineVec = Vector2.Normalize(to - from);
        var normal = Normal(lineVec) * width / 2f;
        var from1 = from + normal;
        var from2 = from - normal;
        var to1 = to + normal;
        var to2 = to - normal;

        ReadOnlySpan<float> vertices = stackalloc float[12]
        {
            from1.X, from1.Y,   from2.X, from2.Y,   to1.X,   to1.Y,
            to1.X,   to1.Y,     to2.X,   to2.Y,     from2.X, from2.Y
        };

        _dynLineVao.Bind();
        _dynLineVbo.Bind();
        _dynLineVbo.BufferData(vertices);
        _dynLineVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);

        _solidProgram.Use();
        var modelMatrix = Matrix4x4.Identity;
        _solidProgram.SetModel(&modelMatrix);
        _solidProgram.SetColor(ref color);

        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public override void RenderLines(Vector2[] points, bool loop, float width, Vector4 color)
    {
        if (points.Length < 2)
        {
            throw new RendererException("there must be at least two points to render a line");
        }
        if (points.Length == 2)
        {
            RenderLine(points[0], points[1], width, color);
            return;
        }


        var numberOfIterations = loop ? points.Length : points.Length - 1;

        Span<Vector2> vertices = stackalloc Vector2[numberOfIterations * 6];
        var v = 0;
        var hWidth = width / 2f;

        for (var i = 0; i < numberOfIterations; i++)
        {
            var from = points[i];
            var to = i == points.Length - 1 ? points[0] : points[i + 1];
            Vector2 fromOffset;
            Vector2 toOffset;
            var line = Vector2.Normalize(to - from);
            var normal = Normal(line);
            if (!loop && i == 0)
            {
                fromOffset = normal * hWidth;
            }
            else 
            {
                var prev = i == 0 ? points[^1] : points[i - 1];
                var miter = Normal(Vector2.Normalize(Vector2.Normalize(from - prev) + line)); 
                var len = hWidth / Vector2.Dot(normal, miter);
                fromOffset = miter * len;
            }
            if (!loop && i >= points.Length - 2)
            {
                toOffset = normal * hWidth;
            }
            else
            {
                var next = i >= points.Length - 2 ? points[i - points.Length + 2] : points[i + 2];
                var miter = Normal(Vector2.Normalize(Vector2.Normalize(next - to) + line)); 
                var len = hWidth / Vector2.Dot(normal, miter);
                toOffset = miter * len;
            }

            vertices[v++] = from + fromOffset;
            vertices[v++] = from - fromOffset;
            vertices[v++] = to + toOffset;

            vertices[v++] = to + toOffset;
            vertices[v++] = to - toOffset;
            vertices[v++] = from - fromOffset; 
        }

        _dynLineVao.Bind();
        _dynLineVbo.Bind();
        _dynLineVbo.BufferData(MemoryMarshal.Cast<Vector2, float>(vertices));
        _dynLineVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);

        _solidProgram.Use();
        var modelMatrix = Matrix4x4.Identity;
        _solidProgram.SetModel(&modelMatrix);
        _solidProgram.SetColor(ref color);

        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint) numberOfIterations * 6);
    }

    public void RenderAtlas(Font font)
    {
        _textRenderer.RenderAtlas((GLStbttFont) font);
    }

    public override Vector2 MeasureText(string text, Font font, float size)
    {
        if (font is not GLStbttFont glFont)
            throw new RendererException("Font is not OpenGL font");

        return GLTextRenderer.MeasureString(text, glFont, size);
    }

    public override void RenderText(string text, Vector2 position, Font font, float size, Vector4 color)
    {
        if (font is not GLStbttFont glFont)
            throw new RendererException("Font is not OpenGL font");
        
        _textRenderer.Render(text, glFont, size, position, ref color, out _);
    }

    public override void Clear(Vector4 color)
    {
        _gl.ClearColor(color.X, color.Y, color.Z, color.W);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public override void Dispose()
    {
        _texQuadProgram.Dispose();
        _solidProgram.Dispose();
        _quadVbo.Dispose();
        _quadVao.Dispose();
        _dynLineVbo.Dispose();
        _dynLineVao.Dispose();
        foreach(var (_, tex) in _textures)
        {
            tex.Dispose();
        }
    }
}
