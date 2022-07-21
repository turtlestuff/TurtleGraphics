using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Silk.NET.OpenGL;

namespace RenderyThing.OpenGL;

unsafe class GLTextRenderer
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
        //UV
        0.0f, 0.0f,
        0.0f, 0.0f,
        0.0f, 0.0f,
        
        0.0f, 0.0f,
        0.0f, 0.0f,
        0.0f, 0.0f,
    };


    readonly GL _gl;
    readonly OpenGLRenderer _renderer;
    readonly ShaderProgram _fontShader;
    readonly VertexArrayObject _fontQuadVao;
    readonly VertexBufferObject _fontQuadVbo;

    Stream GetResStream(string path) => 
        GetType().Assembly.GetManifestResourceStream($"RenderyThing.OpenGL.{path}") ?? throw new FileNotFoundException($"{path} not found");

    public GLTextRenderer(GL gl, OpenGLRenderer renderer)
    {
        _gl = gl;
        _renderer = renderer;
        _fontShader = new(gl, GetResStream("Shader.fontQuad.vert"), GetResStream("Shader.fontQuad.frag"));

        _fontQuadVbo = new VertexBufferObject(_gl, BufferUsageARB.StaticDraw);
        _fontQuadVao = new VertexArrayObject(_gl, _fontQuadVbo);

        _fontQuadVbo.Bind();
        _fontQuadVbo.BufferData(quadVertices.AsSpan());

        _fontQuadVao.Bind();
        //puts the vertex data itself first, then the UV data. makes it easier to copy over afterwards
        _fontQuadVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);
        _fontQuadVao.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2, 12);
    }

    public void Render(string text, GLStbttFont font, float size, Vector2 position)
    {
        var scale = font.ScaleForPixelHeight(size);
        font.GetFontVMetrics(out var asc, out var des, out var lg);
        var ascent = asc * scale;
        var descent = des * scale;
        var lineGap = lg * scale;

        var glyphs = text.EnumerateRunes().Select(font.FindGlyphIndex).ToArray();
        var pos = new Vector2(position.X, position.Y + ascent);
        for (var i = 0; i < glyphs.Length; i++)
        {
            var glyph = glyphs[i];
            var atlas = font.GetOrCreateGlyphAtlasEntry(glyph, size);
            
        }
    }
}