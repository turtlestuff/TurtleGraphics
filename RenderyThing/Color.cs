namespace RenderyThing;

public readonly struct ColorF
{
    readonly Vector4 _val;

    public float R => _val.X;
    public float G => _val.Y;
    public float B => _val.Z;
    public float A => _val.W;

    public ColorF(Vector4 val)
    {
        _val = val;
    }
}