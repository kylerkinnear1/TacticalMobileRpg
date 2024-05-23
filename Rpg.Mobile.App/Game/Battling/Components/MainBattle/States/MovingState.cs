using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
using static Rpg.Mobile.App.Game.Battling.Components.MainBattle.MainBattleComponent;

namespace Rpg.Mobile.App.Game.Battling.Components.MainBattle.States;

public class MovingState : IBattleState
{
    private readonly MainBattleComponent _battle;
    private readonly BattleData _data;
    private readonly IPathCalculator _path;
    private readonly BattleMenuComponent _menu;

    public void Enter()
    {
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var walkableTiles = _path
            .CreateFanOutArea(_data.ActiveUnitStartPosition, _data.Map.Corner, _data.CurrentUnit.Stats.Movement)
            .Where(x => x == _data.ActiveUnitStartPosition ||
                        !_data.UnitCoordinates.ContainsValue(x) &&
                        _data.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();

        _data.ActiveUnitStartPosition = _data.UnitCoordinates[_data.CurrentUnit];
        _data.WalkableTiles = walkableTiles;

        _menu.SetButtons(
            new("Attack", _ => _battleService.ChangeBattleState(BattleStep.SelectingAttackTarget)),
            new("Magic", _ => _battleService.ChangeBattleState(BattleStep.SelectingSpell)),
            new("Wait", _ => _battleService.AdvanceToNextUnit()));
    }

    public void Execute(float deltaTime)
    {
        if (_battle.CurrentUnitTween is not null)
        {
            _battle.CurrentUnit.Position = _battle.CurrentUnitTween.Advance(deltaTime);
            _battle.CurrentUnitTween = _battle.CurrentUnitTween.IsComplete ? null : _battle.CurrentUnitTween;
        }

        var walkShadows = _data.WalkableTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _battle.MoveShadow.Shadows.Set(walkShadows);
    }

    public void Leave()
    {
        _data.WalkableTiles.Clear();
        _battle.MoveShadow.Shadows.Clear();

        Bus.Global.Unsubscribe<TileClickedEvent>(TileClicked);
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_data.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _data.UnitCoordinates[_data.CurrentUnit] = evnt.Tile;
        var finalTarget = _battle.GetPositionForTile(evnt.Tile, _battle.CurrentUnit.Bounds.Size);
        _battle.CurrentUnitTween = _battle.CurrentUnit.Position.SpeedTween(500f, finalTarget);
        Bus.Global.Publish(new UnitMovedEvent(_data.CurrentUnit, evnt.Tile));
    }
}
