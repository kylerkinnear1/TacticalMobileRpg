using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Battling.Systems.Handlers;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Battling.Systems;

public class BattleStateService
{
    private readonly BattleState _state;
    private readonly IPathCalculator _path;
    private readonly StartBattleHandler _startBattle;
    private readonly AdvanceToNextUnitHandler _advanceUnit;
    private readonly ChangeBattleStateHandler _changeStep;
    private readonly SelectTileHandler _selectTile;
    private readonly TargetSpellHandler _targetSpell;
    private readonly ValidTargetCalculator _validTargetCalculator;
    private readonly PlaceUnitHandler _placeUnit;

    private BattleUnitState CurrentUnit => _state.TurnOrder[_state.ActiveUnitIndex];

    public BattleStateService(BattleState state, IPathCalculator path)
    {
        _state = state;
        _path = path;

        _changeStep = new(_state, _path);
        _advanceUnit = new(_state, _changeStep);
        _startBattle = new(_state, _advanceUnit);
        _placeUnit = new(_state, _startBattle);
        _selectTile = new(_state, _path, _placeUnit, _advanceUnit);
        _targetSpell = new(_state, _changeStep);
        
        _validTargetCalculator = new ValidTargetCalculator(_state);
    }

    public void PlaceUnit(Point tile) => _placeUnit.Handle(tile);

    public void AdvanceToNextUnit() => _advanceUnit.Handle();

    public void ChangeBattleState(BattleStep step) => _changeStep.Handle(step);

    public void SelectTile(Point tile) => _selectTile.Handle(tile);

    public void TargetSpell(SpellState spell) => _targetSpell.Handle(spell);

    public bool IsValidMagicTargetTile(Point tile) => _validTargetCalculator.IsValidMagicTargetTile(tile);
    public bool IsValidAttackTargetTile(Point tile) => _validTargetCalculator.IsValidAttackTargetTile(tile);
}

public record LoadStateClickedEvent : IEvent;
public record SaveStateClickedEvent : IEvent;
