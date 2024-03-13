using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Game.Battling.Domain;

public enum TerrainType
{
    Grass,
    Rock
}

public class TileState
{
    public TerrainType Type { get; set; } = TerrainType.Grass;
    public BattleUnitState? Unit { get; set; }
}

public class MapState
{
    public IImage GrassImage { get; set; }
    public IImage RockImage { get; set; }

    public TileState[,] Tiles { get; set; }
    public List<BattleUnitState> TurnOrder { get; set; } = new();

    public int RowCount => Tiles.GetLength(0);
    public int ColumnCount => Tiles.GetLength(1);

    public MapState(IImage grassImage, IImage rockImage, TileState[,] tiles)
    {
        GrassImage = grassImage;
        RockImage = rockImage;
        Tiles = tiles;
    }

    public static MapState New(int rows, int columns)
    {
        var state = new MapState(Sprites.Images.Grass03, Sprites.Images.Rock01, new TileState[rows, columns]);
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                state.Tiles[row, col] = new();
            }
        }

        return state;
    }
}

// TODO: Figure out a better data structure.
public static class ArrayExtensions
{
    public static IEnumerable<T> Flatten<T>(this T[,] map)
    {
        for (var row = 0; row < map.GetLength(0); row++)
        {
            for (var col = 0; col < map.GetLength(1); col++)
            {
                yield return map[row, col];
            }
        }
    }
}