using Rpg.Mobile.App.Game.MainBattle.Calculators;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.NewRound;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Setup;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

public interface IBattlePhase : IState { }
public class BattlePhaseMachine: StateMachine<IBattlePhase>, IDisposable
{
    public record Context(
        BattleData Data,
        MainBattleComponent Main,
        BattleMenuComponent Menu,
        IPathCalculator Path);

    private readonly ISubscription[] _subscriptions;
    private readonly Context _context;

    public BattlePhaseMachine(Context context)
    {
        _context = context;
        _subscriptions =
        [
            Bus.Global.Subscribe<SetupPhase.UnitPlacementCompletedEvent>(_ => StartFirstRound()),
            Bus.Global.Subscribe<ActivePhase.NotEnoughMpEvent>(_ => ShowMessage("Not enough MP.")),
            Bus.Global.Subscribe<ActivePhase.CompletedEvent>(_ => UnitTurnEnded())
        ];
    }

    private void StartFirstRound()
    {
        Change(new NewRoundPhase(_context));
        Change(new ActivePhase(_context));
    }

    private void UnitTurnEnded()
    {
        // TODO: Make Select Unit Phase and Steps
        _context.Main.Units[_context.Data.CurrentUnit].Unit.HealthBar.HasGone = true;
        _context.Data.ActiveUnitIndex = (_context.Data.ActiveUnitIndex + 1) % _context.Data.TurnOrder.Count;

        if (_context.Data.ActiveUnitIndex == 0)
            Change(new NewRoundPhase(_context));

        Change(new ActivePhase(_context));
    }
    
    private void ShowMessage(string message)
    {
        _context.Main.Message.Position = new(_context.Main.Map.Bounds.Left, _context.Main.Map.Bounds.Top - 10f);
        _context.Main.Message.Play(message);
    }

    public void Dispose() => _subscriptions.DisposeAll();
}


