using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class SelectingAttackTargetStep(
    BattleData _data,
    IEventBus _bus,
    ISelectingAttackTargetCalculator _attackTargetCalculator,
    IPathCalculator _path) : ActivePhase.IStep
{
    public record StartedEvent(List<Point> AttackTargetTiles) : IEvent;

    public record AttackTargetSelectedEvent(int TargetId) : IEvent;

    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions =
        [
            _bus.Subscribe<TileClickedEvent>(TileClicked)
        ];

        var gridToUnit = _data.UnitCoordinates
            .ToLookup(x => x.Value, x => x.Key);

        var legalTargets = _path
            .CreateFanOutArea(
                _data.UnitCoordinates[_data.CurrentUnit().UnitId],
                _data.Map.Corner(),
                _data.CurrentUnit().Stats.AttackMinRange,
                _data.CurrentUnit().Stats.AttackMaxRange)
            .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y != _data.CurrentUnit().PlayerId))
            .ToList();

        _data.Active.AttackTargetTiles.Set(legalTargets);
        _bus.Publish(new StartedEvent(legalTargets));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
        _data.Active.AttackTargetTiles.Clear();
        _subscriptions.DisposeAll();
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_attackTargetCalculator.IsValidAttackTargetTile(evnt.Tile, _data))
            return;

        var enemy = _data
            .UnitsAt(evnt.Tile)
            .Single(x => x.PlayerId != _data.CurrentUnit().PlayerId);

        _bus.Publish(new AttackTargetSelectedEvent(enemy.UnitId));
    }
}