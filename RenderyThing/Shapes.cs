namespace RenderyThing;

public static class Shapes
{
    public static int LineVtxCount() => 6;
    public static void Line(Vector2 from, Vector2 to, float width, in Span<Vector2> vertices)
    {
        var lineVec = Vector2.Normalize(to - from);
        var normal = lineVec.Normal() * width / 2f;
        var from1 = from + normal;
        var from2 = from - normal;
        var to1 = to + normal;
        var to2 = to - normal;

        vertices[0] = from1;
        vertices[1] = from2;
        vertices[2] = to1;

        vertices[3] = to1;
        vertices[4] = to2;
        vertices[5] = from2;
    }

    public static int LinesMiterVtxCount(int pointsAmt, bool loop) => pointsAmt == 2 ? 6 : (loop ? pointsAmt : pointsAmt - 1) * 6;

    public static void LinesMiter(ReadOnlySpan<Vector2> points, float width, bool loop, in Span<Vector2> vertices)
    {
        if (points.Length < 2)
        {
            throw new RendererException("Array of points must have at least two points in order to make lines");
        }

        if (points.Length == 2)
        {
            Line(points[0], points[1], width, vertices);
        }

        var numberOfIterations = loop ? points.Length : points.Length - 1;

        var v = 0;
        var hWidth = width / 2f;

        for (var i = 0; i < numberOfIterations; i++)
        {
            var from = points[i];
            var to = i == points.Length - 1 ? points[0] : points[i + 1];
            Vector2 fromOffset;
            Vector2 toOffset;
            var line = Vector2.Normalize(to - from);
            var normal = line.Normal();
            if (!loop && i == 0)
            {
                fromOffset = normal * hWidth;
            }
            else 
            {
                var prev = i == 0 ? points[^1] : points[i - 1];
                var miter = Vector2.Normalize(Vector2.Normalize(from - prev) + line).Normal(); 
                var len = hWidth / Vector2.Dot(normal, miter);
                fromOffset = miter * len;
            }
            if (!loop && i >= points.Length - 2)
            {
                toOffset = normal * hWidth;
            }
            else
            {
                var next = i >= points.Length - 2 ? points[i - points.Length + 2] : points[i + 2];
                var miter = Vector2.Normalize(Vector2.Normalize(next - to) + line).Normal(); 
                var len = hWidth / Vector2.Dot(normal, miter);
                toOffset = miter * len;
            }

            vertices[v++] = from + fromOffset;
            vertices[v++] = from - fromOffset;
            vertices[v++] = to + toOffset;

            vertices[v++] = to + toOffset;
            vertices[v++] = to - toOffset;
            vertices[v++] = from - fromOffset; 
        }
    }

    public static int TraingulateConvexVtxCount(int pointsAmt) => (pointsAmt - 2) * 3;

    public static void TriangulateConvex(ReadOnlySpan<Vector2> points, in Span<Vector2> vertices)
    {
        var iterations = points.Length - 2;
        var c = 0;
        var first = points[0];
        for (var i = 0; i < iterations; i++)
        {
            vertices[c++] = first;
            vertices[c++] = points[i + 1];
            vertices[c++] = points[i + 2];
        }
    }

    public static void RegularNGonPoints(Vector2 center, float radius, int sides, float rotation, in Span<Vector2> points)
    {
        var angleDiff = MathF.Tau / sides;
        for (var i = 0; i < sides; i++)
        {
            var angle = MathF.IEEERemainder(rotation + angleDiff * i, MathF.Tau);
            var (sin, cos) = MathF.SinCos(angle);
            points[i] = center + new Vector2(cos, sin) * radius;
        }
    }

    public static int SolidRegularNGonVtxCount(int sides) => TraingulateConvexVtxCount(sides);

    public static void SolidRegularNGon(Vector2 center, float radius, int sides, float rotation, in Span<Vector2> vertices)
    {
        Span<Vector2> points = stackalloc Vector2[sides];
        RegularNGonPoints(center, radius, sides, rotation, points);
        TriangulateConvex(points, vertices);
    }

    public static int RegularNGonOutlineVtxCount(int sides) => LinesMiterVtxCount(sides, true);

    public static void RegularNGonOutline(Vector2 center, float radius, int sides, float rotation, float width, in Span<Vector2> vertices)
    {
        Span<Vector2> points = stackalloc Vector2[sides];
        RegularNGonPoints(center, radius, sides, rotation, points);
        LinesMiter(points, width, true, vertices);
    }

}