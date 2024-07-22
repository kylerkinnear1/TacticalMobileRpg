using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.Data;

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
    public Array2d<TileState> Tiles { get; set; }
    public List<Point> Player1Origins { get; set; }
    public List<Point> Player2Origins { get; set; }
    public List<BattleUnitType> Team1 { get; set; }
    public List<BattleUnitType> Team2 { get; set; }
    public List<BattleUnitStats> BaseStats { get; set; }

    public int Width => Tiles.Width;
    public int Height => Tiles.Height;
    public Point Corner => new(Width, Height);

    public MapData(
        Array2d<TileState> tiles,
        List<BattleUnitType> team1,
        List<BattleUnitType> team2,
        List<Point> player1Origins,
        List<Point> player2Origins,
        List<BattleUnitStats> baseStats)
    {
        Tiles = tiles;
        Team1 = team1;
        Team2 = team2;
        Player1Origins = player1Origins;
        Player2Origins = player2Origins;
        BaseStats = baseStats;
    }
}


public record Coordinate(int X, int Y);
public record MapJson(
    int RowCount,
    int ColumnCount,
    List<BattleUnitType> Player1Team,
    List<BattleUnitType> Player2Team,
    List<Coordinate> Player1Origins,
    List<Coordinate> Player2Origins,
    List<Coordinate> RockPositions,
    List<BattleUnitStats> BaseStats);

public static class MapStateMapper
{
    public static MapData ToState(this MapJson mapJson)
    {
        var tiles = new Array2d<TileState>(mapJson.RowCount, mapJson.ColumnCount);
        for (var i = 0; i < tiles.Data.Length; i++)
            tiles[i] = new();

        mapJson.RockPositions.ForEach(x => tiles[x.X, x.Y].Type = TerrainType.Rock);

        var state = new MapData(
            tiles,
            mapJson.Player1Team,
            mapJson.Player2Team,
            mapJson.Player1Origins.Select(x => new Point(x.X, x.Y)).ToList(),
            mapJson.Player2Origins.Select(x => new Point(x.X, x.Y)).ToList(),
            mapJson.BaseStats);

        return state;
    }
}
