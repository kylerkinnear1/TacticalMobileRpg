using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;
using static Rpg.Mobile.App.Game.MainBattle.Components.MainBattleComponent;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetStep(Context _context) : ActivePhase.IStep
{
    private BattleData Data => _context.Data;

    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = 
        [
            Bus.Global.Subscribe<TileHoveredEvent>(TileHovered),
            Bus.Global.Subscribe<TileClickedEvent>(TileClicked)
        ];

        var gridToUnit = _context.Data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        var legalTargets = _context.Path
            .CreateFanOutArea(
                Data.UnitCoordinates[_context.Data.CurrentUnit],
                Data.Map.Corner,
                Data.CurrentSpell!.MinRange,
                Data.CurrentSpell.MaxRange)
            .Where(x =>
                !gridToUnit.Contains(x) ||
                Data.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != Data.CurrentUnit.PlayerId) ||
                 Data.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == Data.CurrentUnit.PlayerId))
            .ToList();

        _context.Data.SpellTargetTiles.Set(legalTargets);
        _context.Main.AttackTargetHighlight.Range = Data.CurrentSpell.Aoe;
        
        var attackTiles = legalTargets
            .Select(x => 
                new RectF(_context.Main.GetPositionForTile(x, TileSize), TileSize));
        
        _context.Main.AttackShadow.Shadows.Set(attackTiles);

        _context.Menu.SetButton(new("Back", _ => Bus.Global.Publish(new ActivePhase.BackClickedEvent())));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
        _subscriptions.DisposeAll();
        _context.Data.SpellTargetTiles.Clear();
        _context.Main.AttackTargetHighlight.Visible = false;
        _context.Main.AttackShadow.Shadows.Clear();
    }

    private void CastSpell(SpellData spell, Point target)
    {
        var hits = _context.Path
            .CreateFanOutArea(target, Data.Map.Corner, spell.MinRange - 1, spell.MaxRange - 1).ToHashSet();

        var units = Data.UnitCoordinates.Where(x => hits.Contains(x.Value));
        var targets = units
            .Where(x =>
                x.Key.PlayerId == Data.CurrentUnit.PlayerId && spell.TargetsFriendlies ||
                x.Key.PlayerId != Data.CurrentUnit.PlayerId && spell.TargetsEnemies)
            .ToList();

        CastSpell(spell, targets.Select(x => x.Key));
    }

    private void CastSpell(SpellData spell, IEnumerable<BattleUnitData> targets)
    {
        var damage = CalcSpellDamage(spell);
        Data.CurrentUnit.RemainingMp -= spell.MpCost;
        Bus.Global.Publish(new ActivePhase.UnitsDamagedEvent(targets, damage));
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
        if (!IsValidMagicTargetTile(evnt.Tile))
        {
            _context.Main.AttackTargetHighlight.Visible = false;
            return;
        }

        _context.Main.AttackTargetHighlight.Center = evnt.Tile;
        _context.Main.AttackTargetHighlight.Visible = true;
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile)) return;
        CastSpell(Data.CurrentSpell, evnt.Tile);
    }

    private bool IsValidMagicTargetTile(Point tile)
    {
        if (!Data.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = Data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               (Data.CurrentSpell.TargetsEnemies && hoveredUnit.PlayerId != Data.CurrentUnit.PlayerId ||
                Data.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == Data.CurrentUnit.PlayerId);
    }
}
