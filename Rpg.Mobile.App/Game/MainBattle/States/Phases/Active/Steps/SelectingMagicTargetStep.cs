﻿using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.States.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Active.Steps;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetPhase(Context _context) : IBattlePhase
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

        _context.Menu.SetButtons(Data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ => Bus.Global.Publish(new SpellSelectedEvent(x))))
            .Append(new("Back", _ => Bus.Global.Publish(new BackClickedEvent())))
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
        if (Data.CurrentUnit.RemainingMp < spell.MpCost)
        {
            Bus.Global.Publish(new NotEnoughMpEvent(spell));
            return;
        }

        var damage = CalcSpellDamage(spell);
        Data.CurrentUnit.RemainingMp -= spell.MpCost;
        Bus.Global.Publish(new UnitDamageAssignedEvent(targets, damage));
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

        if (Data.CurrentSpell is null)
            throw new NotImplementedException();
        
        CastSpell(Data.CurrentSpell, evnt.Tile);
    }

    private bool IsValidMagicTargetTile(Point tile)
    {
        if (Data.CurrentSpell is null || !Data.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = Data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               (Data.CurrentSpell.TargetsEnemies && hoveredUnit.PlayerId != Data.CurrentUnit.PlayerId ||
                Data.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == Data.CurrentUnit.PlayerId);
    }
}
