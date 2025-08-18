using System.Drawing;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Api;

public enum TerrainType
{
    Grass,
    Rock
}

public class TileState
{
    public TerrainType Type { get; set; } = TerrainType.Grass;
}

public class MapData
{
    public Array2d<TileState> Tiles { get; set; } = new(0, 0);
    public List<Point> Player1Origins { get; set; } = new();
    public List<Point> Player2Origins { get; set; } = new();
    public List<BattleUnitType> Team1 { get; set; } = new();
    public List<BattleUnitType> Team2 { get; set; } = new();
    public List<BattleUnitStats> BaseStats { get; set; } = new();
    
    // TODO: Remove
    public int Width() => Tiles.Width;
    public int Height() => Tiles.Height;
    public Point Corner() => new(Width(), Height());
}

// TODO: Remove from API
public record MapJson(
    int RowCount,
    int ColumnCount,
    List<BattleUnitType> Player1Team,
    List<BattleUnitType> Player2Team,
    List<Coordinate> Player1Origins,
    List<Coordinate> Player2Origins,
    List<Coordinate> RockPositions,
    List<BattleUnitStats> BaseStats);

public record Coordinate(int X, int Y);

// TODO: Remove from API
public static class MapStateMapper
{
    public static MapData ToState(this MapJson mapJson)
    {
        var tiles = new Array2d<TileState>(mapJson.RowCount, mapJson.ColumnCount);
        for (var i = 0; i < tiles.Data.Length; i++)
            tiles[i] = new();

        mapJson.RockPositions.ForEach(x => tiles[x.X, x.Y].Type = TerrainType.Rock);

        var state = new MapData
        {
            Tiles = tiles,
            Team1 = mapJson.Player1Team,
            Team2 = mapJson.Player2Team,
            Player1Origins = mapJson.Player1Origins.Select(x => new Point(x.X, x.Y)).ToList(),
            Player2Origins = mapJson.Player2Origins.Select(x => new Point(x.X, x.Y)).ToList(),
            BaseStats = mapJson.BaseStats
        };

        return state;
    }
}
