using Microsoft.AspNetCore.SignalR.Client;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Api.Battles;

public interface IBattleClient
{
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

    public delegate void SetupPhaseStartedHandler(string gameId, BattleSetupPhaseData data);
    public event SetupPhaseStartedHandler? SetupStarted;

    private void SetupEventHandlers()
    {
        _hub.On<string, BattleSetupPhaseData>(nameof(IBattleEventApi.SetupStarted),
            (gameId, data) => { SetupStarted?.Invoke(gameId, data); });
    }
}