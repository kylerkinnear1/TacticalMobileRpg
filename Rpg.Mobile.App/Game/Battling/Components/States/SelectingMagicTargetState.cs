using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Components.States;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetState : IState
{
    private readonly BattleData _data;
    private readonly BattleComponent _component;
    private readonly IPathCalculator _path;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
        Bus.Global.Unsubscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Unsubscribe<TileClickedEvent>(TileClicked);
    }

    private void CastSpell(SpellData spell, Point target)
    {
        var hits = _path
            .CreateFanOutArea(target, _data.Map.Corner, spell.MinRange - 1, spell.MaxRange - 1)
            .ToHashSet();

        var units = _data.UnitCoordinates.Where(x => hits.Contains(x.Value));
        var targets = units
            .Where(x =>
                x.Key.PlayerId == _data.CurrentUnit.PlayerId && spell.TargetsFriendlies ||
                x.Key.PlayerId != _data.CurrentUnit.PlayerId && spell.TargetsEnemies)
            .ToList();

        CastSpell(spell, targets.Select(x => x.Key));
    }

    private void CastSpell(SpellData spell, IEnumerable<BattleUnitData> targets)
    {
        if (_data.CurrentUnit.RemainingMp < spell.MpCost)
        {
            Bus.Global.Publish(new NotEnoughMpEvent(spell));
            return;
        }

        var damage = CalcSpellDamage(spell);
        _data.CurrentUnit.RemainingMp -= spell.MpCost;
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
                _data.TurnOrder.Remove(unit);
                _data.UnitCoordinates.Remove(unit);
            }
        }

        if (_data.ActiveUnitIndex >= _data.TurnOrder.Count)
            _data.ActiveUnitIndex = 0;

        var defeatedComponents = defeatedUnits.Select(x => _component.Units[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Visible = false;
        }

        Bus.Global.Publish(new UnitDamagedEvent(damagedUnits));
        Bus.Global.Publish(new UnitsDefeatedEvent(defeatedUnits));
    }

    private void TileHovered(TileHoveredEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile)) return;

        _component.AttackTargetHighlight.Center = evnt.Tile;
        _component.AttackTargetHighlight.Range = 1;
        _component.AttackTargetHighlight.Visible = true;
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile)) return;

        CastSpell(_data.CurrentSpell, evnt.Tile);
    }

    public bool IsValidMagicTargetTile(Point tile)
    {
        if (_data.CurrentSpell is null || !_data.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = _data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               ((_data.CurrentSpell.TargetsEnemies && hoveredUnit.PlayerId != _data.CurrentUnit.PlayerId) ||
                (_data.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == _data.CurrentUnit.PlayerId));
    }
}
