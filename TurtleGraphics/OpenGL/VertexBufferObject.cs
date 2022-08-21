using Silk.NET.OpenGL;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace TurtleGraphics.OpenGL;

unsafe readonly struct VertexBufferObject : IDisposable
{
    readonly GL _gl;
    readonly uint _handle;
    readonly BufferUsageARB _bufferUsage;
    public VertexBufferObject(GL gl, BufferUsageARB bufferUsage)
    {
        _gl = gl;
        _handle = gl.GenBuffer();
        _bufferUsage = bufferUsage;
        Bind();
    }

    public void BufferData<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        _gl.BufferData(BufferTargetARB.ArrayBuffer, data, _bufferUsage);
    }

    public void BufferData(void* data, nuint length)
    {
        _gl.BufferData(BufferTargetARB.ArrayBuffer, length, data, _bufferUsage);
    }

    public void BufferSubData<T>(nint offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        _gl.BufferSubData(BufferTargetARB.ArrayBuffer, offset * sizeof(T), data);
    }

    public void Bind()
    {
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_handle);
    }
}
