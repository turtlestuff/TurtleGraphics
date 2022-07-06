using RenderyThing;
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
}

void OnUpdate(double deltaTime)
{
    queue.QueueSprite(new(new(20,20), 0, new(1,1,1,1)));
    queue.QueueSprite(new(new(60,60), 0, new(1,1,1,1)));
    queue.QueueSprite(new(new(20,60), 0, new(1,1,1,1)));
}

void OnRender(double deltaTime)
{
    if (renderer is null) return;
    renderer.Render(queue);
}
