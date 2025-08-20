using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.Server.Battles;
using Rpg.Mobile.Server.Lobby;

namespace Rpg.Mobile.Server;

public class GameHub(ILobbyProvider _lobbyProvider)
    : Hub<IEventApi>, ICommandApi
{
    public async Task ConnectToGame(string gameId) =>
        await _lobbyProvider.ConnectToGame(this, gameId);

    public async Task LeaveGame(string gameId) =>
        await _lobbyProvider.LeaveGame(this, gameId);

    public async Task EndGame(string gameId) =>
        await _lobbyProvider.EndGame(this, gameId);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _lobbyProvider.OnDisconnectedAsync(this, exception);
        await base.OnDisconnectedAsync(exception);
    }
}