using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using RenderyThing;
using RenderyThing.OpenGL;
using Silk.NET.Input;
using Silk.NET.Windowing;

var avgOver = 16;
var renderTimes = new double[avgOver];
var frameRates = new double[avgOver];
var avgIndex = 0;
var numTurtles = 1000;
var worldSize = 1000;
var camera = Vector2.Zero;

IWindow window;
Renderer renderer = default!;
IInputContext input = default!;
Font font = default!;
var turtles = new (Vector2 Pos, float Angle, Vector4 Col, Vector2 Dir, float Rot)[numTurtles]; 

var options = WindowOptions.Default;
options.Size = new(800, 600);
options.Title = "RenderyThing Test Project";

window = Window.Create(options);

void OnLoad()
{
    renderer = Renderer.GetApi(window);
    using var ts = typeof(Program).Assembly.GetManifestResourceStream("ExampleProject.turtle.png") ?? throw new Exception("turtle.png not found");
    using var fs = typeof(Program).Assembly.GetManifestResourceStream("ExampleProject.NotoSans.ttf") ?? throw new Exception("noto sans not found");
    var tex = renderer.AddTexture(ts, "turtle", new() { ScalingType = ScalingType.NearestNeighbor });
    font = renderer.CreateFont(fs);
    for (var i = 0; i < numTurtles; i++)
    {
        var x = Random.Shared.Next(tex.Size.X, worldSize - tex.Size.X);
        var y = Random.Shared.Next(tex.Size.Y, worldSize - tex.Size.Y);
        var col = Color.FromArgb((int) ((uint) Random.Shared.Next() | 0xFF000000)).ToVector4();
        var mov = new Vector2(Random.Shared.NextSingle() - 0.5f, Random.Shared.NextSingle() - 0.5f) *
            Random.Shared.NextSingle() * 10f * 60f;
        var rot = (Random.Shared.NextSingle() - 0.5f) * 0.5f * 60f;
        turtles[i] = (new(x, y), 0f, col, mov, rot);
    }

    input = window.CreateInput();
    input.Mice[0].Scroll += (mouse, wheel) => { renderer.Scale = Math.Clamp(renderer.Scale + wheel.Y * 0.2f, 0.2f, 4f); };
}

var dragging = false;
var lastMousePos = Vector2.Zero;

void OnUpdate(double deltaTime)
{
    var dT = (float) deltaTime;
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
        return; //Pause turtle movement while scrolling
    }
    else
    {
        dragging = false;
        mouse.Cursor.CursorMode = CursorMode.Normal;
    }

    for (var i = 0; i < numTurtles; i++)
    {
        ref var turtle = ref turtles[i];
        turtle.Pos += turtle.Dir * dT;
        if (turtle.Pos.X < 0 || turtle.Pos.X > worldSize - tex.Size.X)
        {
            turtle.Dir.X *= -1;
            turtle.Pos += turtle.Dir * dT;
        }
        if (turtle.Pos.Y < 0 || turtle.Pos.Y > worldSize - tex.Size.Y)
        {
            turtle.Dir.Y *= -1;
            turtle.Pos += turtle.Dir * dT;

        }
        turtle.Angle = MathF.IEEERemainder(turtle.Angle + turtle.Rot * dT, MathF.Tau);
    }
}

void OnRender(double deltaTime)
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    renderer.Clear(Color.Black.ToVector4());
    var tex = renderer.GetTexture("turtle");
    renderer.RenderRect(-camera, new(worldSize), 0, Color.CornflowerBlue.ToVector4());
    for (var i = 0; i < numTurtles; i++)
    {
        ref var turtle = ref turtles[i];
        var relPos = turtle.Pos - camera;
        var centerPos = relPos + ((Vector2)tex.Size) / 2f;
        //renderer.RenderLine(centerPos, centerPos + turtle.Dir / 2, 5, turtle.Col);
        renderer.RenderSprite(tex, relPos, Vector2.One, turtle.Angle, turtle.Col);
    }
    var str = $"Position: {camera.X}, {camera.Y}";
    var size = 16f;
    var textSize = renderer.MeasureText(str, font, size);
    var textPos = new Vector2((renderer.Size.X - textSize.X) / 2f, 0f);
    renderer.RenderRect(textPos, textSize, 0f, new(1f, 0.2f, 0f, 0.5f));
    renderer.RenderText(str, textPos, font, size, Vector4.One);
    stopwatch.Stop();
    renderTimes[avgIndex] = stopwatch.Elapsed.TotalMilliseconds;
    frameRates[avgIndex++] = 1 / deltaTime;
    if (avgIndex == avgOver)
    {   
        Console.WriteLine($"render time: {renderTimes.Average():F3} ms | frame rate: {frameRates.Average():F3} FPS (avg. over {avgOver} frames)");
        avgIndex = 0;
    }
}

void OnClosing()
{
    Console.WriteLine();
    renderer.Dispose();
}

window.Load += OnLoad;
window.Update += OnUpdate;
window.Render += OnRender;
window.Closing += OnClosing;
window.Run();
