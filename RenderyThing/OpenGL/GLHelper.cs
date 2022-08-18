namespace TurtleGraphics.OpenGL;

static class GLHelper
{
    public static Stream GetResStream(string path) =>
        typeof(GLHelper).Assembly.GetManifestResourceStream($"RenderyThing.OpenGL.{path}") ?? throw new FileNotFoundException($"{path} not found");

    public static Matrix4x4 ModelMatrix(Vector2 position, float rotation, Vector2 size)
    {
        if (rotation == 0f)
        {
            return Matrix4x4.CreateScale(size.X, size.Y, 1f) *
            Matrix4x4.CreateTranslation(position.X, position.Y, 0);
        }
        else
        {
            return Matrix4x4.CreateScale(size.X, size.Y, 1f) *
            RotationFromCenterRect(size, rotation) *
            Matrix4x4.CreateTranslation(position.X, position.Y, 0);
        }
    }

    public static Matrix4x4 RotationFromCenterRect(Vector2 size, float rotation) =>
        Matrix4x4.CreateTranslation(-0.5f * size.X, -0.5f * size.Y, 0) *
        Matrix4x4.CreateRotationZ(rotation) *
        Matrix4x4.CreateTranslation(0.5f * size.X, 0.5f * size.Y, 0);
}