using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.States.BattlePhaseMachine;
using Extensions = Rpg.Mobile.App.Utils.Extensions;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Active.Steps;

public class SelectingAttackTargetPhase : IBattlePhase
{
    private readonly Context _context;

    public SelectingAttackTargetPhase(Context context) => _context = context;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var gridToUnit = Enumerable.ToLookup<KeyValuePair<BattleUnitData, Point>, Point, BattleUnitData>(_context.Data.UnitCoordinates, x => x.Value, x => x.Key);

        var legalTargets = Enumerable.Where<Point>(_context.Path
                .CreateFanOutArea(
                    _context.Data.UnitCoordinates[_context.Data.CurrentUnit],
                    _context.Data.Map.Corner,
                    _context.Data.CurrentUnit.Stats.AttackMinRange,
                    _context.Data.CurrentUnit.Stats.AttackMaxRange), x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != _context.Data.CurrentUnit.PlayerId))
            .ToList();

        Extensions.Set(_context.Data.AttackTargetTiles, legalTargets);

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

        var enemy = Enumerable.Single<BattleUnitData>(_context.Data.UnitsAt(evnt.Tile), x => x.PlayerId != _context.Data.CurrentUnit.PlayerId);
        var damage = CalcAttackDamage(_context.Data.CurrentUnit.Stats.Attack, enemy.Stats.Defense);
        Bus.Global.Publish<UnitDamageAssignedEvent>(new(new[] { enemy }, damage));
    }

    private bool IsValidAttackTargetTile(Point tile)
    {
        if (!_context.Data.AttackTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = Enumerable.FirstOrDefault<BattleUnitData>(_context.Data.UnitsAt(tile));
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
