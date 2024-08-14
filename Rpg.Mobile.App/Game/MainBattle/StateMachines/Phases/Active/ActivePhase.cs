using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;

public class ActivePhase(BattlePhaseMachine.Context _context) : IBattlePhase
{
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
        
        _step.Change(new IdleStep(_context));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
        _step.Change(null);
        _subscriptions.DisposeAll();
    }

    private void BackClicked(BackClickedEvent evnt)
    {
        // TODO: Reset position - State machine on Unit Component to move it
        // to isolate the moving code? Sounds good to me.
        _step.Change(new IdleStep(_context));
        throw new NotImplementedException();
    }
    
    private void UnitDamaged(UnitsDamagedEvent evnt)
    {
        var defeatedUnits = new List<BattleUnitData>();
        var damagedUnits = new List<(BattleUnitData Unit, int Damage)>();
        foreach (var unit in evnt.Units)
        {
            unit.RemainingHealth = evnt.Damage >= 0
                ? Math.Max(unit.RemainingHealth - evnt.Damage, 0)
                : Math.Min(unit.Stats.MaxHealth, unit.RemainingHealth - evnt.Damage);

            damagedUnits.Add((unit, evnt.Damage));

            if (unit.RemainingHealth <= 0)
            {
                defeatedUnits.Add(unit);
                _context.Data.TurnOrder.Remove(unit);
                _context.Data.UnitCoordinates.Remove(unit);
            }
        }

        if (_context.Data.ActiveUnitIndex >= _context.Data.TurnOrder.Count)
            _context.Data.ActiveUnitIndex = 0;

        var positions = damagedUnits
            .Select(x => (_context.Main.Units[x.Unit].Position, Data: x.Damage))
            .ToList();

        _context.Main.DamageIndicator.SetDamage(positions);

        var defeatedComponents = defeatedUnits.Select(x => _context.Main.Units[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Visible = false;
        }

        Bus.Global.Publish(new CompletedEvent(_context.Data.CurrentUnit));
    }
}
