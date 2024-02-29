using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class BattleGridScene : SceneBase
{
    private readonly MapState _map = new()
    {
        Grid = new GridState { ColCount = 10, RowCount = 8}
    };

    public BattleGridScene(IGraphicsView view) : base(view)
    {
        Add(new MapComponent(new(30f, 10f, 200f, 200f), _map));

        ActiveCamera.Offset = new PointF(400f, 600f);
    }
}