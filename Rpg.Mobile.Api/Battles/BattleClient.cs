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

    private void SetupEventHandlers()
    {
        _hub.On<string, List<BattleUnitData>, BattleSetupPhaseData>(nameof(IBattleEventApi.SetupStarted),
            (gameId, units, setupData) => SetupStarted?.Invoke(gameId, units, setupData));

        _hub.On<string, int, int, Point>(nameof(IBattleEventApi.UnitPlaced),
            (gameId, unitId, currentPlaceOrderIndex, tile) => UnitPlaced?.Invoke(gameId, unitId, currentPlaceOrderIndex, tile));
    }
}