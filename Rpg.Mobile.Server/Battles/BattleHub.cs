using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Rpg.Mobile.Api;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

namespace Rpg.Mobile.Server;

public class GameContext
{
    public object Lock { get; } = new();
    public BattlePhaseMachine? BattlePhase { get; set; }
    public string? Player0ConnectionId { get; set; }
    public string? Player1ConnectionId { get; set; }
}

public class BattleHub : Hub<IBattleHubClient>, IBattleHubServer
{
    private readonly ConcurrentDictionary<string, GameContext> _games = new();
    
    public async Task JoinGame(string gameId)
    {
        var game = _games.GetOrAdd(gameId, _ => new GameContext { Player0ConnectionId = Context.ConnectionId });
        
        if (game.Player0ConnectionId != Context.ConnectionId &&
            !string.IsNullOrEmpty(game.Player1ConnectionId) &&
            game.Player1ConnectionId != Context.ConnectionId)
        {
            await Clients.Caller.GameAlreadyStarted(gameId);
            return;
        }
        
        if (game.Player0ConnectionId != Context.ConnectionId)
            game.Player1ConnectionId = Context.ConnectionId;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await Clients.Group(gameId).GameJoined(Context.ConnectionId);
    }
    
    public override async Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is null)
            return;

        await Clients.User(Context.UserIdentifier).Connected(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is null)
            return;

        await Clients.User(Context.UserIdentifier).Disconnected(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}