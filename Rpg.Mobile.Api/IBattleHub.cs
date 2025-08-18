namespace Rpg.Mobile.Api;

public interface IBattleHubServer
{
    Task JoinGame(string gameId);
}

public interface IBattleHubClient
{
    Task Connected(string connectionId);
    
    Task GameJoined(string gameId);
    Task GameAlreadyStarted(string gameId);
    
    Task Disconnected(string connectionId);
}