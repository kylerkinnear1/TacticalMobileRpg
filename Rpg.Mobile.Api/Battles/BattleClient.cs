using System.Drawing;
using Microsoft.AspNetCore.SignalR.Client;
using Rpg.Mobile.Api.Battles.Data;
using static Rpg.Mobile.Api.Battles.IBattleClient;

namespace Rpg.Mobile.Api.Battles;

public interface IBattleClient
{
    Task TileClicked(string gameId, Point tile);
    
    public delegate void SetupPhaseStartedHandler(string gameId, List<BattleUnitData> units, BattleSetupPhaseData data);
    event SetupPhaseStartedHandler? SetupStarted;
    
    public delegate void UnitPlacedHandler(string gameId, int unitId, int currentPlaceOrderIndex, Point tile);
    event UnitPlacedHandler? UnitPlaced;
    
    public delegate void NewRoundStartedHandler(string gameId, List<int> turnOrderIds, int activeUnitIndex);
    event NewRoundStartedHandler? NewRoundStarted;

    public delegate void ActivePhaseStartedHandler(string gameId, BattleActivePhaseData activePhaseData);
    event ActivePhaseStartedHandler? ActivePhaseStarted;

    public delegate void IdleStepStartedHandler(string gameId, List<Point> walkableTiles);
    event IdleStepStartedHandler? IdleStepStarted;

    public delegate void IdleStepEndedHandler(string gameId, int unitId);
    event IdleStepEndedHandler? IdleStepEnded;

    public delegate void UnitMovedHandler(string gameId, int unitId, Point tile);
    event UnitMovedHandler? UnitMoved;
}

public class BattleClient : IBattleClient
{
    private readonly HubConnection _hub;

    public BattleClient(HubConnection hub)
    {
        _hub = hub;
        SetupEventHandlers();
    }
    public async Task TileClicked(string gameId, Point tile)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.TileClicked), gameId, tile);
    }

    public event SetupPhaseStartedHandler? SetupStarted;
    public event UnitPlacedHandler? UnitPlaced;
    public event NewRoundStartedHandler? NewRoundStarted;
    public event ActivePhaseStartedHandler? ActivePhaseStarted;
    public event IdleStepStartedHandler? IdleStepStarted;
    public event IdleStepEndedHandler? IdleStepEnded;
    public event UnitMovedHandler? UnitMoved;

    private void SetupEventHandlers()
    {
        _hub.On<string, List<BattleUnitData>, BattleSetupPhaseData>(nameof(IBattleEventApi.SetupStarted),
            (gameId, units, setupData) => SetupStarted?.Invoke(gameId, units, setupData));

        _hub.On<string, int, int, Point>(nameof(IBattleEventApi.UnitPlaced),
            (gameId, unitId, currentPlaceOrderIndex, tile) => UnitPlaced?.Invoke(gameId, unitId, currentPlaceOrderIndex, tile));
        
        _hub.On<string, List<int>, int>(nameof(IBattleEventApi.NewRoundStarted),
            (gameId, turnOrderIds, activeUnitIndex) => NewRoundStarted?.Invoke(gameId, turnOrderIds, activeUnitIndex));

        _hub.On<string, BattleActivePhaseData>(nameof(IBattleEventApi.ActivePhaseStarted),
            (gameId, activePhaseData) => ActivePhaseStarted?.Invoke(gameId, activePhaseData));

        _hub.On<string, List<Point>>(nameof(IBattleEventApi.IdleStepStarted),
            (gameId, walkableTiles) => IdleStepStarted?.Invoke(gameId, walkableTiles));

        _hub.On<string, int>(nameof(IBattleEventApi.IdleStepEnded),
            (gameId, unitId) => IdleStepEnded?.Invoke(gameId, unitId));

        _hub.On<string, int, Point>(nameof(IBattleEventApi.UnitMoved),
            (gameId, unitId, tile) => UnitMoved?.Invoke(gameId, unitId, tile));
    }
}