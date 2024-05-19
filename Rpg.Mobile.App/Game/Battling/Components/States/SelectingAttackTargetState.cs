using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Components.States;

public class SelectingAttackTargetState : IState
{
    private readonly BattleComponent _component;
    private readonly BattleData _data;

    public SelectingAttackTargetState(BattleComponent component, BattleData data)
    {
        _component = component;
        _data = data;
    }

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        _component.AttackTargetHighlight.Range = 1;
    }

    public void Execute(float deltaTime) { }

    public void Leave()
    {
        Bus.Global.Unsubscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Unsubscribe<TileClickedEvent>(TileClicked);

        _component.AttackTargetHighlight.Visible = false;
    }

    private void TileHovered(TileHoveredEvent evnt)
    {
        if (!IsValidAttackTargetTile(evnt.Tile)) return;

        _component.AttackTargetHighlight.Center = evnt.Tile;
        _component.AttackTargetHighlight.Range = 1;
        _component.AttackTargetHighlight.Visible = true;
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidAttackTargetTile(evnt.Tile)) return;

        var enemy = _data.UnitsAt(evnt.Tile).Single(x => x.PlayerId != _data.CurrentUnit.PlayerId);
        var damage = CalcAttackDamage(_data.CurrentUnit.Stats.Attack, enemy.Stats.Defense);
        DamageUnits(new[] { enemy }, damage);
    }

    private bool IsValidAttackTargetTile(Point tile)
    {
        if (!_data.AttackTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = _data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               hoveredUnit.PlayerId != _data.CurrentUnit.PlayerId;
    }

    private int CalcAttackDamage(int attack, int defense)
    {
        var deterministicDamage = Math.Max(1, attack - defense);
        var damageRangeModifier = Rng.Instance.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage + (int)Math.Round(damageRangeModifier);
        return Math.Max(1, damage);
    }

    private void DamageUnits(IEnumerable<BattleUnitData> units, int damage)
    {
        var defeatedUnits = new List<BattleUnitData>();
        var damagedUnits = new List<(BattleUnitData Unit, int Damage)>();
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

        var positions = damagedUnits
            .Select(x => (_component.Units[x.Unit].Position, Data: x.Damage))
            .ToList();

        _component.DamageIndicator.SetDamage(positions);

        var defeatedComponents = defeatedUnits.Select(x => _component.Units[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Visible = false;
        }

        Bus.Global.Publish(new UnitDamagedEvent(damagedUnits));
        Bus.Global.Publish(new UnitsDefeatedEvent(defeatedUnits));
    }
}
