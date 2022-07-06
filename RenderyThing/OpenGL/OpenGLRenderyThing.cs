using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RenderyThing.OpenGL;

public unsafe sealed class OpenGLRenderer : Renderer
{
    //TODO: move to files!
    const string vertSource = 
    @"
        #version 330 core //Using version GLSL version 3.3
        layout (location = 0) in vec4 vPos;
        
        uniform mat4 model;
        uniform mat4 projection;
        
        void main()
        {
            gl_Position = projection * model * vec4(vPos.xy, 0.0, 1.0);
        }
    ";

    const string fragSource = 
    @"
        #version 330 core
        out vec4 color;
        void main()
        {
            color = vec4(100.0f/255.0f, 149.0f/255.0f, 237.0f/255.0f, 1.0f); //cornflower blue :)))))
        }
    ";
    
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
    readonly uint _shader;

    readonly int _modelUniform;
    readonly int _projectionUniform;

    public OpenGLRenderer(IWindow window) : base(window)
    {
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
        _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 2 * sizeof(float), null);
        _gl.EnableVertexAttribArray(0); 

        //create shaders
        var vert = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vert, vertSource);
        _gl.CompileShader(vert);
        var info = _gl.GetShaderInfoLog(vert);
        if(info != "") 
            throw new Exception("compilation of vertex shader failed: " + info);

        var frag = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(frag, fragSource);
        _gl.CompileShader(frag);
        _gl.GetShaderInfoLog(vert);
        if(info != "") 
            throw new Exception("compilation of fragment shader failed: " + info);

        _shader = _gl.CreateProgram();
        _gl.AttachShader(_shader, vert);
        _gl.AttachShader(_shader, frag);
        _gl.LinkProgram(_shader);

        _gl.GetProgram(_shader, ProgramPropertyARB.LinkStatus, out var status);
        if (status == 0)
            throw new Exception("linking of shader program failed: " + _gl.GetProgramInfoLog(_shader));
        
        //delete individual shaders
        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);

        _modelUniform = _gl.GetUniformLocation(_shader, "model");
        _projectionUniform = _gl.GetUniformLocation(_shader, "projection");
        if (_modelUniform == -1 || _projectionUniform == -1)
           throw new Exception("what.");
    }

    public override Texture AddTexture(Stream file, string name)
    {
        throw new NotImplementedException();
    }

    public override Texture GetTexture(string name)
    {
        throw new NotImplementedException();
    }

    public override void Render(RenderQueue queue)
    {
        queue.Finalize(out var sprites);
        _gl.UseProgram(_shader);
        _gl.BindVertexArray(_quadVao);
        var projectionMatrix = Matrix4X4<float>.Identity * Matrix4X4.CreateOrthographicOffCenter(0f, 800f, 600f, 0f, -100f, 100f);
        foreach (var sprite in sprites)
        {
            var modelMatrix = 
                Matrix4X4<float>.Identity * Matrix4X4.CreateScale(20f, 20f, 1f) * Matrix4X4.CreateTranslation(new Vector3D<float>(sprite.Position, 0f));
            _gl.UniformMatrix4(_projectionUniform, 1, false, (float*)&projectionMatrix);
            _gl.UniformMatrix4(_modelUniform, 1, false, (float*)&modelMatrix);
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);  
        }
    }

    public override void Dispose()
    {
        throw new NotImplementedException();
    }
}
