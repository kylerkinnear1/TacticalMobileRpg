using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle;

public class BattleNetwork
{
    public record SetupStartedEvent(BattleSetupPhaseData Data) : IEvent;
    
    public record UnitsDamagedEvent(
        List<(BattleUnitData Unit, int Damage)> DamagedUnits,
        List<BattleUnitData> DefeatedUnits) : IEvent;

    public record UnitPlacedEvent(BattleUnitData Unit, Point Tile) : IEvent;

    private readonly IBattleClient _battleClient;
    private readonly IEventBus _bus;
    private ISubscription[] _subscriptions = [];
    private readonly IGameLoop _game;
    
    public BattleNetwork(IBattleClient battleClient, IEventBus bus, IGameLoop game)
    {
        _battleClient = battleClient;
        _bus = bus;
        _game = game;
    }
    
    public void Connect()
    {
        _battleClient.SetupStarted += SetupStarted;
    }

    public void Disconnect()
    {
        _battleClient.SetupStarted -= SetupStarted;
        _subscriptions.DisposeAll();
    }
    
    private void SetupStarted(string gameId, BattleSetupPhaseData data)
    {
        _game.Execute(() => _bus.Publish(new SetupStartedEvent(data)));
    }
}