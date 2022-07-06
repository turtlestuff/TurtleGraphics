namespace RenderyThing;

public sealed class RenderQueue
{
    List<RenderSprite> _spriteQueue = new();

    public void ClearQueue()
    {
        _spriteQueue = new();
    }

    public void QueueSprite(RenderSprite sprite) => _spriteQueue.Add(sprite);

    //is this even a good idea???
    public void Finalize(out List<RenderSprite> sq)
    {
        sq = _spriteQueue;
        ClearQueue();
    }
}
