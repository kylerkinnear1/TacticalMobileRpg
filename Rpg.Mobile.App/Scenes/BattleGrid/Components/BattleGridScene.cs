using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;

    public BattleGridScene(IGraphicsView view) : base(view)
    {
        _map = Add(new MapComponent(new(30f, 10f, 200f, 200f)));
        ActiveCamera.Offset = new(400, 400);
    }
}