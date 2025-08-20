using Microsoft.AspNetCore.SignalR.Client;
using static Rpg.Mobile.Api.ILobbyClient;

namespace Rpg.Mobile.Api;

public interface ILobbyClient
{
    Task ConnectToGame(string gameId);
    Task LeaveGame(string gameId);
    Task EndGame(string gameId);

    public delegate void PlayerJoinedHandler(string gameId, int playerId);
    event PlayerJoinedHandler? PlayerJoined;

    public delegate void GameStartedHandler(string gameId, BattleData battle);
    event GameStartedHandler? GameStarted;

    public delegate void GameFullHandler(string gameId);
    event GameFullHandler? GameFull;

    public delegate void PlayerLeftHandler(string gameId, int playerId);
    event PlayerLeftHandler? PlayerLeft;

    public delegate void GameEndedHandler(string gameId);
    event GameEndedHandler? GameEnded;
}

public class LobbyClient : ILobbyClient
{
    private readonly HubConnection _hub;

    public LobbyClient(HubConnection hub)
    {
        _hub = hub;
        SetupEventHandlers();
    }

    public async Task ConnectToGame(string gameId)
    {
        await _hub.InvokeAsync("ConnectToGame", gameId);
    }

    public async Task LeaveGame(string gameId)
    {
        await _hub.InvokeAsync("LeaveGame", gameId);
    }

    public async Task EndGame(string gameId)
    {
        await _hub.InvokeAsync("EndGame", gameId);
    }

    public event PlayerJoinedHandler? PlayerJoined;
    public event GameStartedHandler? GameStarted;
    public event GameFullHandler? GameFull;
    public event PlayerLeftHandler? PlayerLeft;
    public event GameEndedHandler? GameEnded;

    private void SetupEventHandlers()
    {
        _hub.On<string, int>("PlayerJoined", (gameId, playerId) =>
        {
            PlayerJoined?.Invoke(gameId, playerId);
        });

        _hub.On<string, BattleData>("GameStarted", (gameId, battleData) =>
        {
            GameStarted?.Invoke(gameId, battleData);
        });

        _hub.On<string>("GameFull", (gameId) =>
        {
            GameFull?.Invoke(gameId);
        });

        _hub.On<string, int>("PlayerLeft", (gameId, playerId) =>
        {
            PlayerLeft?.Invoke(gameId, playerId);
        });

        _hub.On<string>("GameEnded", (gameId) =>
        {
            GameEnded?.Invoke(gameId);
        });
    }
}
