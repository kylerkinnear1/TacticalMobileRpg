using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.MainBattleStateMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States.ActivePhase;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetState : IBattleState
{
    private readonly Context _context;
    private BattleData Data => _context.Data;

    public SelectingMagicTargetState(Context context) => _context = context;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);

        var gridToUnit = _context.Data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
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

        _context.Menu.SetButtons(Data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ => Bus.Global.Publish(new SpellSelectedEvent(x))))
            .Append(new("Back", _ => Bus.Global.Publish(new BackClickedEvent())))
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
        var hits = _context.Path
            .CreateFanOutArea(target, Data.Map.Corner, spell.MinRange - 1, spell.MaxRange - 1)
            .ToHashSet();

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
        if (Data.CurrentUnit.RemainingMp < spell.MpCost)
        {
            Bus.Global.Publish(new NotEnoughMpEvent(spell));
            return;
        }

        var damage = CalcSpellDamage(spell);
        Data.CurrentUnit.RemainingMp -= spell.MpCost;
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

        _context.Main.AttackTargetHighlight.Center = evnt.Tile;
        _context.Main.AttackTargetHighlight.Range = 1;
        _context.Main.AttackTargetHighlight.Visible = true;
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile)) return;

        CastSpell(Data.CurrentSpell, evnt.Tile);
    }

    public bool IsValidMagicTargetTile(Point tile)
    {
        if (Data.CurrentSpell is null || !Data.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = Data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               (Data.CurrentSpell.TargetsEnemies && hoveredUnit.PlayerId != Data.CurrentUnit.PlayerId ||
                Data.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == Data.CurrentUnit.PlayerId);
    }
}
