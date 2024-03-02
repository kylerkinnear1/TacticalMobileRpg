using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public MapComponent(RectF bounds) : base(bounds)
    {
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.Fill(Bounds.Size);
    }
}