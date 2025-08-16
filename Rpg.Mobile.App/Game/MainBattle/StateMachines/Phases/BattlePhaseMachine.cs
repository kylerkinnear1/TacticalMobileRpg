using Rpg.Mobile.App.Game.MainBattle.Calculators;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Damage;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.NewRound;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Setup;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

public interface IBattlePhase : IState { }
public class BattlePhaseMachine : IDisposable
{
    public record Context(
        BattleData Data,
        MainBattleComponent Main,
        BattleMenuComponent Menu,
        IPathCalculator Path,
        IMagicDamageCalculator MagicDamageCalc,
        IAttackDamageCalculator AttackDamageCalc);

    private readonly ISubscription[] _subscriptions;
    private readonly Context _context;
    private readonly StateMachine<IBattlePhase> _state = new();

    public BattlePhaseMachine(Context context)
    {
        _context = context;
        _subscriptions =
        [
            Bus.Global.Subscribe<SetupPhase.CompletedEvent>(_ => StartFirstRound()),
            Bus.Global.Subscribe<ActivePhase.NotEnoughMpEvent>(_ => ShowMessage("Not enough MP.")),
            Bus.Global.Subscribe<ActivePhase.CompletedEvent>(_ => UnitTurnEnded()),
            Bus.Global.Subscribe<SelectingAttackTargetStep.AttackTargetSelectedEvent>(ApplyDamage),
            Bus.Global.Subscribe<SelectingMagicTargetStep.MagicTargetSelectedEvent>(ApplyDamage),
            Bus.Global.Subscribe<DamagePhase.CompletedEvent>(_ => UnitTurnEnded())
        ];
    }

    public void Change(IBattlePhase phase) => _state.Change(phase);
    public void Execute(float deltaTime) => _state.Execute(deltaTime);
    
    private void StartFirstRound()
    {
        _state.Change(new NewRoundPhase(_context));
        _state.Change(new ActivePhase(_context));
    }

    private void UnitTurnEnded()
    {
        _context.Main.Units[_context.Data.CurrentUnit].Unit.HealthBar.HasGone = true;
        _context.Data.ActiveUnitIndex = (_context.Data.ActiveUnitIndex + 1) % _context.Data.TurnOrder.Count;

        if (_context.Data.ActiveUnitIndex == 0)
            _state.Change(new NewRoundPhase(_context));

        _state.Change(new ActivePhase(_context));
    }
    
    private void ShowMessage(string message)
    {
        _context.Main.Message.Position = new(_context.Main.Map.Bounds.Left, _context.Main.Map.Bounds.Top - 10f);
        _context.Main.Message.Play(message);
    }

    private void ApplyDamage(SelectingAttackTargetStep.AttackTargetSelectedEvent evnt)
    {
        var phase = new DamagePhase(_context);
        _state.Change(phase);
        phase.PerformAttack(evnt);
    }

    private void ApplyDamage(SelectingMagicTargetStep.MagicTargetSelectedEvent evnt)
    {
        var phase = new DamagePhase(_context);
        _state.Change(phase);
        phase.CastSpell(evnt);
    }

    public void Dispose() => _subscriptions.DisposeAll();
}


