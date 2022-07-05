namespace RenderyThing;

public sealed class RenderQueue
{
    List<RenderSprite> spriteQueue = new();

    public void ClearQueue()
    {
        spriteQueue = new();
    }

    public void QueueSprite(RenderSprite sprite) => spriteQueue.Add(sprite);

    //is this even a good idea???
    public void Finalise(out List<RenderSprite> sq)
    {
        sq = spriteQueue;
        ClearQueue();
    }
}