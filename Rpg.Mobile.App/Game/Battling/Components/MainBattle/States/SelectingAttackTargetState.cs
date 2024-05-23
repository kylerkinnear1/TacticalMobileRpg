using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Components.MainBattle.States;

public class SelectingAttackTargetState : IBattleState
{
    private readonly MainBattleComponent _component;
    private readonly BattleData _data;
    private readonly BattleMenuComponent _menu;
    private readonly IPathCalculator _path;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var gridToUnit = _data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);

        var legalTargets = _path
            .CreateFanOutArea(
                _data.UnitCoordinates[_data.CurrentUnit],
                _data.Map.Corner,
                _data.CurrentUnit.Stats.AttackMinRange,
                _data.CurrentUnit.Stats.AttackMaxRange)
            .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != _data.CurrentUnit.PlayerId))
            .ToList();

        _data.AttackTargetTiles.Set(legalTargets);

        _component.AttackTargetHighlight.Range = 1;
        _menu.SetButtons(new ButtonData("Back", _ => _battleService.ChangeBattleState(BattleStep.Moving)));
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
        Bus.Global.Publish<UnitDamageAssignedEvent>(new(new [] { enemy }, damage));
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
}
