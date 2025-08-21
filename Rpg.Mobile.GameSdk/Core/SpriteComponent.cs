using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Images;

namespace Rpg.Mobile.GameSdk.Core;

public class SpriteComponent(IImage sprite) : SpriteComponentBase(sprite);

public abstract class SpriteComponentBase(IImage sprite) : ComponentBase(new(0, 0, sprite.Width, sprite.Height))
{
    public IImage Sprite { get; set; } = sprite;
    public float Scale { get; private set; } = 1f;

    public override void Update(float deltaTime) => Bounds = Sprite.GetBounds(Bounds.Location, Scale);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.DrawImage(Sprite, Scale);
    }

    public void UpdateScale(float scale)
    {
        Scale = scale;
        Bounds = Sprite.GetBounds(scale);
    }
}
