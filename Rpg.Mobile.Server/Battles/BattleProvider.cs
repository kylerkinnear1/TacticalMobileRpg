namespace Rpg.Mobile.Server.Battles;

public interface IBattleProvider
{
    Task UnitMovementStarted(string gameId, Point targetTile);
}

public class BattleProvider
{
    
}