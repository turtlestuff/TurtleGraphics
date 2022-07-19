using System.Runtime.CompilerServices;
using System.Text;
using static StbTrueTypeSharp.StbTrueType; //StbTrueType.stbtt_Whatever is a mouthful

namespace RenderyThing.OpenGL;

unsafe class GLStbttFont : Font
{
    readonly byte[] _data;
    readonly byte* _dataPtr;
    readonly stbtt_fontinfo _fontInfo;

    public GLStbttFont(Stream stream)
    {
        var size = checked((int) stream.Length); //if your font file is larger than 3GB(?)
        _data = GC.AllocateArray<byte>(size, pinned: true);
        _dataPtr = (byte*) Unsafe.AsPointer(ref _data[0]); //Since the array is pinned, there shouldn't be a problem.
        stream.Read(_data, 0, size);
        _fontInfo = new();
        stbtt_InitFont(_fontInfo, _dataPtr, stbtt_GetFontOffsetForIndex(_dataPtr, 0));
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
        
    public float ScaleForPixelHeight(float height) => stbtt_ScaleForPixelHeight(_fontInfo, height);


    public override void Dispose()
    {
        _fontInfo.Dispose();
    }
}