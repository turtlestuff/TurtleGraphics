using System.Drawing;
using System.Numerics;
using RenderyThing;
using Silk.NET.Input;
using Silk.NET.Windowing;

var numTurtles = 1000;
var worldSize = 1000;
var camera = Vector2.Zero;

IWindow window;
Renderer renderer = default!;
IInputContext input = default!;
var turtles = new (Vector2 Pos, float Angle, Vector4 Col, Vector2 Dir, float Rot)[numTurtles]; 

var options = WindowOptions.Default;
options.Size = new(800, 600);
options.Title = "RenderyThing Test Project";
window = Window.Create(options);

void OnLoad()
{
    renderer = Renderer.GetApi(window);
    using var fs = typeof(Program).Assembly.GetManifestResourceStream("ExampleProject.turtle.png") ?? throw new Exception("turtle.png not found");
    var tex = renderer.AddTexture(fs, "turtle", new() { ScalingType = ScalingType.NearestNeighbor });

    for (var i = 0; i < numTurtles; i++)
    {
        var x = Random.Shared.Next(0, worldSize);
        var y = Random.Shared.Next(0, worldSize);
        var col = Color.FromArgb((int) ((uint) Random.Shared.Next() | 0xFF000000)).ToVector4();
        var mov = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle()) *
            (Random.Shared.NextSingle() - 0.5f) * 10f;
        var rot = (Random.Shared.NextSingle() - 0.5f) * 0.5f;
        turtles[i] = (new(x, y), 0f, col, mov, rot);
    }

    input = window.CreateInput();
    input.Mice[0].Scroll += (mouse, wheel) => { renderer.Scale += wheel.Y * 0.2f; };
}

var dragging = false;
var lastMousePos = Vector2.Zero;

void OnUpdate(double deltaTime)
{
    var tex = renderer.GetTexture("turtle");

    var mouse = input.Mice[0];
    if (mouse.IsButtonPressed(MouseButton.Left))
    {
        if (dragging)
        {
            camera += (lastMousePos - mouse.Position) / renderer.Scale;
        }
        else
        {
            mouse.Cursor.CursorMode = CursorMode.Disabled;
            dragging = true;
        }
        lastMousePos = mouse.Position;
    }
    else
    {
        dragging = false;
        mouse.Cursor.CursorMode = CursorMode.Normal;
    }

    for (var i = 0; i < numTurtles; i++)
    {
        ref var turtle = ref turtles[i];
        if (turtle.Pos.X < 0 || turtle.Pos.X > worldSize)
            turtle.Dir.X *= -1;
        if (turtle.Pos.Y < 0 || turtle.Pos.Y > worldSize)
            turtle.Dir.Y *= -1;
        turtle.Pos += turtle.Dir;
        turtle.Angle = MathF.IEEERemainder(turtle.Angle + turtle.Rot, MathF.Tau);
    }
}

void OnRender(double deltaTime)
{
    renderer.Clear(Color.CornflowerBlue.ToVector4());
    var tex = renderer.GetTexture("turtle");
    for (var i = 0; i < numTurtles; i++)
    {
        ref var turtle = ref turtles[i];
        renderer.RenderSprite(tex, turtle.Pos - camera, Vector2.One, turtle.Angle, turtle.Col);
    }
}

window.Load += OnLoad;
window.Update += OnUpdate;
window.Render += OnRender;

window.Run();

