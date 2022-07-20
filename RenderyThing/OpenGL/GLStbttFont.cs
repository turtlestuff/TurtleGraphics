using System.Runtime.CompilerServices;
using System.Text;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using static StbTrueTypeSharp.StbTrueType; //StbTrueType.stbtt_Whatever is a mouthful

namespace RenderyThing.OpenGL;

unsafe class GLStbttFont : Font
{
    readonly struct AtlasEntry
    {
        internal readonly Rectangle<float> Rectangle;
    }

    readonly GL _gl;

    readonly byte[] _fontData;
    readonly byte* _fontDataPtr;
    readonly stbtt_fontinfo _fontInfo;

    readonly Dictionary<(int Glyph, float Size), AtlasEntry> _atlasEntries;
    readonly uint _atlasSize = 2048;
    readonly uint _altasTexHandle;
    int _currentAtlasX = 0;
    int _currentAtlasY = 0;
    int _nextAtlasY = 0;


    public GLStbttFont(GL gl, Stream stream)
    {
        _gl = gl;
        var size = checked((int) stream.Length); //if your font file is larger than 3GB(?)
        _fontData = GC.AllocateArray<byte>(size, pinned: true);
        _fontDataPtr = (byte*) Unsafe.AsPointer(ref _fontData[0]); //Since the array is pinned, there shouldn't be a problem.
        stream.Read(_fontData, 0, size);
        _fontInfo = new();
        stbtt_InitFont(_fontInfo, _fontDataPtr, stbtt_GetFontOffsetForIndex(_fontDataPtr, 0));

        _altasTexHandle = _gl.GenTexture();
        UseAtlasTexture();
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, _atlasSize, _atlasSize, 0, PixelFormat.Red, 
            PixelType.UnsignedByte, pixels: null); //generates an empty texture.

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Nearest); 
            //using nearest should avoid ugly things around the borders
    }

    public int FindGlyphIndex(Rune codepoint) => stbtt_FindGlyphIndex(_fontInfo, codepoint.Value);

    public void GetFontVMetrics(out int ascent, out int descent, out int lineGap)
    {
        fixed (int* asc = &ascent, des = &descent, lg = &lineGap)
            stbtt_GetFontVMetrics(_fontInfo, asc, des, lg);
    }

    public void GetGlyphBitmapBox(int glyphIndex, float scaleX, float scaleY, out int ix0, out int iy0, out int ix1, out int iy1)
    {
        fixed (int* x0 = &ix0, y0 = &iy0, x1 = &ix1, y1 = &iy1)
            stbtt_GetGlyphBitmapBox(_fontInfo, glyphIndex, scaleX, scaleY, x0, y0, x1, y1);
    }

    public void GetGlyphHMetrics(int glyphIndex, out int advanceWidth, out int leftSideBearing)
    {
        fixed (int* aw = &advanceWidth, lsb = &leftSideBearing)
            stbtt_GetGlyphHMetrics(_fontInfo, glyphIndex, aw, lsb);
    }

    public int GetGlyphKernAdvance(int glyph1, int glyph2) => stbtt_GetGlyphKernAdvance(_fontInfo, glyph1, glyph2);

    public void MakeGlyphBitmap(Span<byte> output, int outWidth, int outHeight, int outStride, float scaleX, float scaleY, int glyphIndex)
    {
        fixed (byte* outPtr = output)
            stbtt_MakeGlyphBitmap(_fontInfo, outPtr, outWidth, outHeight, outStride, scaleX, scaleY, glyphIndex);
    }
    
    public byte[] GetGlyphBitmap(float scaleX, float scaleY, int glyphIndex, out int width, out int height, out int xOff, out int yOff)
    {
        //ive cheated here to make this managed
        GetGlyphBitmapBox(glyphIndex, scaleX, scaleY, out var ix0, out var iy0, out var ix1, out var iy1);
        width = ix1 - ix0;
        height = iy1 - iy0;
        xOff = ix0;
        yOff = iy0;

        var output = new byte[width * height];
        MakeGlyphBitmap(output, width, height, width, scaleX, scaleY, glyphIndex);
        return output;
    }

    public float ScaleForPixelHeight(float height) => stbtt_ScaleForPixelHeight(_fontInfo, height);

    

    public void UseAtlasTexture()
    {
        _gl.BindTexture(TextureTarget.Texture2D, _altasTexHandle);
    }


    public override void Dispose()
    {
        _gl.DeleteTexture(_altasTexHandle);
        _fontInfo.Dispose();
    }
}