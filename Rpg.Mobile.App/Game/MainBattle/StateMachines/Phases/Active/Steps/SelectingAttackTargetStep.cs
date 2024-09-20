﻿using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.Components.MainBattleComponent;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class SelectingAttackTargetStep(Context _context) : ActivePhase.IStep
{
    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions =
        [
            Bus.Global.Subscribe<TileHoveredEvent>(TileHovered),
            Bus.Global.Subscribe<TileClickedEvent>(TileClicked)
        ];

        var gridToUnit = _context.Data.UnitCoordinates.ToLookup<KeyValuePair<BattleUnitData, Point>, Point, BattleUnitData>(x => x.Value, x => x.Key);

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

        var attackTiles = legalTargets
            .Select(x => 
                new RectF(_context.Main.GetPositionForTile(x, TileSize), TileSize));
        
        _context.Main.AttackShadow.Shadows.Set(attackTiles);
        _context.Menu.SetButtons(new ButtonData("Back", _ => Bus.Global.Publish(new ActivePhase.BackClickedEvent())));
    }

    public void Execute(float deltaTime) { }

    public void Leave()
    {
        _subscriptions.DisposeAll();
        _context.Main.AttackTargetHighlight.Visible = false;
        _context.Data.AttackTargetTiles.Clear();
        _context.Main.AttackShadow.Shadows.Clear();
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
        Bus.Global.Publish(new ActivePhase.UnitsDamagedEvent(new[] { enemy }, damage));
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
