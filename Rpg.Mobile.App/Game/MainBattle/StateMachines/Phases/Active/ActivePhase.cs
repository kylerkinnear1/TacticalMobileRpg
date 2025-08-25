using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;

public class ActivePhase : IBattlePhase
{
    public interface IStep : IState
    {
    }

    public record BackClickedEvent : IEvent;
    public record AttackClickedEvent : IEvent;
    public record MagicClickedEvent : IEvent;
    public record WaitClickedEvent : IEvent;
    
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;
    private readonly MenuComponent _menu;
    private readonly IEventBus _bus;
    private readonly ISelectingAttackTargetCalculator _attackTargetCalculator;
    private readonly ISelectingMagicTargetCalculator _magicTargetCalculator;

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IStep> _step = new();

    public ActivePhase(
        BattleData data, 
        MainBattleComponent mainBattle, 
        MenuComponent menu, 
        IEventBus bus,
        ISelectingAttackTargetCalculator attackTargetCalculator,
        ISelectingMagicTargetCalculator magicTargetCalculator)
    {
        _data = data;
        _mainBattle = mainBattle;
        _menu = menu;
        _bus = bus;
        _attackTargetCalculator = attackTargetCalculator;
        _magicTargetCalculator = magicTargetCalculator;
    }

    public void Enter()
    {
        _subscriptions =
        [
            _bus.Subscribe<BattleNetwork.IdleStepStartedEvent>(IdleStepStarted),
            _bus.Subscribe<BattleNetwork.SelectingAttackTargetStartedEvent>(SelectingAttackTargetStarted),
            _bus.Subscribe<BattleNetwork.SelectingMagicTargetStartedEvent>(SelectingMagicTargetStarted),
            _bus.Subscribe<BattleNetwork.SelectingSpellStartedEvent>(SelectingSpellStarted)
        ];
    }

    public void Execute(float deltaTime)
    {
        var currentUnitPosition = _data.UnitCoordinates[_data.CurrentUnit().UnitId];
        _mainBattle.CurrentUnitShadow.Shadows.SetSingle(
            new(
                currentUnitPosition.X * MainBattleComponent.TileWidth, 
                currentUnitPosition.Y * MainBattleComponent.TileWidth, 
                MainBattleComponent.TileWidth, 
                MainBattleComponent.TileWidth));
        
        _step.Execute(deltaTime);
    }

    public void Leave()
    {
        _subscriptions.DisposeAll();
    }
    
    private void IdleStepStarted(BattleNetwork.IdleStepStartedEvent evnt)
    {
        _data.Active.WalkableTiles = evnt.WalkableTiles;
        _step.Change(new IdleStep(_menu, _bus, _data, _mainBattle));
    }
    
    private void SelectingAttackTargetStarted(BattleNetwork.SelectingAttackTargetStartedEvent evnt)
    {
        _data.Active.AttackTargetTiles = evnt.AttackTargetTiles;
        _step.Change(new SelectingAttackTargetStep(_mainBattle, _menu, _bus, _data, _attackTargetCalculator));
    }
    
    private void SelectingMagicTargetStarted(BattleNetwork.SelectingMagicTargetStartedEvent evnt)
    {
        _data.Active.SpellTargetTiles = evnt.MagicTargetTiles;
        _data.Active.CurrentSpell = evnt.Spell;
        _step.Change(new SelectingMagicTargetStep(_mainBattle, _menu, _data, _bus, _magicTargetCalculator));
    }
    
    private void SelectingSpellStarted(BattleNetwork.SelectingSpellStartedEvent evnt)
    {
        _data.CurrentUnit().Spells = evnt.Spells;
        _step.Change(new SelectingSpellStep(_data, _menu, _bus));
    }
}