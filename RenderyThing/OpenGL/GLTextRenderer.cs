using System.ComponentModel;
using Silk.NET.OpenGL;

namespace RenderyThing.OpenGL;

unsafe class GLTextRenderer
{
    readonly GL _gl;
    
    public GLTextRenderer(GL gl)
    {
        _gl = gl;
    }
}