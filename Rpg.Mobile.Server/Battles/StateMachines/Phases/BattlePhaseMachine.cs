using Rpg.Mobile.Api;
using Rpg.Mobile.App.Game.MainBattle.Calculators;
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
            Bus.Global.Subscribe<SetupPhaseServer.CompletedEvent>(_ => StartFirstRound()),
            Bus.Global.Subscribe<ActivePhaseServer.NotEnoughMpEvent>(_ => ShowMessage("Not enough MP.")),
            Bus.Global.Subscribe<ActivePhaseServer.CompletedEvent>(_ => UnitTurnEnded()),
            Bus.Global.Subscribe<SelectingAttackTargetStepServer.AttackTargetSelectedEvent>(ApplyDamage),
            Bus.Global.Subscribe<SelectingMagicTargetStepServer.MagicTargetSelectedEvent>(ApplyDamage),
            Bus.Global.Subscribe<DamagePhaseServer.CompletedEvent>(_ => UnitTurnEnded())
        ];
    }

    public void Change(IBattlePhase phase) => _state.Change(phase);
    public void Execute(float deltaTime) => _state.Execute(deltaTime);
    
    private void StartFirstRound()
    {
        _state.Change(new NewRoundPhase(_context));
        _state.Change(new ActivePhaseServer(_context));
    }

    private void UnitTurnEnded()
    {
        _context.Data.Active.ActiveUnitIndex = (_context.Data.Active.ActiveUnitIndex + 1) % _context.Data.Active.TurnOrder.Count;

        if (_context.Data.Active.ActiveUnitIndex == 0)
            _state.Change(new NewRoundPhase(_context));

        _state.Change(new ActivePhaseServer(_context));
    }

    private void ApplyDamage(SelectingAttackTargetStepServer.AttackTargetSelectedEvent evnt)
    {
        var phase = new DamagePhaseServer(_context);
        _state.Change(phase);
        phase.PerformAttack(evnt);
    }

    private void ApplyDamage(SelectingMagicTargetStepServer.MagicTargetSelectedEvent evnt)
    {
        var phase = new DamagePhaseServer(_context);
        _state.Change(phase);
        phase.CastSpell(evnt);
    }

    public void Dispose() => _subscriptions.DisposeAll();
}

public class BattlePhaseMachineClient
{
    private void ShowMessage(string message)
    {
        _context.Main.Message.Position = new(_context.Main.Map.Bounds.Left, _context.Main.Map.Bounds.Top - 10f);
        _context.Main.Message.Play(message);
    }
}
