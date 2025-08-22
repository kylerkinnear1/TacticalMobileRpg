using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.Server.Battles;
using Rpg.Mobile.Server.Battles.Calculators;
using Rpg.Mobile.Server.Battles.StateMachines.Phases;

namespace Rpg.Mobile.Server.Lobby;

public interface ILobbyProvider
{
    Task ConnectToGame(Hub<IEventApi> hub, string gameId, IEnumerable<BattleUnitType> team);
    Task LeaveGame(Hub<IEventApi> hub, string gameId);
    Task EndGame(Hub<IEventApi> hub, string gameId);
    Task OnDisconnectedAsync(Hub<IEventApi> hub, Exception? exception);
}

public class LobbyProvider(
    MapLoader _mapLoader,
    ConcurrentDictionary<string, GameContext> _games,
    IPathCalculator _path,
    IMagicDamageCalculator _magicDamage,
    IAttackDamageCalculator _attackDamage,
    ISelectingAttackTargetCalculator _attackTargetCalc,
    ISelectingMagicTargetCalculator _magicTargetCalc,
    IBattleProvider _battleProvider) : ILobbyProvider
{
    public async Task ConnectToGame(Hub<IEventApi> hub, string gameId, IEnumerable<BattleUnitType> team)
    {
        var game = _games.GetOrAdd(gameId, _ => new GameContext());
        var gameIsFull = false;
        var didGameStart = false;
        var playerId = 0;
        var battleData = new BattleData();

        lock (game.Lock)
        {
            gameIsFull = !string.IsNullOrEmpty(game.Player0ConnectionId) &&
                         !string.IsNullOrEmpty(game.Player1ConnectionId);

            if (!gameIsFull)
            {
                playerId = AssignPlayerSlot(hub.Context.ConnectionId, game, team);
                didGameStart = !string.IsNullOrEmpty(game.Player0ConnectionId) &&
                               !string.IsNullOrEmpty(game.Player1ConnectionId);

                if (didGameStart)
                {
                    battleData = StartGame(game, hub, gameId);
                }
            }
        }

        if (gameIsFull)
        {
            await hub.Clients.Caller.GameFull(gameId);
            return;
        }

        await hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, gameId);
        await hub.Clients
            .Group(gameId)
            .PlayerJoined(gameId, playerId);

        if (didGameStart)
        {
            await hub.Clients
                .Group(gameId)
                .GameStarted(gameId, battleData);
        }
    }

    public async Task LeaveGame(Hub<IEventApi> hub, string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return;
        }

        int? playerId = null;
        lock (game.Lock)
        {
            if (game.Player0ConnectionId == hub.Context.ConnectionId)
            {
                game.Player0ConnectionId = null;
                playerId = 0;
            }
            else if (game.Player1ConnectionId == hub.Context.ConnectionId)
            {
                game.Player1ConnectionId = null;
                playerId = 1;
            }

            // Clean up empty games
            if (string.IsNullOrEmpty(game.Player0ConnectionId) &&
                string.IsNullOrEmpty(game.Player1ConnectionId))
            {
                EndGame(gameId, game);
            }

            game.BattlePhase?.Dispose();
        }

        if (playerId.HasValue)
        {
            await hub.Groups.RemoveFromGroupAsync(hub.Context.ConnectionId, gameId);
            await hub.Clients.Group(gameId).PlayerLeft(gameId, playerId.Value);
        }
    }

    public async Task EndGame(Hub<IEventApi> hub, string gameId)
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
            isValidPlayer = game.Player0ConnectionId == hub.Context.ConnectionId ||
                            game.Player1ConnectionId == hub.Context.ConnectionId;

            player0Id = game.Player0ConnectionId;
            player1Id = game.Player1ConnectionId;

            if (!isValidPlayer)
                return;

            EndGame(gameId, game);
        }

        await hub.Clients
            .Group(gameId)
            .GameEnded(gameId);

        // Remove players from group
        if (!string.IsNullOrEmpty(player0Id))
            await hub.Groups.RemoveFromGroupAsync(player0Id, gameId);
        if (!string.IsNullOrEmpty(player1Id))
            await hub.Groups.RemoveFromGroupAsync(player1Id, gameId);
    }

    public async Task OnDisconnectedAsync(Hub<IEventApi> hub, Exception? exception)
    {
        // Handle player disconnection - find and clean up any games they were in
        foreach (var (gameId, game) in _games)
        {
            int? playerId = null;
            lock (game.Lock)
            {
                if (game.Player0ConnectionId != hub.Context.ConnectionId &&
                    game.Player1ConnectionId != hub.Context.ConnectionId)
                {
                    continue;
                }

                playerId = RemovePlayer(hub.Context.ConnectionId, game);
                if (string.IsNullOrEmpty(game.Player0ConnectionId) &&
                    string.IsNullOrEmpty(game.Player1ConnectionId))
                {
                    EndGame(gameId, game);
                }
            }

            if (!playerId.HasValue)
                continue;

            await hub.Clients
                .Group(gameId)
                .PlayerLeft(gameId, playerId.Value);
        }
    }

    private int AssignPlayerSlot(string connectionId, GameContext game, IEnumerable<BattleUnitType> team)
    {
        if (string.IsNullOrEmpty(game.Player0ConnectionId))
        {
            game.Player0ConnectionId = connectionId;
            game.Data.Team0 = team.ToList();
            return 0;
        }

        game.Player1ConnectionId = connectionId;
        game.Data.Team1 = team.ToList();
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

    private void EndGame(string gameId, GameContext game)
    {
        _games.TryRemove(gameId, out _);
        _battleProvider.UnsubscribeFromGame(gameId);
        game.BattlePhase?.Dispose();
    }
    
    private BattleData StartGame(GameContext game, Hub<IEventApi> hub, string gameId)
    {
        var battleData = _mapLoader.LoadBattleData();
        game.Data = battleData;

        var bus = new EventBus();
        game.BattlePhase = new BattlePhaseMachine(
            battleData,
            bus,
            _path,
            _attackTargetCalc,
            _magicTargetCalc,
            _magicDamage,
            _attackDamage);
        
        _battleProvider.SubscribeToGame(hub, gameId, game.Bus);
        return battleData;
    }
}