using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Components.States;

public class TurnStartedState : IState
{
    private readonly BattleData _data;
    private readonly BattleComponent _component;

    public void Enter()
    {
        _data.TurnOrder.Set(_data.TurnOrder.Shuffle(Rng.Instance).ToList());
        _component.Units.Values.ToList().ForEach(x => x.HealthBar.HasGone = false);
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
