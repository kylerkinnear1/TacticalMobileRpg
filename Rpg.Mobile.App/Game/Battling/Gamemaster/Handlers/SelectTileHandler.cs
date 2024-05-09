using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Gamemaster.Handlers;

public class SelectTileHandler
{
    private readonly BattleState _state;
    private readonly IPathCalculator _path;
    private readonly PlaceUnitHandler _placeUnit;
    private readonly AdvanceToNextUnitHandler _advanceUnit;

    public SelectTileHandler(
        BattleState state,
        IPathCalculator path,
        PlaceUnitHandler placeUnit,
        AdvanceToNextUnitHandler advanceUnit)
    {
        _state = state;
        _path = path;
        _placeUnit = placeUnit;
        _advanceUnit = advanceUnit;
    }

    public void Handle(Point tile)
    {
        if (_state.Step == BattleStep.Setup)
        {
            _placeUnit.Handle(tile);
        }

        if (_state.Step == BattleStep.Moving && _state.WalkableTiles.Contains(tile))
        {
            _state.UnitCoordinates[_state.CurrentUnit] = tile;
            Bus.Global.Publish(new UnitMovedEvent(_state.CurrentUnit, tile));
        }

        if (_state.Step == BattleStep.SelectingAttackTarget)
        {
            var enemy = _state.UnitsAt(tile).FirstOrDefault(x => x.PlayerId != _state.CurrentUnit.PlayerId);
            if (enemy is null)
                return;

            var damage = CalcAttackDamage(_state.CurrentUnit.Stats.Attack, enemy.Stats.Defense);
            DamageUnits(new[] { enemy }, damage);
        }

        if (_state.Step == BattleStep.SelectingMagicTarget && _state.CurrentSpell is not null)
        {
            CastSpell(_state.CurrentSpell, tile);
        }
    }

    private int CalcAttackDamage(int attack, int defense)
    {
        var deterministicDamage = Math.Max(1, attack - defense);
        var damageRangeModifier = Rng.Instance.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage + (int)Math.Round(damageRangeModifier);
        return Math.Max(1, damage);
    }

    private void CastSpell(SpellState spell, Point target)
    {
        var hits = _path
            .CreateFanOutArea(target, _state.Map.Corner, spell.MinRange - 1, spell.MaxRange - 1)
            .ToHashSet();

        var units = _state.UnitCoordinates.Where(x => hits.Contains(x.Value));
        var targets = units
            .Where(x =>
                x.Key.PlayerId == _state.CurrentUnit.PlayerId && spell.TargetsFriendlies ||
                x.Key.PlayerId != _state.CurrentUnit.PlayerId && spell.TargetsEnemies)
            .ToList();

        CastSpell(spell, targets.Select(x => x.Key));
    }

    private void CastSpell(SpellState spell, IEnumerable<BattleUnitState> targets)
    {
        if (_state.CurrentUnit.RemainingMp < spell.MpCost)
        {
            Bus.Global.Publish(new NotEnoughMpEvent(spell));
            return;
        }

        var damage = CalcSpellDamage(spell);
        _state.CurrentUnit.RemainingMp -= spell.MpCost;
        DamageUnits(targets, damage);
    }

    private int CalcSpellDamage(SpellState spell) =>
        spell.Type switch
        {
            SpellType.Fire1 => Rng.Instance.Int(6, 8),
            SpellType.Fire2 => Rng.Instance.Int(7, 9),
            SpellType.Cure1 => -6,
            _ => throw new ArgumentException()
        };

    private void DamageUnits(IEnumerable<BattleUnitState> units, int damage)
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

        _advanceUnit.Handle();
    }
}
