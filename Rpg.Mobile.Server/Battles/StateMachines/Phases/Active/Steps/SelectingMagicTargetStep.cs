using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetStep(
    BattleData _data,
    IEventBus _bus,
    ISelectingMagicTargetCalculator _magicTargetCalculator,
    IPathCalculator _path) : ActivePhase.IStep
{
    public record StartedEvent(SpellData ActiveSpell, List<Point> SpellTargetTiles) : IEvent;
    public record MagicTargetSelectedEvent(Point Target) : IEvent;
    
    private BattleData Data => _data;

    private ISubscription[] _subscriptions = [];
    
    public void Enter()
    {
        _subscriptions =
        [
            _bus.Subscribe<TileClickedEvent>(TileClicked)
        ];
        
        var gridToUnit = _data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        var legalTargets = _path
            .CreateFanOutArea(
                Data.UnitCoordinates[_data.CurrentUnit().UnitId],
                Data.Map.Corner(),
                Data.Active.CurrentSpell!.MinRange,
                Data.Active.CurrentSpell.MaxRange)
            .Where(x =>
                !gridToUnit.Contains(x) ||
                Data.Active.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y != Data.CurrentUnit().PlayerId) ||
                 Data.Active.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y == Data.CurrentUnit().PlayerId))
            .ToList();

        _data.Active.SpellTargetTiles.Set(legalTargets);
        _bus.Publish(new StartedEvent(_data.Active.CurrentSpell!, legalTargets));
    }

    public void Execute(float deltaTime) { }

    public void Leave()
    {
        _data.Active.SpellTargetTiles.Clear();
        _subscriptions.DisposeAll();
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_magicTargetCalculator.IsValidMagicTargetTile(evnt.Tile, _data)) 
            return;
        
        _bus.Publish(new MagicTargetSelectedEvent(evnt.Tile));
    }
}