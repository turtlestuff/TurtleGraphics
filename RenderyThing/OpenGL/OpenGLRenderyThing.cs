using System.Numerics;
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

    readonly uint _quadVao;
    readonly uint _quadVbo;
    readonly uint _texQuadProgram;
    readonly uint _solidProgram;
    
    Stream GetResStream(string path) => GetType().Assembly.GetManifestResourceStream($"RenderyThing.OpenGL.{path}") ?? throw new Exception($"{path} not found");

    uint CreateShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        var info = _gl.GetShaderInfoLog(shader);
        if(info != "") 
            throw new Exception("shader compilation failed: " + info);
        return shader;
    }

    uint LinkProgram(uint vertex, uint fragment)
    {
        var program = _gl.CreateProgram();
        _gl.AttachShader(program, vertex);
        _gl.AttachShader(program, fragment);
        _gl.LinkProgram(program);

        _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out var status);
        if (status == 0)
            throw new Exception("linking of shader program failed: " + _gl.GetProgramInfoLog(program));
        return program;
    }

    public OpenGLRenderer(IWindow window) : base(window)
    {
        //read shaders from resources
        using Stream texQuadVS = GetResStream("Shaders.texQuad.vert"),
            texQuadFS = GetResStream("Shaders.texQuad.frag"),
            solidVS = GetResStream("Shaders.solidColor.vert"),
            solidFS = GetResStream("Shaders.solidColor.frag");
        using StreamReader texQuadVertSR = new(texQuadVS),
            texQuadFragSR = new(texQuadFS),
            solidVertSR = new(solidVS),
            solidFragSR = new(solidFS);
        var texQuadVertSrc = texQuadVertSR.ReadToEnd();
        var texQuadFragSrc = texQuadFragSR.ReadToEnd();
        var solidVertSrc = solidVertSR.ReadToEnd();
        var solidFragSrc = solidFragSR.ReadToEnd();

        _gl = GL.GetApi(window);
        _gl.Viewport(Size);
        //generate the buffer for the quad
        _quadVao = _gl.GenVertexArray();
        _quadVbo = _gl.GenBuffer();

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _quadVbo);
        fixed(float* v = quadVertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) quadVertices.Length * sizeof(float), v, BufferUsageARB.StaticDraw);
        }

        _gl.BindVertexArray(_quadVao);
        
        //defines the array as having Vector2D<float>, basically
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
        _gl.EnableVertexAttribArray(0); 

        //create shaders
        var texVert = CreateShader(ShaderType.VertexShader, texQuadVertSrc);
        var texFrag = CreateShader(ShaderType.FragmentShader, texQuadFragSrc);
        var solidVert = CreateShader(ShaderType.VertexShader, solidVertSrc);
        var solidFrag = CreateShader(ShaderType.FragmentShader, solidFragSrc);

        _solidProgram = LinkProgram(solidVert, solidFrag);
        _texQuadProgram = LinkProgram(texFrag, texVert);
        
        //set some parameters with textures, apparently some drivers need this even if using with only 1 texture
        _gl.UseProgram(_texQuadProgram);
        _gl.Uniform1(_gl.GetUniformLocation(_texQuadProgram, "texture1"), 0);
        _gl.ActiveTexture(TextureUnit.Texture0);

        //delete individual shaders
        _gl.DeleteShader(texVert);
        _gl.DeleteShader(texFrag);
        _gl.DeleteShader(solidVert);
        _gl.DeleteShader(solidFrag);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _window.Resize += size => 
        {
            _gl.Viewport(size);
        };
    }
    protected override Texture CreateTexture(Stream file, TextureOptions options)
    {
        return new OpenGLTexture(file, _gl, options);
    }

    static Matrix4X4<float> ModelMatrix(Vector2D<float> position, float rotation, Vector2D<float> size)
    {
        if (rotation == 0f)
        {
            return Matrix4X4.CreateScale(size.X, size.Y, 1f) * 
            Matrix4X4.CreateTranslation(position.X, position.Y, 0);
        }
        else 
        {
            return Matrix4X4.CreateScale(size.X, size.Y, 1f) * 
            RotationFromCenterRect(size, rotation) *
            Matrix4X4.CreateTranslation(position.X, position.Y, 0);
        }
    }
    static Matrix4X4<float> RotationFromCenterRect(Vector2D<float> size, float rotation) =>
        Matrix4X4.CreateTranslation(-0.5f * size.X, -0.5f * size.Y, 0) *
        Matrix4X4.CreateRotationZ(rotation) *
        Matrix4X4.CreateTranslation(0.5f * size.X, 0.5f * size.Y, 0);

    public override void RenderSprite(Texture texture, Vector2D<float> position, Vector2D<float> scale, float rotation, Vector4D<float> color)
    {
        if (texture is not OpenGLTexture tex)
        {
            throw new Exception($"invalid texture type: expected OpenGLTexture and got {texture.GetType().Name}");
        }
        _gl.UseProgram(_texQuadProgram);
        _gl.BindVertexArray(_quadVao);

        //TODO: Use Uniform Buffer Objects
        var projectionMatrix = Matrix4X4.CreateOrthographicOffCenter(0f, Size.X, Size.Y, 0f, -100f, 100f);
        _gl.UniformMatrix4(_gl.GetUniformLocation(_texQuadProgram, "projection"), 1, false, (float*)&projectionMatrix);

        var actualSize = new Vector2D<float>(tex.Size.X * scale.X, tex.Size.Y * scale.Y);
        var modelMatrix = ModelMatrix(position, rotation, actualSize);

        _gl.UniformMatrix4(_gl.GetUniformLocation(_texQuadProgram, "model"), 1, false, (float*)&modelMatrix);

        var c = (Vector4) color;
        _gl.Uniform4(_gl.GetUniformLocation(_texQuadProgram, "color"), ref c);
        
        tex.Use();
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);  
    }

    public override void RenderRect(Vector2D<float> position, Vector2D<float> size, float rotation, Vector4D<float> color)
    {
        _gl.UseProgram(_solidProgram);
        _gl.BindVertexArray(_quadVao);
        //TODO: Use UBO to buffer a proj mat?
        var projectionMatrix = Matrix4X4.CreateOrthographicOffCenter(0f, Size.X, Size.Y, 0f, -100f, 100f);
        _gl.UniformMatrix4(_gl.GetUniformLocation(_solidProgram, "projection"), 1, false, (float*)&projectionMatrix);

        var modelMatrix = ModelMatrix(position, rotation, size);
        _gl.UniformMatrix4(_gl.GetUniformLocation(_solidProgram, "model"), 1, false, (float*)&modelMatrix);

        var c = (Vector4) color;
        _gl.Uniform4(_gl.GetUniformLocation(_solidProgram, "color"), ref c);
        
        _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);  
    }

    public override void Clear(Vector4D<float> color)
    {
        _gl.ClearColor(color * 255);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    public override void Dispose()
    {
        _gl.DeleteProgram(_texQuadProgram);
        _gl.DeleteProgram(_solidProgram);
        _gl.DeleteBuffer(_quadVbo);
        _gl.DeleteVertexArray(_quadVao);
        foreach(var (_, tex) in _textures)
        {
            tex.Dispose();
        }
    }
}
