using System.Drawing;
using static Rpg.Mobile.GameEngine.RuleEngine.Battling.Turn;

namespace Rpg.Mobile.GameEngine.RuleEngine.Battling;

public record PlayerId(int Id);

public record Battle(
    Board Board,
    List<Turn> Turns);

public record Turn(
    int Number,
    List<UnitId> RemainingActivations,
    List<Event> Log)
{
    public record Event(PlayerId ActivePlayer);
}

public record Board(
    Map Map,
    Dictionary<UnitId, Unit> Units,
    Dictionary<ObstacleId, Obstacle> Obstacles,
    Point Bounds);

public record Map(
    Dictionary<Point, Tile> Tiles,
    Dictionary<UnitId, Point> InitialUnitPositions,
    Dictionary<ObstacleId, Obstacle> InitialObstaclePositions);

public record Tile(Point Position, bool AllowUnits);

public record ObstacleId(int Id);
public record Obstacle(ObstacleId Id, Point Position);