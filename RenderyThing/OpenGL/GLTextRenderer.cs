using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using Silk.NET.OpenGL;
using Silk.NET.SDL;

namespace RenderyThing.OpenGL;

unsafe class GLTextRenderer
{
    static readonly float[] quadVertices =
    {
    //  X     Y
        0.0f, 0.0f,
        1.0f, 0.0f,
        0.0f, 1.0f,
    
        1.0f, 0.0f,
        0.0f, 1.0f,
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

    float _scale;

    public GLTextRenderer(GL gl, OpenGLRenderer renderer)
    {
        _gl = gl;
        _renderer = renderer;
        using var vert = GLHelper.GetResStream("Shaders.fontQuad.vert");
        using var frag = GLHelper.GetResStream("Shaders.fontQuad.frag");
        _fontShader = new(gl, vert, frag);

        _fontQuadVbo = new VertexBufferObject(_gl, BufferUsageARB.DynamicDraw);
        _fontQuadVao = new VertexArrayObject(_gl, _fontQuadVbo);

        _fontQuadVbo.Bind();
        _fontQuadVbo.BufferData(quadVertices.AsSpan());

        _fontQuadVao.Bind();
        //puts the vertex data itself first, then the UV data. makes it easier to copy over afterwards
        _fontQuadVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);
        _fontQuadVao.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2, 12);
    }

    public void UpdateProjectionMatrix(Vector2D<int> newFBSize, float scale)
    {
        _fontShader.Use();
        var projMat = Matrix4x4.CreateOrthographicOffCenter(left: 0, right: newFBSize.X, top: 0,  bottom: newFBSize.Y, zNearPlane: -1f, zFarPlane: 1f);
        _fontShader.SetProjection(&projMat);
        _scale = scale;
    }
    public void RenderAtlas(GLStbttFont font)
    {
        _renderer.RenderSprite(new OpenGLTexture(font._altasTexHandle, new((int) font._atlasSize), _gl, new()), Vector2.Zero, Vector2.One, 0f, new(1,1,1,0.5f));
    }

    public static Vector2 MeasureString(string text, GLStbttFont font, float size)
    {
        var fontScale = font.ScaleForMappingEmToPixels(size);
        font.GetFontVMetrics(out var asc, out var des, out var lg);
        var ascent = asc * fontScale;
        var descent = des * fontScale;
        var lineGap = lg * fontScale;
        var height = ascent - descent;
        var glyphs = text.EnumerateRunes().Select(font.FindGlyphIndex).ToArray();
        var width = 0f;
        for (var i = 0; i < glyphs.Length; i++)
        {
            var glyph = glyphs[i];
            font.GetGlyphHMetrics(glyph, out var aw, out var lsb);
            var advanceWidth = aw * fontScale;
            var leftSideBearing = lsb * fontScale;
            if (i < glyphs.Length - 1)
            {
                var nextGlyph = glyphs[i + 1];
                var kern = font.GetGlyphKernAdvance(glyph, nextGlyph);
                advanceWidth += kern * fontScale;
            }
            width += advanceWidth;
        }
        return new(width, height);
    }

    public void Render(string text, GLStbttFont font, float size, Vector2 position, ref Vector4 color, out Vector2 outSize)
    {
        Span<float> newUVs = stackalloc float[12];
        var pxSize = size * _scale;
        var pxPos = position * _scale;
        var fontScale = font.ScaleForMappingEmToPixels(pxSize);
        font.GetFontVMetrics(out var asc, out var des, out var lg);
        var ascent = asc * fontScale;
        var descent = des * fontScale;
        var lineGap = lg * fontScale;
        var height = (ascent - descent) / _scale;

        _fontQuadVao.Bind();
        _fontQuadVbo.Bind();
        _fontShader.Use();
        font.UseAtlasTexture();

        var width = 0f;
        var glyphs = text.EnumerateRunes().Select(font.FindGlyphIndex).ToArray();
        var textPos = new Vector2(pxPos.X, pxPos.Y + ascent);
        for (var i = 0; i < glyphs.Length; i++)
        {
            var glyph = glyphs[i];
            var entry = font.GetOrCreateGlyphAtlasEntry(glyph, pxSize);
            font.GetGlyphHMetrics(glyph, out var aw, out var lsb);
            var advanceWidth = aw * fontScale;
            var leftSideBearing = lsb * fontScale;
            if (i < glyphs.Length - 1)
            {
                var nextGlyph = glyphs[i + 1];
                var kern = font.GetGlyphKernAdvance(glyph, nextGlyph);
                advanceWidth += kern * fontScale;
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

            _fontQuadVbo.BufferSubData(offset: 12, newUVs);
            _fontQuadVao.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2, 0);
            _fontQuadVao.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2, 12);

            var mat = GLHelper.ModelMatrix(new(textPos.X + leftSideBearing, textPos.Y + entry.Offset.Y), 0f, entry.Size);

            _fontShader.SetModel(&mat);
            _fontShader.SetColor(ref color);

            _gl.DrawArrays(PrimitiveType.Triangles, 0, 6);

            textPos.X += advanceWidth;
            width += advanceWidth / _scale;
        }
        outSize = new(width, height);
    }
}