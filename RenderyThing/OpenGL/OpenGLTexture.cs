using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;

namespace RenderyThing.OpenGL;

public unsafe sealed class OpenGLTexture : Texture
{
    readonly uint _handle;
    readonly GL _gl;
    public OpenGLTexture(Stream fileStream, GL gl, TextureOptions options, PixelFormat format = PixelFormat.Rgba) : base(options)
    {
        _gl = gl;
        _handle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
        using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(fileStream);
        Span<byte> span = new byte[sizeof(Rgba32) * img.Width * img.Height];
        img.CopyPixelDataTo(span);
        fixed (byte* data = span)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint) img.Width, (uint) img.Height, 0, 
                format, PixelType.UnsignedByte, data);
        }
        var scaleFilter = (int) (Options.ScalingType == ScalingType.Linear ? GLEnum.Linear : GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, scaleFilter);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, scaleFilter);
        
        Size = new(img.Width, img.Height);
    }
       
    
    public void Use()
    {
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }


    public override void Dispose()
    {
        _gl.DeleteTexture(_handle);
    }
}
