using Silk.NET.OpenGL;

namespace TurtleGraphics.OpenGL;

unsafe readonly struct VertexArrayObject : IDisposable
{
    readonly GL _gl;
    readonly uint _handle;

    public VertexArrayObject(GL gl, in VertexBufferObject vbo)
    {
        _gl = gl;
        _handle = gl.GenVertexArray();
        Bind();
        vbo.Bind();
    }

    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    public void VertexAttribPointer(uint index, int size, VertexAttribPointerType type, bool normalize, uint stride, uint offset)
    {
        _gl.VertexAttribPointer(index, size, type, normalize, stride * sizeof(float), (void*) (offset * sizeof(float)));
        _gl.EnableVertexAttribArray(index);
    }
    
    public void Dispose()
    {
        _gl.DeleteVertexArray(_handle);
    }
}
