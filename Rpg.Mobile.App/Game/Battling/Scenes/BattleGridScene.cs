using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.App.Game.Menu;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;
    private readonly MiniMapComponent _miniMap;
    private readonly ButtonComponent _attackButton;
    private readonly ButtonComponent _waitButton;

    public BattleGridScene()
    {
        _map = Add(new MapComponent(new(0f, 0f, 200f, 200f)));
        _miniMap = Add(new MiniMapComponent(ActiveCamera, new(1200f, 500f, 100f, 100f)) { IgnoreCamera = true });

        var buttonTop = 0f;
        _attackButton = Add(new ButtonComponent(new(1200f, buttonTop += 50f, 100f, 50f), "Attack", OnAttack) { IgnoreCamera = true });
        _waitButton = Add(new ButtonComponent(new(1200f, buttonTop += 60f, 100f, 50f), "Wait", OnWait) { IgnoreCamera = true });

        ActiveCamera.Offset = new PointF(-10f, -30f);
    }

    public void OnAttack(IEnumerable<PointF> touches)
    {

    }

    public void OnWait(IEnumerable<PointF> touches)
    {

    }
}
