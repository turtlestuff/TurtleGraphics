using System.Drawing;
using System.Numerics;
using RenderyThing;
using Silk.NET.Input;
using Silk.NET.Windowing;

var numTurtles = 1000;
IWindow window;
Renderer? renderer = null;
var turtles = new (Vector2 Pos, float Angle, Vector4 Col, Vector2 Dir, float Rot)[numTurtles]; 

var options = WindowOptions.Default;
options.Size = new(800, 600);
options.Title = "RenderyThing testing project";
window = Window.Create(options);

window.Load += OnLoad;
window.Update += OnUpdate;
window.Render += OnRender;

window.Run();


void OnLoad()
{
    renderer = Renderer.GetApi(window);
    using var fs = typeof(Program).Assembly.GetManifestResourceStream("ExampleProject.turtle.png") ?? throw new Exception("turtle.png not found");
    var tex = renderer.AddTexture(fs, "turtle", new() { ScalingType = ScalingType.NearestNeighbor });

    for (var i = 0; i < numTurtles; i++)
    {
        var x = Random.Shared.Next(0, renderer.Size.X - tex.Size.X);
        var y = Random.Shared.Next(0, renderer.Size.Y - tex.Size.Y);
        var col = Color.FromArgb((int) ((uint) Random.Shared.Next() | 0xFF000000)).ToVector4();
        var mov = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle()) *
            (Random.Shared.NextSingle() - 0.5f) * 10f;
        var rot = (Random.Shared.NextSingle() - 0.5f) * 0.5f;
        turtles[i] = (new(x, y), 0f, col, mov, rot);
    }
}

void OnUpdate(double deltaTime)
{
    if (renderer is null) return;
    var tex = renderer.GetTexture("turtle");

    var input = window.CreateInput();
    var mouse = input.Mice[0];
    if (mouse.IsButtonPressed(MouseButton.Left))
    {
        renderer.Scale = (Math.Clamp(mouse.Position.X, 0, window.Size.X) / window.Size.X) + 1;
    }

    for (var i = 0; i < numTurtles; i++)
    {
        ref var turtle = ref turtles[i];
        turtle.Pos += turtle.Dir;
        if (turtle.Pos.X < 0 || turtle.Pos.X > renderer.Size.X - tex.Size.X)
            turtle.Dir.X *= -1;
        if (turtle.Pos.Y < 0 || turtle.Pos.Y > renderer.Size.Y - tex.Size.Y)
            turtle.Dir.Y *= -1;
        turtle.Angle = MathF.IEEERemainder(turtle.Angle + turtle.Rot, MathF.Tau);
    }
}

void OnRender(double deltaTime)
{

    if (renderer is null) return;
    renderer.Clear(Color.CornflowerBlue.ToVector4());
    var tex = renderer.GetTexture("turtle");
    for (var i = 0; i < numTurtles; i++)
    {
        ref var turtle = ref turtles[i];
        renderer.RenderSprite(tex, turtle.Pos, Vector2.One, turtle.Angle, turtle.Col);
    }

}
