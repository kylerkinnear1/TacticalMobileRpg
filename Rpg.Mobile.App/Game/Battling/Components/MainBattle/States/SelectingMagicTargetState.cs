using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Components.MainBattle.States;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetState : IBattleState
{
    private readonly BattleData _data;
    private readonly MainBattleComponent _component;
    private readonly IPathCalculator _path;
    private readonly BattleMenuComponent _menu;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var gridToUnit = _data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        var allTargets = _path
            .CreateFanOutArea(
               _data.UnitCoordinates[_data.CurrentUnit],
               _data.Map.Corner,
               _data.CurrentSpell.MinRange,
               _data.CurrentSpell.MaxRange)
            .Where(x =>
                !gridToUnit.Contains(x) ||
                (_data.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != _data.CurrentUnit.PlayerId) ||
                 (_data.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == _data.CurrentUnit.PlayerId))))
            .ToList();

        _data.SpellTargetTiles.Set(allTargets);


        // TODO: Figure out place, this aint it.
        _menu.SetButtons(_data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ =>
            {
                _data.CurrentSpell = x;
                _changeState.Handle(BattleStep.SelectingMagicTarget);
            }))
            .Append(new("Back", _ => _battleService.ChangeBattleState(BattleStep.Moving)))
            .ToArray());
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
        Bus.Global.Publish<UnitDamageAssignedEvent>(new(targets, damage));
    }

    private int CalcSpellDamage(SpellData spell) =>
        spell.Type switch
        {
            SpellType.Fire1 => Rng.Instance.Int(6, 8),
            SpellType.Fire2 => Rng.Instance.Int(7, 9),
            SpellType.Cure1 => -6,
            _ => throw new ArgumentException()
        };

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
