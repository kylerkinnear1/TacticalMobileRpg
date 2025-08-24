using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle;

public class BattleNetwork
{
    public record SetupStartedEvent(List<BattleUnitData> Units, BattleSetupPhaseData SetupData) : IEvent;
    
    public record UnitsDamagedEvent(
        List<(BattleUnitData Unit, int Damage)> DamagedUnits,
        List<BattleUnitData> DefeatedUnits) : IEvent;

    public record UnitPlacedEvent(int UnitId, int CurrentUnitPlaceOrderIndex, Point Tile) : IEvent;
    public record NewRoundStartedEvent(List<int> TurnOrderIds, int ActiveUnitIndex) : IEvent;
    public record ActivePhaseStartedEvent(BattleActivePhaseData ActivePhaseData) : IEvent;
    public record IdleStepStartedEvent(List<Point> WalkableTiles) : IEvent;
    public record IdleStepEndedEvent(int UnitId) : IEvent;

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
        _battleClient.UnitPlaced += UnitPlaced;
        _battleClient.NewRoundStarted += NewRoundStarted;
        _battleClient.ActivePhaseStarted += ActivePhaseStarted;
        _battleClient.IdleStepStarted += IdleStepStarted;
        _battleClient.IdleStepEnded += IdleStepEnded;

        _subscriptions =
        [
            _bus.Subscribe<GridComponent.TileClickedEvent>(TileClicked)
        ];
    }
    
    public void Disconnect()
    {
        _battleClient.SetupStarted -= SetupStarted;
        _battleClient.UnitPlaced -= UnitPlaced;
        _battleClient.NewRoundStarted -= NewRoundStarted;
        _battleClient.ActivePhaseStarted -= ActivePhaseStarted;
        _battleClient.IdleStepStarted -= IdleStepStarted;
        _battleClient.IdleStepEnded -= IdleStepEnded;
        
        _subscriptions.DisposeAll();
    }

    private void UnitPlaced(string gameId, int unitId, int currentPlaceOrderIndex, Point tile)
    {
        _game.Execute(() => _bus.Publish(new UnitPlacedEvent(unitId, currentPlaceOrderIndex, tile)));
    }
    
    private void SetupStarted(string gameId, List<BattleUnitData> units, BattleSetupPhaseData data)
    {
        _game.Execute(() => _bus.Publish(new SetupStartedEvent(units, data)));
    }
    
    private void NewRoundStarted(string gameId, List<int> turnOrderIds, int activeUnitIndex)
    {
        _game.Execute(() => _bus.Publish(new NewRoundStartedEvent(turnOrderIds, activeUnitIndex)));
    }

    private void ActivePhaseStarted(string gameId, BattleActivePhaseData activePhaseData)
    {
        _game.Execute(() => _bus.Publish(new ActivePhaseStartedEvent(activePhaseData)));
    }

    private void IdleStepStarted(string gameId, List<Point> walkableTiles)
    {
        _game.Execute(() => _bus.Publish(new IdleStepStartedEvent(walkableTiles)));
    }

    private void IdleStepEnded(string gameId, int unitId)
    {
        _game.Execute(() => _bus.Publish(new IdleStepEndedEvent(unitId)));
    }
    
    private void TileClicked(GridComponent.TileClickedEvent evnt)
    {
        _battleClient.TileClicked(_settings.GameId, evnt.Tile);
    }
}