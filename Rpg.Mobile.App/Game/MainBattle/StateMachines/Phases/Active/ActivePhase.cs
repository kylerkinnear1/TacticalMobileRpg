using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Damage;
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
            Bus.Global.Subscribe<IdleStep.CompletedEvent>(evnt => Bus.Global.Publish(new CompletedEvent(evnt.CurrentUnit)))
        ];
        
        _context.Data.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
        _step.Change(new IdleStep(_context));
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
}
