using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Gamemaster;

public enum BattleStep
{
    Setup,
    Moving,
    SelectingSpell,
    SelectingAttackTarget,
    SelectingMagicTarget
}

public class BattleState
{
    public MapState Map { get; set; }

    public List<BattleUnitState> TurnOrder { get; set; } = new();
    public Dictionary<BattleUnitState, Point> UnitCoordinates { get; set; } = new();
    public int ActiveUnitIndex { get; set; } = -1;
    public Point ActiveUnitStartPosition { get; set; } = Point.Empty;

    public BattleStep Step { get; set; } = BattleStep.Setup;
    public SpellState? CurrentSpell { get; set; }

    public BattleUnitState? CurrentUnit => ActiveUnitIndex >= 0 ? TurnOrder[ActiveUnitIndex] : null;
    public List<Point> WalkableTiles { get; set; } = new();
    public List<Point> AttackTargetTiles { get; set; } = new();
    public List<Point> SpellTargetTiles { get; set; } = new();

    public BattleState(MapState map)
    {
        Map = map;
    }
}

public class BattleStateService
{
    private readonly BattleState _state;
    private readonly IPathCalculator _path;

    private BattleUnitState CurrentUnit => _state.TurnOrder[_state.ActiveUnitIndex];

    public BattleStateService(BattleState state, IPathCalculator path)
    {
        _state = state;
        _path = path;
    }

    public void StartBattle()
    {
        if (_state.ActiveUnitIndex >= 0)
            throw new NotSupportedException("Battle already started.");

        var player1Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        foreach (var (unit, point) in player1Units.Zip(_state.Map.Player1Origins))
            _state.UnitCoordinates[unit] = point;

        foreach (var (unit, point) in player2Units.Zip(_state.Map.Player2Origins))
            _state.UnitCoordinates[unit] = point;

        _state.TurnOrder = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        Bus.Global.Publish(new BattleStartedEvent());
        AdvanceToNextUnit();
    }

    public void AdvanceToNextUnit()
    {
        var isLastUnit = _state.ActiveUnitIndex + 1 >= _state.TurnOrder.Count;
        _state.ActiveUnitIndex = !isLastUnit ? _state.ActiveUnitIndex + 1 : 0;
        if (isLastUnit)
            _state.TurnOrder.Set( _state.TurnOrder.Shuffle(Rng.Instance).ToList());
        
        _state.ActiveUnitStartPosition = _state.UnitCoordinates[CurrentUnit];

        Bus.Global.Publish(new ActiveUnitChangedEvent(CurrentUnit));

        ChangeBattleState(BattleStep.Moving);
    }

    public void ChangeBattleState(BattleStep step)
    {
        
        _state.AttackTargetTiles.Clear();
        _state.WalkableTiles.Clear();
        _state.SpellTargetTiles.Clear();
        _state.CurrentSpell = step == BattleStep.SelectingMagicTarget ? _state.CurrentSpell : null;
        _state.Step = step switch
        {
            BattleStep.Moving => SetupMovingStep(),
            BattleStep.SelectingAttackTarget => SetupAttackTarget(),
            BattleStep.SelectingSpell => SetupSelectSpell(), 
            BattleStep.SelectingMagicTarget => SetupMagicTarget(),
            _ => throw new ArgumentException()
        };

        Bus.Global.Publish(new BattleStepChangedEvent(step));
    }

    public void SetupSpell(SpellState spell)
    {
        _state.CurrentSpell = spell;
        ChangeBattleState(BattleStep.SelectingMagicTarget);
    }

    public void DamageUnit(BattleUnitState enemy, int damage)
    {
        enemy.RemainingHealth = damage >= 0
            ? Math.Max(enemy.RemainingHealth - damage, 0)
            : Math.Min(enemy.Stats.MaxHealth, enemy.RemainingHealth - damage);

        if (enemy.RemainingHealth <= 0)
        {
            _state.UnitCoordinates.Remove(enemy);
            Bus.Global.Publish(new UnitsDefeatedEvent(new[] { enemy }));
        }

        AdvanceToNextUnit();
    }

    private BattleStep SetupMovingStep()
    {
        var walkableTiles = _path
            .CreateFanOutArea(_state.ActiveUnitStartPosition, _state.Map.Corner, CurrentUnit.Stats.Movement)
            .Where(x => x == _state.ActiveUnitStartPosition ||
                        !_state.UnitCoordinates.ContainsValue(x) && _state.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();

        _state.WalkableTiles = walkableTiles;
        return BattleStep.Moving;
    }

    private BattleStep SetupAttackTarget()
    {
        var gridToUnit = _state.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        
        var legalTargets = _path
            .CreateFanOutArea(
                _state.UnitCoordinates[CurrentUnit],
                new(_state.Map.Width, _state.Map.Height),
                CurrentUnit.Stats.AttackMinRange,
                CurrentUnit.Stats.AttackMaxRange)
            .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != CurrentUnit.PlayerId))
            .ToList();

        _state.AttackTargetTiles.Set(legalTargets);
        return BattleStep.SelectingAttackTarget;
    }

    private BattleStep SetupSelectSpell()
    {
        return BattleStep.SelectingSpell;
    }

    private BattleStep SetupMagicTarget()
    {
        var gridToUnit = _state.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        var allTargets = _path
            .CreateFanOutArea(
                _state.UnitCoordinates[CurrentUnit],
                new(_state.Map.Width, _state.Map.Height),
                CurrentUnit.Stats.AttackMinRange,
                CurrentUnit.Stats.AttackMaxRange)
            .Where(x => 
                (_state.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != _state.CurrentUnit.PlayerId) ||
                 (_state.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == _state.CurrentUnit.PlayerId))))
            .ToList();
        
        _state.SpellTargetTiles.Set(allTargets);
        return BattleStep.SelectingMagicTarget;
    }

    //private void CastSpell(SpellState spell, Point position)
    //{
    //    //var currentUnit = CurrentUnit;
    //    //if (currentUnit.State.RemainingMp < spell.MpCost)
    //    //{
    //    //    //_stats.Label = "Not enough MP"; // TODO: Less hacky way.
    //    //    return;
    //    //}

    //    //var targets = _unitComponents.Values
    //    //    .Where(x =>
    //    //        GetTileForPosition(x.Position) == position &&
    //    //        (spell.TargetsEnemies && x.State.PlayerId != currentUnit.State.PlayerId ||
    //    //         spell.TargetsFriendlies && x.State.PlayerId == currentUnit.State.PlayerId))
    //    //    .ToList();

    //    //if (targets.Count == 0)
    //    //    return;

    //    //currentUnit.State.RemainingMp -= spell.MpCost;
    //    //var damage = _spellDamage.CalcDamage(spell);
    //    //foreach (var target in targets)
    //    //{
    //    //    _battleService.DamageUnit(target.State, damage);
    //    //}
    //}
}

public record BattleStartedEvent : IEvent;
public record ActiveUnitChangedEvent(BattleUnitState State) : IEvent;
public record BattleStepChangedEvent(BattleStep Step) : IEvent;
public record UnitsDefeatedEvent(IEnumerable<BattleUnitState> Defeated) : IEvent;
public record NotEnoughMpEvent(SpellState Spell) : IEvent;