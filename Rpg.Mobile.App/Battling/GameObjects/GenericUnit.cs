using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class GenericUnitState
{
    public PointF Location { get; set; }
    public IImage Sprite { get; set; }
    public float Scale { get; set; }

    public GenericUnitState(PointF location, IImage sprite, float scale)
    {
        Location = location;
        Sprite = sprite;
        Scale = scale;
    }
}

public class GenericUnitGameObject : IGameObject
{
    private readonly GenericUnitState _thisState;
    private readonly BattleSceneState _sceneState;
    private int _currentPosition = 0;
    private DateTime _nextUpdate = DateTime.MinValue;
    private readonly int _totalPositions;

    public GenericUnitGameObject(GenericUnitState thisState, BattleSceneState sceneState)
    {
        _thisState = thisState;
        _sceneState = sceneState;

        _totalPositions = sceneState.Grid.RowCount * sceneState.Grid.ColumnCount;
    }

    public void Update(TimeSpan delta)
    {
        if (DateTime.Now < _nextUpdate)
        {
            return;
        }

        _nextUpdate = DateTime.Now.AddSeconds(1);
        _currentPosition = _currentPosition < _totalPositions - 1 ? _currentPosition + 1 : 0;

        var row = _currentPosition / _sceneState.Grid.ColumnCount;
        var col = _currentPosition % _sceneState.Grid.RowCount;

        var left = _sceneState.Grid.Size * row + _sceneState.Grid.Position.X;
        var top = _sceneState.Grid.Size * col + _sceneState.Grid.Position.Y;

        _thisState.Location = new(left, top);
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Draw(_thisState.Sprite, _thisState.Location, _thisState.Scale);
    }
}
