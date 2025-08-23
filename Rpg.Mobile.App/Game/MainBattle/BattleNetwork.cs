using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
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
    private readonly IGameLoop _game;
    private readonly GameSettings _settings;
    
    private ISubscription[] _subscriptions = [];

    public BattleNetwork(IBattleClient battleClient, IEventBus bus, IGameLoop game, GameSettings settings)
    {
        _battleClient = battleClient;
        _bus = bus;
        _game = game;
        _settings = settings;
    }

    public void Connect()
    {
        _battleClient.SetupStarted += SetupStarted;

        _subscriptions =
        [
            _bus.Subscribe<GridComponent.TileClickedEvent>(TileClicked)
        ];
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
    
    private void TileClicked(GridComponent.TileClickedEvent evnt)
    {
        _battleClient.TileClicked(_settings.GameId, evnt.Tile);
    }
}