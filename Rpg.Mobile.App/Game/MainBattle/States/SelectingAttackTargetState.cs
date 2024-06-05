using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.MainBattleStateMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States;

public class SelectingAttackTargetState : IBattleState
{
    private readonly Context _context;

    public SelectingAttackTargetState(Context context) => _context = context;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var gridToUnit = _context.Data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);

        var legalTargets = _context.Path
            .CreateFanOutArea(
                _context.Data.UnitCoordinates[_context.Data.CurrentUnit],
                _context.Data.Map.Corner,
                _context.Data.CurrentUnit.Stats.AttackMinRange,
                _context.Data.CurrentUnit.Stats.AttackMaxRange)
            .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != _context.Data.CurrentUnit.PlayerId))
            .ToList();

        _context.Data.AttackTargetTiles.Set(legalTargets);

        _context.Main.AttackTargetHighlight.Range = 1;
        _context.Menu.SetButtons(new ButtonData("Back", _ => Bus.Global.Publish(new BackClickedEvent())));
    }

    public void Execute(float deltaTime) { }

    public void Leave()
    {
        Bus.Global.Unsubscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Unsubscribe<TileClickedEvent>(TileClicked);

        _context.Main.AttackTargetHighlight.Visible = false;
    }

    private void TileHovered(TileHoveredEvent evnt)
    {
        if (!IsValidAttackTargetTile(evnt.Tile)) return;

        _context.Main.AttackTargetHighlight.Center = evnt.Tile;
        _context.Main.AttackTargetHighlight.Range = 1;
        _context.Main.AttackTargetHighlight.Visible = true;
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidAttackTargetTile(evnt.Tile)) return;

        var enemy = _context.Data.UnitsAt(evnt.Tile).Single(x => x.PlayerId != _context.Data.CurrentUnit.PlayerId);
        var damage = CalcAttackDamage(_context.Data.CurrentUnit.Stats.Attack, enemy.Stats.Defense);
        Bus.Global.Publish<UnitDamageAssignedEvent>(new(new [] { enemy }, damage));
    }

    private bool IsValidAttackTargetTile(Point tile)
    {
        if (!_context.Data.AttackTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = _context.Data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               hoveredUnit.PlayerId != _context.Data.CurrentUnit.PlayerId;
    }

    private int CalcAttackDamage(int attack, int defense)
    {
        var deterministicDamage = Math.Max(1, attack - defense);
        var damageRangeModifier = Rng.Instance.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage + (int)Math.Round(damageRangeModifier);
        return Math.Max(1, damage);
    }
}
