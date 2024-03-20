using Rpg.Mobile.GameSdk;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Game.Battling.Domain.Battles;

public static partial class Battles
{
    public static readonly MapState Demo = CreateDemo();

    private static MapState CreateDemo()
    {
        var state = MapState.New(10, 12);
        var player1Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        var player1StartingPositions = new List<Point>
        {
            new(1, 1), new(1, 3), new(1, 5), new(1, 7), new(1, 9)
        };

        var player2StartingPositions = new List<Point>
        {
            new(8, 1), new(8, 3), new(8, 5), new(8, 7), new(8, 9)
        };

        foreach (var (unit, point) in player1Units.Zip(player1StartingPositions))
            state.UnitTiles[unit] = point;

        foreach (var (unit, point) in player2Units.Zip(player2StartingPositions))
            state.UnitTiles[unit] = point;

        var rockPositions = new List<Point>
        {
            new(3, 5), new(6, 4), new(6, 5), new(3, 4), new(2, 2), new(7, 9), new(4, 8), new(5, 8)
        };

        rockPositions.ForEach(x => state.Tiles[x.X, x.Y].Type = TerrainType.Rock);

        var allUnits = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        state.TurnOrder.AddRange(allUnits);

        return state;
    }
}
