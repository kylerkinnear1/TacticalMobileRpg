using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Systems.Handlers;

public class SelectTileHandler
{
    private readonly BattleData _state;
    private readonly IPathCalculator _path;
    private readonly PlaceUnitHandler _placeUnit;
    private readonly AdvanceToNextUnitHandler _advanceUnit;
    private readonly ValidTargetCalculator _targetCalculator;

    public SelectTileHandler(
        BattleData state,
        IPathCalculator path,
        PlaceUnitHandler placeUnit,
        AdvanceToNextUnitHandler advanceUnit,
        ValidTargetCalculator targetCalculator)
    {
        _state = state;
        _path = path;
        _placeUnit = placeUnit;
        _advanceUnit = advanceUnit;
        _targetCalculator = targetCalculator;
    }

    public void Handle(Point tile)
    {
        if (_state.Step == BattleStep.Setup)
        {
            _placeUnit.Handle(tile);
            return;
        }

        if (_state.Step == BattleStep.Moving && _state.WalkableTiles.Contains(tile))
        {
            _state.UnitCoordinates[_state.CurrentUnit] = tile;
            Bus.Global.Publish(new UnitMovedEvent(_state.CurrentUnit, tile));
            return;
        }

        if (_targetCalculator.IsValidAttackTargetTile(tile))
        {
            var enemy = _state.UnitsAt(tile).FirstOrDefault(x => x.PlayerId != _state.CurrentUnit.PlayerId);
            if (enemy is null)
                return;

            var damage = CalcAttackDamage(_state.CurrentUnit.Stats.Attack, enemy.Stats.Defense);
            DamageUnits(new[] { enemy }, damage);
            return;
        }

        if (_targetCalculator.IsValidMagicTargetTile(tile))
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

    private void CastSpell(SpellData spell, Point target)
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

    private void CastSpell(SpellData spell, IEnumerable<BattleUnitData> targets)
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

    private int CalcSpellDamage(SpellData spell) =>
        spell.Type switch
        {
            SpellType.Fire1 => Rng.Instance.Int(6, 8),
            SpellType.Fire2 => Rng.Instance.Int(7, 9),
            SpellType.Cure1 => -6,
            _ => throw new ArgumentException()
        };

    private void DamageUnits(IEnumerable<BattleUnitData> units, int damage)
    {
        var defeatedUnits = new List<BattleUnitData>();
        var damagedUnits = new List<(BattleUnitData, int)>();
        foreach (var unit in units)
        {
            unit.RemainingHealth = damage >= 0
                ? Math.Max(unit.RemainingHealth - damage, 0)
                : Math.Min(unit.Stats.MaxHealth, unit.RemainingHealth - damage);

            damagedUnits.Add((unit, damage));

            if (unit.RemainingHealth <= 0)
            {
                defeatedUnits.Add(unit);
                _state.TurnOrder.Remove(unit);
                _state.UnitCoordinates.Remove(unit);
            }
        }

        if (_state.ActiveUnitIndex >= _state.TurnOrder.Count)
            _state.ActiveUnitIndex = 0;
        
        Bus.Global.Publish(new UnitDamagedEvent(damagedUnits));
        Bus.Global.Publish(new UnitsDefeatedEvent(defeatedUnits));

        _advanceUnit.Handle();
    }
}
