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
    public HashSet<int> PlayerRerolls { get; set; } = new();

    public IEnumerable<BattleUnitState> UnitsAt(Point tile) =>
        UnitCoordinates.Where(x => x.Value == tile).Select(x => x.Key);

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

        var player1Units = StatPresets.All.Append(StatPresets.Warrior).Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Append(StatPresets.Warrior).Shuffle(Rng.Instance).ToList();
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
        var previousUnit = _state.ActiveUnitIndex >= 0 ? CurrentUnit : null;
        var isLastUnit = _state.ActiveUnitIndex + 1 >= _state.TurnOrder.Count;
        _state.ActiveUnitIndex = !isLastUnit ? _state.ActiveUnitIndex + 1 : 0;
        if (isLastUnit)
            _state.TurnOrder.Set(_state.TurnOrder.Shuffle(Rng.Instance).ToList());
        
        _state.ActiveUnitStartPosition = _state.UnitCoordinates[CurrentUnit];
        _state.PlayerRerolls.Clear();

        Bus.Global.Publish(new ActiveUnitChangedEvent(previousUnit, CurrentUnit));

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

    public void SelectTile(Point tile)
    {
        if (_state.Step == BattleStep.SelectingAttackTarget)
        {
            var enemy = _state.UnitsAt(tile).FirstOrDefault(x => x.PlayerId != _state.CurrentUnit.PlayerId);
            if (enemy is null)
                return;

            var damage = CalcAttackDamage(_state.CurrentUnit.Stats.Attack, enemy.Stats.Defense);
            DamageUnits(new [] { enemy }, damage);
        }

        if (_state.Step == BattleStep.SelectingMagicTarget && _state.CurrentSpell is not null)
        {
            CastSpell(_state.CurrentSpell, tile);
        }
    }

    public int CalcAttackDamage(int attack, int defense)
    {
        var deterministicDamage = Math.Max(1, attack - defense);
        var damageRangeModifier = Rng.Instance.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage + (int)Math.Round(damageRangeModifier);
        return Math.Max(1, damage);
    }

    public int CalcSpellDamage(SpellState spell) =>
        spell.Type switch
        {
            SpellType.Fire1 => Rng.Instance.Int(6, 8),
            SpellType.Fire2 => Rng.Instance.Int(7, 9),
            SpellType.Cure1 => -6,
            _ => throw new ArgumentException()
        };

    public void CastSpell(SpellState spell, Point target)
    {
        var hits = _path
            .CreateFanOutArea(target, _state.Map.Corner, spell.MinRange - 1, spell.MaxRange - 1)
            .ToHashSet();

        var units = _state.UnitCoordinates.Where(x => hits.Contains(x.Value));
        var targets = units
            .Where(x =>
                x.Key.PlayerId == CurrentUnit.PlayerId && spell.TargetsFriendlies ||
                x.Key.PlayerId != CurrentUnit.PlayerId && spell.TargetsEnemies)
            .ToList();

        CastSpell(spell, targets.Select(x => x.Key));
    }

    public void CastSpell(SpellState spell, IEnumerable<BattleUnitState> targets)
    {
        if (CurrentUnit.RemainingMp < spell.MpCost)
        {
            Bus.Global.Publish(new NotEnoughMpEvent(spell));
            return;
        }

        var damage = CalcSpellDamage(spell);
        CurrentUnit.RemainingMp -= spell.MpCost;
        DamageUnits(targets, damage);
    }

    public void TargetSpell(SpellState spell)
    {
        _state.CurrentSpell = spell;
        ChangeBattleState(BattleStep.SelectingMagicTarget);
    }

    public void DamageUnits(IEnumerable<BattleUnitState> units, int damage)
    {
        var defeatedUnits = new List<BattleUnitState>();
        var damagedUnits = new List<(BattleUnitState, int)>();
        foreach (var unit in units)
        {
            unit.RemainingHealth = damage >= 0
                ? Math.Max(unit.RemainingHealth - damage, 0)
                : Math.Min(unit.Stats.MaxHealth, unit.RemainingHealth - damage);

            damagedUnits.Add((unit, damage));

            if (unit.RemainingHealth <= 0)
            {
                defeatedUnits.Add(unit);
            }
        }

        Bus.Global.Publish(new UnitDamagedEvent(damagedUnits));
        Bus.Global.Publish(new UnitsDefeatedEvent(defeatedUnits));

        AdvanceToNextUnit();
    }

    public bool IsValidMagicTargetTile(Point tile)
    {
        var hoveredUnit = _state.UnitsAt(tile).FirstOrDefault();
        return _state.Step == BattleStep.SelectingMagicTarget &&
               hoveredUnit != null &&
               ((_state.CurrentSpell.TargetsEnemies && hoveredUnit.PlayerId != CurrentUnit.PlayerId) ||
                (_state.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == CurrentUnit.PlayerId));
    }

    public bool IsValidAttackTargetTile(Point tile)
    {
        var hoveredUnit = _state.UnitsAt(tile).FirstOrDefault();
        return _state.Step == BattleStep.SelectingAttackTarget &&
               hoveredUnit != null &&
               hoveredUnit.PlayerId != CurrentUnit.PlayerId;
    }

    public void RerollUnit()
    {
        if (_state.PlayerRerolls.Contains(_state.CurrentUnit.PlayerId))
            return;

        var alreadyGoneUnits = _state.TurnOrder.Where((_, i) => i < _state.ActiveUnitIndex);
        var previousUnit = CurrentUnit;
        var remainingUnits = _state.TurnOrder
            .Where((_, i) => i >= _state.ActiveUnitIndex)
            .Shuffle(Rng.Instance);

        var selection = remainingUnits.First(x => x.PlayerId == _state.CurrentUnit.PlayerId);
        var newTurnOrder = alreadyGoneUnits
            .Append(selection)
            .Concat(remainingUnits
                .Skip(1));
        _state.TurnOrder.Set(newTurnOrder);
        _state.PlayerRerolls.Add(_state.CurrentUnit.PlayerId);

        Bus.Global.Publish(new ActiveUnitChangedEvent(previousUnit, _state.CurrentUnit));
        Bus.Global.Publish(new BattleStepChangedEvent(BattleStep.Moving));
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
                _state.Map.Corner,
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
                _state.Map.Corner,
                _state.CurrentSpell.MinRange,
                _state.CurrentSpell.MaxRange)
            .Where(x => 
                !gridToUnit.Contains(x) ||
                (_state.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != _state.CurrentUnit.PlayerId) ||
                 (_state.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == _state.CurrentUnit.PlayerId))))
            .ToList();
        
        _state.SpellTargetTiles.Set(allTargets);
        return BattleStep.SelectingMagicTarget;
    }
}

public record BattleStartedEvent : IEvent;
public record ActiveUnitChangedEvent(BattleUnitState? PreviousUnit, BattleUnitState NextUnit) : IEvent;
public record BattleStepChangedEvent(BattleStep Step) : IEvent;
public record UnitsDefeatedEvent(IEnumerable<BattleUnitState> Defeated) : IEvent;
public record NotEnoughMpEvent(SpellState Spell) : IEvent;
public record UnitDamagedEvent(List<(BattleUnitState Unit, int Damage)> Hits) : IEvent;