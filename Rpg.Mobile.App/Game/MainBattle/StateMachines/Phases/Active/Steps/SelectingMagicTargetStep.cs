using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

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

        var gridToUnit = _context.Data.UnitCoordinates.ToLookup<KeyValuePair<BattleUnitData, Point>, Point, BattleUnitData>(x => x.Value, x => x.Key);
        var allTargets = _context.Path
            .CreateFanOutArea(
                Data.UnitCoordinates[_context.Data.CurrentUnit],
                Data.Map.Corner,
                Data.CurrentSpell!.MinRange,
                Data.CurrentSpell.MaxRange).Where(x =>
                !gridToUnit.Contains(x) ||
                Data.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != Data.CurrentUnit.PlayerId) ||
                 Data.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == Data.CurrentUnit.PlayerId))
            .ToList();

        Data.SpellTargetTiles.Set(allTargets);
        _context.Data.AttackTargetTiles.Set(allTargets);
        _context.Main.AttackTargetHighlight.Range = Data.CurrentSpell.MaxRange;

        _context.Menu.SetButtons(Data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ => Bus.Global.Publish(new ActivePhase.SpellSelectedEvent(x))))
            .Append(new("Back", _ => Bus.Global.Publish(new ActivePhase.BackClickedEvent())))
            .ToArray());
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave() => _subscriptions.DisposeAll();

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
        if (!IsValidMagicTargetTile(evnt.Tile)) return;

        _context.Main.AttackTargetHighlight.Center = evnt.Tile;
        _context.Main.AttackTargetHighlight.Range = 1;
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
