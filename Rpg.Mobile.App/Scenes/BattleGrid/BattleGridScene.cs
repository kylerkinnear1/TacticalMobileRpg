using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid;

public class BattleGridScene : SceneBase
{
    private readonly GridComponent _grid = new(new () { ColCount = 15, RowCount = 10, Position = new (10f, 10f)});

    public BattleGridScene()
    {
        Add(_grid);
    }
}