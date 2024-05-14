using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Images;

namespace Rpg.Mobile.GameSdk.Core;

public class Sprite : SpriteComponentBase
{
    public Sprite(IImage sprite) : base(sprite) { }
}

public abstract class SpriteComponentBase : ComponentBase
{
    public IImage Sprite { get; set; }
    public float Scale { get; private set; } = 1f;

    protected SpriteComponentBase(IImage sprite) : base(new(0, 0, sprite.Width, sprite.Height))
    {
        Sprite = sprite;
    }

    public override void Update(float deltaTime) => Bounds = Sprite.GetBounds(Bounds.Location, Scale);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!Visible)
            return;
        canvas.DrawImage(Sprite, Scale);
    }

    public void UpdateScale(float scale)
    {
        Scale = scale;
        Bounds = Sprite.GetBounds(scale);
    }
}
