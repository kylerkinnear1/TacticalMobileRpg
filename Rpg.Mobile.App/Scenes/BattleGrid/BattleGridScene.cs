using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid;

public class BattleGridScene : SceneBase
{
    private readonly GridComponent _grid = new(new () { ColCount = 15, RowCount = 10, Position = new(30f, 100f)});

    public BattleGridScene()
    {
        Add(_grid);
    }
}