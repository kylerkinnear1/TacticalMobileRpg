using Rpg.Mobile.GameSdk.Inputs;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.UserInterface;

public class MouseCoordinateComponent : TextboxComponent
{
    private readonly IMouse _mouse;

    public MouseCoordinateComponent(IMouse mouse, RectF bounds) : base(bounds)
    {
        _mouse = mouse;

        CornerRadius = 2f;
    }

    public override void Update(float deltaTime)
    {
        var screenCursor = _mouse.GetScreenMousePosition();
        var windowCursor = _mouse.GetRelativeClientPosition();
        Label = new[]
        {
            $"Screen: {screenCursor.X},{screenCursor.Y}",
            $"Client: {windowCursor.X},{windowCursor.Y}",
        }.JoinLines(true);
    }
}
