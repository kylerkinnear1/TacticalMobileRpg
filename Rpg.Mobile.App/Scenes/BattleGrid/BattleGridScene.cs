using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid;

public class BattleGridScene : SceneBase
{
    private readonly MapState _map = new()
    {
        Grid = new GridState { ColCount = 10, RowCount = 8}
    };

    public BattleGridScene()
    {
        Add(new MapComponent(new(30f, 10f, 200f, 200f), _map));

        ActiveCamera.FocalPoint = new PointF(400f, 600f);
    }
}