using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;

public class ActivePhase(BattlePhaseMachine.Context _context) : IBattlePhase
{
    private int TileWidth => MapComponent.TileWidth;
    
    public interface IStep : IState { }

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IStep> _step = new();
    
    public record MagicClickedEvent : IEvent;
    public record AttackClickedEvent : IEvent;
    public record BackClickedEvent : IEvent;
    public record CompletedEvent(BattleUnitData Unit) : IEvent;
    public record UnitsDamagedEvent(IEnumerable<BattleUnitData> Units, int Damage) : IEvent;
    public record NotEnoughMpEvent(SpellData Spell) : IEvent;
    public record SpellSelectedEvent(SpellData Spell) : IEvent;

    public void Enter()
    {
        _subscriptions =
        [
            Bus.Global.Subscribe<AttackClickedEvent>(_ => _step.Change(new SelectingAttackTargetStep(_context))),
            Bus.Global.Subscribe<MagicClickedEvent>(_ => _step.Change(new SelectingSpellStep(_context))),
            Bus.Global.Subscribe<BackClickedEvent>(BackClicked),
            Bus.Global.Subscribe<SpellSelectedEvent>(_ => _step.Change(new SelectingMagicTargetStep(_context))),
            Bus.Global.Subscribe<IdleStep.CompletedEvent>(evnt => Bus.Global.Publish(new CompletedEvent(evnt.CurrentUnit))),
            Bus.Global.Subscribe<UnitsDamagedEvent>(UnitDamaged)
        ];
        
        _context.Data.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
        _step.Change(new IdleStep(_context));
        _context.Main.DamageIndicator.Visible = true;
    }

    public void Execute(float deltaTime)
    {
        var currentUnitPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
        _context.Main.CurrentUnitShadow.Shadows.SetSingle(
            new(currentUnitPosition.X * TileWidth, currentUnitPosition.Y * TileWidth, TileWidth, TileWidth));
        _step.Execute(deltaTime);
    }

    public void Leave()
    {
        _step.Change(null);
        _subscriptions.DisposeAll();
        _context.Main.DamageIndicator.Visible = false;
    }

    private void BackClicked(BackClickedEvent evnt)
    {
        var position = _context.Main.GetPositionForTile(
            _context.Data.ActiveUnitStartPosition, 
            _context.Main.CurrentUnit.Unit.Bounds.Size);

        _context.Data.UnitCoordinates[_context.Data.CurrentUnit] = _context.Data.ActiveUnitStartPosition;
            
        _context.Main.CurrentUnit.MoveTo(
            position, 
            () => _step.Change(new IdleStep(_context)));
    }
    
    private void UnitDamaged(UnitsDamagedEvent evnt)
    {
        var defeatedUnits = new List<BattleUnitData>();
        var damagedUnits = new List<(BattleUnitData Unit, int Damage)>();
        foreach (var unit in evnt.Units)
        {
            unit.RemainingHealth = evnt.Damage >= 0
                ? Math.Max(unit.RemainingHealth - evnt.Damage, 0)
                : Math.Min(unit.RemainingHealth - evnt.Damage, unit.Stats.MaxHealth);

            damagedUnits.Add((unit, evnt.Damage));

            if (unit.RemainingHealth <= 0)
            {
                defeatedUnits.Add(unit);
                
                // TODO: Don't remove. Just mark as dead.
                _context.Data.TurnOrder.Remove(unit);
                _context.Data.UnitCoordinates.Remove(unit);
            }
        }

        if (_context.Data.ActiveUnitIndex >= _context.Data.TurnOrder.Count)
            _context.Data.ActiveUnitIndex = 0;

        var positions = damagedUnits
            .Select(x => (_context.Main.Units[x.Unit].Unit.Position, x.Damage))
            .ToList();

        _context.Main.DamageIndicator.SetDamage(positions);
        _context.Main.DamageIndicator.Visible = true;

        var defeatedComponents = defeatedUnits.Select(x => _context.Main.Units[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Unit.Visible = false;
        }

        Bus.Global.Publish(new CompletedEvent(_context.Data.CurrentUnit));
    }
}
