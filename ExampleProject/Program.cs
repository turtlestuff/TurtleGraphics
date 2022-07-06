using System.Drawing;
using RenderyThing;
using Silk.NET.Maths;
using Silk.NET.Windowing;

IWindow window;
Renderer? renderer = null;
RenderQueue queue = new();

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
    renderer.AddTexture(fs, "turtle", new() { ScalingType = ScalingType.NearestNeighbor });
}

void OnUpdate(double deltaTime)
{
    if (renderer is null) 
        return;
    var turtleTex = renderer.GetTexture("turtle");
    queue.QueueSprite(new(turtleTex, new(20,20), 0, Vector2D<float>.One, Vector4D<float>.One));
    queue.QueueSprite(new(turtleTex, new(80,80), 0, Vector2D<float>.One * 2, new(0, 1, 0, 1)));
    queue.QueueSprite(new(turtleTex, new(10,200), 0, Vector2D<float>.One, new(1, 0, 1, 1)));
}

void OnRender(double deltaTime)
{
    if (renderer is null) return;
    renderer.Clear(Color.CornflowerBlue.ToVector4());
    renderer.Render(queue);
}
