using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Setup;

namespace Rpg.Mobile.Server.Battles;

public interface IBattleProvider
{
    Task TileClicked(GameHub hub, string gameId, Point tile);
    Task AttackClicked(GameHub hub, string gameId);
    Task MagicClicked(GameHub hub, string gameId);
    Task SpellSelected(GameHub hub, string gameId, SpellType spellType);
    Task WaitClicked(GameHub hub, string gameId);

    void SubscribeToGame(Hub<IEventApi> hub, string gameId, IEventBus Bus);
    void UnsubscribeFromGame(string gameId);
}

public class BattleProvider : IBattleProvider
{
    private readonly ConcurrentDictionary<string, GameContext> _games;
    private readonly ConcurrentDictionary<string, ISubscription[]> _gameSubscriptions = new();

    public BattleProvider(ConcurrentDictionary<string, GameContext> games)
    {
        _games = games;
    }

    public Task TileClicked(GameHub hub, string gameId, Point tile)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return Task.CompletedTask;

        lock (game.Lock)
        {
            var playerId = GetPlayerId(game, hub.Context.ConnectionId);
            if (playerId.HasValue)
                game.Bus.Publish(new TileClickedEvent(playerId.Value, tile));
        }

        return Task.CompletedTask;
    }

    public Task AttackClicked(GameHub hub, string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return Task.CompletedTask;

        lock (game.Lock)
        {
            var playerId = GetPlayerId(game, hub.Context.ConnectionId);
            if (playerId.HasValue)
                game.Bus.Publish(new ActivePhase.AttackClickedEvent(playerId.Value));
        }

        return Task.CompletedTask;
    }

    public Task MagicClicked(GameHub hub, string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return Task.CompletedTask;

        lock (game.Lock)
        {
            var playerId = GetPlayerId(game, hub.Context.ConnectionId);
            if (playerId.HasValue)
                game.Bus.Publish(new ActivePhase.MagicClickedEvent(playerId.Value));
        }

        return Task.CompletedTask;
    }

    public Task SpellSelected(GameHub hub, string gameId, SpellType spellType)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return Task.CompletedTask;

        lock (game.Lock)
        {
            var playerId = GetPlayerId(game, hub.Context.ConnectionId);
            if (playerId.HasValue)
                game.Bus.Publish(new ActivePhase.SpellSelectedEvent(playerId.Value, spellType));
        }

        return Task.CompletedTask;
    }

    public Task WaitClicked(GameHub hub, string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return Task.CompletedTask;

        lock (game.Lock)
        {
            var playerId = GetPlayerId(game, hub.Context.ConnectionId);
            if (playerId.HasValue)
                game.Bus.Publish(new IdleStep.CompletedEvent(game.Data.CurrentUnit()));
        }

        return Task.CompletedTask;
    }
    
    public void SubscribeToGame(Hub<IEventApi> hub, string gameId, IEventBus bus)
    {
        _gameSubscriptions.GetOrAdd(gameId, _ =>
        [
            bus.SubscribeAsync<ActivePhase.UnitMovedEvent>(async x =>
                await hub
                    .Clients
                    .Group(gameId)
                    .UnitMoved(gameId, x.Tile)),
            bus.SubscribeAsync<SetupPhase.StartedEvent>(async x =>
                await hub
                    .Clients
                    .Group(gameId)
                    .SetupStarted(gameId, x.Units, x.SetupData))
        ]);
    }
    
    public void UnsubscribeFromGame(string gameId)
    {
        if (!_gameSubscriptions.TryGetValue(gameId, out var subscriptions))
            return;

        subscriptions.DisposeAll();
    }
    
    private int? GetPlayerId(GameContext game, string connectionId)
    {
        if (game.Player0ConnectionId == connectionId) 
            return 0;
        if (game.Player1ConnectionId == connectionId)
            return 1;

        return null;
    }
}