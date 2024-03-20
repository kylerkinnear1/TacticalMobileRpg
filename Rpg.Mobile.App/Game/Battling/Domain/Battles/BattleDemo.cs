using Rpg.Mobile.GameSdk;
using System.Text.Json;

namespace Rpg.Mobile.App.Game.Battling.Domain.Battles;

public record Point32(int X, int Y);
public record MapJson(
    int RowCount,
    int ColumnCount,
    List<Point32> Player1Origins,
    List<Point32> Player2Origins,
    List<Point32> RockPositions);

public static partial class Battles
{
    public static readonly MapState Demo = CreateDemo();

    private static MapState CreateDemo()
    {
        var mapJson = LoadMap();

        var state = MapState.New(mapJson.RowCount, mapJson.ColumnCount);
        var player1Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        foreach (var (unit, point) in player1Units.Zip(mapJson.Player1Origins))
            state.UnitTiles[unit] = new(point.X, point.Y);

        foreach (var (unit, point) in player2Units.Zip(mapJson.Player2Origins))
            state.UnitTiles[unit] = new(point.X, point.Y);

        mapJson.RockPositions.ForEach(x => state.Tiles[x.X, x.Y].Type = TerrainType.Rock);

        var allUnits = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        state.TurnOrder.AddRange(allUnits);

        return state;
    }

    private static MapJson? LoadMap()
    {
        var mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.json");
        if (!File.Exists(mapPath)) 
            return BuildDefaultMap();

        var mapText = File.ReadAllText(mapPath);
        return JsonSerializer.Deserialize<MapJson>(mapText);
    }

    private static MapJson BuildDefaultMap() =>
        new(10, 12,
            new List<Point32>
            {
                new(1, 1), new(1, 3), new(1, 5), new(1, 7), new(1, 9)
            },
            new List<Point32>
            {
                new(8, 1), new(8, 3), new(8, 5), new(8, 7), new(8, 9)
            },
            new List<Point32>
            {
                new(3, 5), new(6, 4), new(6, 5), new(3, 4), new(2, 2), new(7, 9), new(4, 8), new(5, 8)
            });
}
