using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;
    private readonly MiniMapComponent _miniMap;

    public BattleGridScene()
    {
        _map = Add(new MapComponent(new(30f, 10f, 200f, 200f)));
        _miniMap = Add(new MiniMapComponent(ActiveCamera, new(1200f, 500f, 100f, 100f)));
    }
}