using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
using static Rpg.Mobile.App.Game.Battling.Components.BattleComponent;

namespace Rpg.Mobile.App.Game.Battling.Components.States;

public class MovingState : IState
{
    private readonly BattleComponent _component;
    private readonly BattleData _data;
    private readonly IPathCalculator _path;

    public MovingState(BattleComponent component, BattleData data, IPathCalculator path)
    {
        _component = component;
        _data = data;
        _path = path;
    }

    public void Enter()
    {
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var walkableTiles = _path
            .CreateFanOutArea(_data.ActiveUnitStartPosition, _data.Map.Corner, _data.CurrentUnit.Stats.Movement)
            .Where(x => x == _data.ActiveUnitStartPosition ||
                        !_data.UnitCoordinates.ContainsValue(x) &&
                        _data.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();

        _data.WalkableTiles = walkableTiles;
    }

    public void Execute(float deltaTime)
    {
        if (_component.CurrentUnitTween is not null)
        {
            _component.CurrentUnit.Position = _component.CurrentUnitTween.Advance(deltaTime);
            _component.CurrentUnitTween = _component.CurrentUnitTween.IsComplete ? null : _component.CurrentUnitTween;
        }

        var walkShadows = _data.WalkableTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _component.MoveShadow.Shadows.Set(walkShadows);
    }

    public void Leave()
    {
        _data.WalkableTiles.Clear();
        _component.MoveShadow.Shadows.Clear();

        _component.Units[_data.CurrentUnit].HealthBar.HasGone = true;

        Bus.Global.Unsubscribe<TileClickedEvent>(TileClicked);
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_data.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _data.UnitCoordinates[_data.CurrentUnit] = evnt.Tile;
        var finalTarget = _component.GetPositionForTile(evnt.Tile, _component.CurrentUnit.Bounds.Size);
        _component.CurrentUnitTween = _component.CurrentUnit.Position.SpeedTween(500f, finalTarget);
        Bus.Global.Publish(new UnitMovedEvent(_data.CurrentUnit, evnt.Tile));
    }
}
