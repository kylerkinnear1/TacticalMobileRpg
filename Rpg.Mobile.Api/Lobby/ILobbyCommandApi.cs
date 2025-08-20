using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Api.Lobby;

public interface ILobbyCommandApi
{
    Task ConnectToGame(string gameId);
    Task LeaveGame(string gameId);
    Task EndGame(string gameId);
}

public interface ILobbyEventApi
{
    Task PlayerJoined(string gameId, int playerId);
    Task GameStarted(string gameId, BattleData battle);
    Task GameFull(string gameId);
    Task PlayerLeft(string gameId, int playerId);
    Task GameEnded(string gameId);
}