using System.Drawing;
using System.Numerics;
using RenderyThing;
using Silk.NET.Maths;
using Silk.NET.Windowing;

IWindow window;
Renderer? renderer = null;

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
}

void OnRender(double deltaTime)
{
    if (renderer is null) return;
    renderer.Clear(Color.CornflowerBlue.ToVector4());
    var tex = renderer.GetTexture("turtle");
    renderer.RenderSprite(tex, new(300, 300), new(1), DateTime.Now.Millisecond / 1000f , Color.Red.ToVector4());
}
