using Silk.NET.OpenGL;

namespace TurtleGraphics.OpenGL;

unsafe readonly struct ShaderProgram : IDisposable
{   
    readonly GL _gl;

    public uint Handle { get; }

    readonly int _projMatrixLocation;
    readonly int _modelMatrixLocation;
    readonly int _colorLocation;
    public ShaderProgram(GL gl, Stream vertSrc, Stream fragSrc)
    {
        static uint CreateShader(GL gl, ShaderType type, string source)
        {
            var shader = gl.CreateShader(type);
            gl.ShaderSource(shader, source);
            gl.CompileShader(shader);
            var info = gl.GetShaderInfoLog(shader);
            if (info != "") 
                throw new RendererException("shader compilation failed: " + info);
            return shader;
        }

        _gl = gl;
        using StreamReader vertSrcReader = new(vertSrc),
            fragSrcReader = new(fragSrc);
        var vert = CreateShader(gl, ShaderType.VertexShader, vertSrcReader.ReadToEnd());
        var frag = CreateShader(gl, ShaderType.FragmentShader, fragSrcReader.ReadToEnd());

        var prog = gl.CreateProgram();
        _gl.AttachShader(prog, vert);
        _gl.AttachShader(prog, frag);
        _gl.LinkProgram(prog);

        _gl.DeleteShader(vert);
        _gl.DeleteShader(frag);

        _gl.GetProgram(prog, ProgramPropertyARB.LinkStatus, out var status);
        if (status == 0)
            throw new RendererException("linking of shader program failed: " + _gl.GetProgramInfoLog(prog));

        _projMatrixLocation = _gl.GetUniformLocation(prog, "projection");
        _modelMatrixLocation = _gl.GetUniformLocation(prog, "model");
        _colorLocation = _gl.GetUniformLocation(prog, "color");

        Handle = prog;
    }
    
    public void SetModel(Matrix4x4* matrix) => _gl.UniformMatrix4(_modelMatrixLocation, 1, false, (float*) matrix);
    public void SetProjection(Matrix4x4* matrix) => _gl.UniformMatrix4(_projMatrixLocation, 1, false, (float*) matrix);
    public void SetColor(ref Vector4 col) => _gl.Uniform4(_colorLocation, ref col);

    public void Use() => _gl.UseProgram(Handle);

    public void Dispose()
    {
        _gl.DeleteProgram(Handle);
    }
}
