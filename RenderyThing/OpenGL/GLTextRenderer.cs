using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using Silk.NET.OpenGL;

namespace RenderyThing.OpenGL;

unsafe class GLTextRenderer
{
    static readonly float[] quadVertices =
    {
    //  X     Y
        0.0f, 0.0f,
        1.0f, 0.0f,
        0.0f, 1.0f,
    
        0.0f, 1.0f,
        1.0f, 0.0f,
        1.0f, 1.0f,
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
        _fontShader = new(gl, GetResStream("Shaders.fontQuad.vert"), GetResStream("Shaders.fontQuad.frag"));

        _fontQuadVbo = new VertexBufferObject(_gl, BufferUsageARB.DynamicDraw);
        _fontQuadVao = new VertexArrayObject(_gl, _fontQuadVbo);

        _fontQuadVbo.Bind();
        _fontQuadVbo.BufferData(quadVertices.AsSpan());

        _fontQuadVao.Bind();
        //puts the vertex data itself first, then the UV data. makes it easier to copy over afterwards
        _fontQuadVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);
        _fontQuadVao.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2, 12);
    }

    public void RenderAtlas(GLStbttFont font)
    {
        _renderer.RenderSprite(new OpenGLTexture(font._altasTexHandle, new((int) font._atlasSize), _gl, new()), Vector2.Zero, Vector2.One, 0f, Vector4.One);
    }

    public void Render(string text, GLStbttFont font, float size, Vector2 position, ref Vector4 color)
    {
        Span<float> newUVs = stackalloc float[12];

        _fontShader.Use();
        var projMat = _renderer.ProjectionMatrix;
        _fontShader.SetProjection(&projMat);

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
            var entry = font.GetOrCreateGlyphAtlasEntry(glyph, size);
            font.GetGlyphHMetrics(glyph, out var aw, out var lsb);
            var advanceWidth = aw * scale;
            var leftSideBearing = lsb * scale;
            if (i < glyphs.Length - 1)
            {
                var nextGlyph = glyphs[i + 1];
                var kern = font.GetGlyphKernAdvance(glyph, nextGlyph);
                advanceWidth += kern * scale;
            }
            
            newUVs[0] = entry.UVLeft; //eeeee
            newUVs[1] = entry.UVTop;  //eeeee

            newUVs[2] = entry.UVRight; //eeeee
            newUVs[3] = entry.UVTop;   //eeeee
            
            newUVs[4] = entry.UVLeft;  //eeeee
            newUVs[5] = entry.UVBottom;//eeeee
            

            newUVs[6] = entry.UVRight; //eeeee
            newUVs[7] = entry.UVTop;   //eeeee

            newUVs[8] = entry.UVLeft;  //eeeee
            newUVs[9] = entry.UVBottom;//eeeee

            newUVs[10] = entry.UVRight; //eeeee
            newUVs[11] = entry.UVBottom; //eeeeeeeeeeeeeeeewwwwwwwwwwwwww

            _fontQuadVao.Bind();
            _fontQuadVbo.Bind();
            _fontQuadVbo.BufferSubData(offset: 12, newUVs);
            _fontQuadVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);
            _fontQuadVao.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2, 12);

            var mat = OpenGLRenderer.ModelMatrix(new(pos.X + leftSideBearing, pos.Y + entry.Offset.Y), 0f, entry.Size);

            _fontShader.SetModel(&mat);
            _fontShader.SetColor(ref color);

            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);

                        _renderer.RenderRect(new(pos.X + leftSideBearing, pos.Y + entry.Offset.Y), entry.Size, 0f, Vector4.One);


            pos.X += advanceWidth;
        }
    }
}