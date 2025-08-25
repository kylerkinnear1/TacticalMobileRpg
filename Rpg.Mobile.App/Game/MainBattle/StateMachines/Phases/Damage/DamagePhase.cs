using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Damage;

public class DamagePhase : IBattlePhase
{
    public record CompletedEvent(BattleUnitData BattleUnit) : IEvent;
    
    private readonly MainBattleComponent _mainBattle;
    private readonly IEventBus _bus;
    private readonly BattleData _data;
    
    public DamagePhase(MainBattleComponent mainBattle, IEventBus bus, BattleData data)
    {
        _mainBattle = mainBattle;
        _bus = bus;
        _data = data;
    }

    public void Enter()
    {
        _mainBattle.DamageIndicator.Visible = true;
    }
    
    public void Leave()
    {
        _mainBattle.DamageIndicator.Visible = false;
    }
    
    public void Execute(float deltaTime)
    {
        if (_mainBattle.DamageIndicator.IsPlaying)
            return;
        
        _bus.Publish(new CompletedEvent(_data.CurrentUnit()));
    }
}