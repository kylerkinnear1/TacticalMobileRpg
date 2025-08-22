using Microsoft.AspNetCore.SignalR;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Server.Battles;
using Rpg.Mobile.Server.Lobby;

namespace Rpg.Mobile.Server;

public class GameHub : Hub<IEventApi>, ICommandApi
{
    private readonly ILobbyProvider _lobbyProvider;
    private readonly IBattleProvider _battleProvider;

    public GameHub(ILobbyProvider lobbyProvider, IBattleProvider battleProvider)
    {
        _lobbyProvider = lobbyProvider;
        _battleProvider = battleProvider;
    }

    public async Task ConnectToGame(string gameId, List<BattleUnitType> team) =>
        await _lobbyProvider.ConnectToGame(this, gameId, team);

    public async Task LeaveGame(string gameId) =>
        await _lobbyProvider.LeaveGame(this, gameId);

    public async Task EndGame(string gameId) =>
        await _lobbyProvider.EndGame(this, gameId);

    public async Task TileClicked(string gameId, Point tile) =>
        await _battleProvider.TileClicked(this, gameId, tile);

    public async Task AttackClicked(string gameId) =>
        await _battleProvider.AttackClicked(this, gameId);

    public async Task MagicClicked(string gameId) =>
        await _battleProvider.MagicClicked(this, gameId);

    public async Task SpellSelected(string gameId, SpellType spellType) =>
        await _battleProvider.SpellSelected(this, gameId, spellType);

    public async Task WaitClicked(string gameId) =>
        await _battleProvider.WaitClicked(this, gameId);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _lobbyProvider.OnDisconnectedAsync(this, exception);
        await base.OnDisconnectedAsync(exception);
    }
}