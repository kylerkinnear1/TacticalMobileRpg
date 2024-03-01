using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.GameSdk;

public abstract class SpriteComponentBase : ComponentBase
{
    public IImage Sprite { get; set; }
    public float Scale { get; set; } = 1f;
    public bool IsVisible { get; set; } = true;

    protected SpriteComponentBase(IImage sprite) : base(new(0, 0, sprite.Width, sprite.Height))
    {
        Sprite = sprite;
    }

    public override void Update(TimeSpan delta) => Bounds = Sprite.GetBounds(Bounds.Location, Scale);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!IsVisible)
            return;

        canvas.Draw(Sprite, Scale);
    }
}
