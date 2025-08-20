using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.Server.Battles;

namespace Rpg.Mobile.Server.Lobby;

public class LobbyHub(
    ConcurrentDictionary<string, GameContext> _games,
    MapLoaderServer _mapLoader)
    : Hub<ILobbyEventApi>, ILobbyCommandApi
{
    public async Task ConnectToGame(string gameId)
    {
        var game = _games.GetOrAdd(gameId, _ => new GameContext());
        var gameIsFull = false;
        var didGameStart = false;
        var playerId = 0;
        lock (game.Lock)
        {
            gameIsFull = !string.IsNullOrEmpty(game.Player0ConnectionId) &&
                             !string.IsNullOrEmpty(game.Player1ConnectionId);
            
            if (!gameIsFull)
            {
                playerId = AssignPlayerSlot(game);
                didGameStart = !string.IsNullOrEmpty(game.Player0ConnectionId) &&
                               !string.IsNullOrEmpty(game.Player1ConnectionId);
            }
        }

        if (gameIsFull)
        {
            await Clients.Caller.GameFull(gameId);
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        await Clients
            .Group(gameId)
            .PlayerJoined(gameId, playerId);

        if (didGameStart)
        {
            var battleData = _mapLoader.LoadBattleData();
            await Clients
                .Group(gameId)
                .GameStarted(gameId, battleData);
        }
    }

    public async Task LeaveGame(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return;
        }

        int? playerId = null;
        lock (game.Lock)
        {
            if (game.Player0ConnectionId == Context.ConnectionId)
            {
                game.Player0ConnectionId = null;
                playerId = 0;
            }
            else if (game.Player1ConnectionId == Context.ConnectionId)
            {
                game.Player1ConnectionId = null;
                playerId = 1;
            }

            // Clean up empty games
            if (string.IsNullOrEmpty(game.Player0ConnectionId) &&
                string.IsNullOrEmpty(game.Player1ConnectionId))
            {
                _games.TryRemove(gameId, out _);
            }
        }

        if (playerId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).PlayerLeft(gameId, playerId.Value);
        }
    }

    public async Task EndGame(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return;
        }

        var isValidPlayer = false;
        var player0Id = "";
        var player1Id = "";
        lock (game.Lock)
        {
            isValidPlayer = game.Player0ConnectionId == Context.ConnectionId ||
                            game.Player1ConnectionId == Context.ConnectionId;

            player0Id = game.Player0ConnectionId;
            player1Id = game.Player1ConnectionId;
        }
        
        if (!isValidPlayer)
            return;
        
        await Clients
            .Group(gameId)
            .GameEnded(gameId);

        // Remove players from group
        if (!string.IsNullOrEmpty(player0Id))
            await Groups.RemoveFromGroupAsync(player0Id, gameId);
        if (!string.IsNullOrEmpty(player1Id))
            await Groups.RemoveFromGroupAsync(player1Id, gameId);

        // Clean up game
        _games.TryRemove(gameId, out _);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Handle player disconnection - find and clean up any games they were in
        foreach (var (gameId, game) in _games)
        {
            int? playerId = null;
            lock (game.Lock)
            {
                if (game.Player0ConnectionId != Context.ConnectionId &&
                    game.Player1ConnectionId != Context.ConnectionId)
                {
                    continue;
                }
                
                playerId = RemovePlayer(Context.ConnectionId, game);
                if (string.IsNullOrEmpty(game.Player0ConnectionId) &&
                    string.IsNullOrEmpty(game.Player1ConnectionId))
                {
                    _games.TryRemove(gameId, out _);
                }
            }

            if (!playerId.HasValue) 
                continue;
            
            await Clients
                .Group(gameId)
                .PlayerLeft(gameId, playerId.Value);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private int AssignPlayerSlot(GameContext game)
    {
        if (string.IsNullOrEmpty(game.Player0ConnectionId))
        {
            game.Player0ConnectionId = Context.ConnectionId;
            return 0;
        }

        game.Player1ConnectionId = Context.ConnectionId;
        return 1;
    }
    
    private int? RemovePlayer(string connectionId, GameContext game)
    {
        if (game.Player0ConnectionId == connectionId)
        {
            game.Player0ConnectionId = null;
            return 0;
        }
            
        if (game.Player1ConnectionId == connectionId)
        {
            game.Player1ConnectionId = null;
            return 1;
        }

        return null;
    }
}