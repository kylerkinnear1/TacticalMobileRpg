using System.Drawing;
using Microsoft.AspNetCore.SignalR.Client;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Api.Battles;

public interface IBattleClient
{
    Task TileClicked(string gameId, Point tile);
    
    event BattleClient.SetupPhaseStartedHandler? SetupStarted;
}

public class BattleClient : IBattleClient
{
    private readonly HubConnection _hub;

    public BattleClient(HubConnection hub)
    {
        _hub = hub;
        SetupEventHandlers();
    }

    public delegate void SetupPhaseStartedHandler(string gameId, List<BattleUnitData> units, BattleSetupPhaseData data);

    public async Task TileClicked(string gameId, Point tile)
    {
        await _hub.InvokeAsync(nameof(IBattleCommandApi.TileClicked), gameId, tile);
    }

    public event SetupPhaseStartedHandler? SetupStarted;

    private void SetupEventHandlers()
    {
        _hub.On<string, List<BattleUnitData>, BattleSetupPhaseData>(nameof(IBattleEventApi.SetupStarted),
            (gameId, units, setupData) => { SetupStarted?.Invoke(gameId, units, setupData); });
    }
}