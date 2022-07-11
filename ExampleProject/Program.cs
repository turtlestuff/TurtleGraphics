using System.Drawing;
using RenderyThing;
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
    renderer.RenderSprite(tex, new(30, 30), new(2), DateTime.Now.Millisecond / 159.22f , Color.GreenYellow.ToVector4());
    renderer.RenderRect(new(30,200), new(128, 64), DateTime.Now.Second / 9.55f, Color.OrangeRed.ToVector4());
}
