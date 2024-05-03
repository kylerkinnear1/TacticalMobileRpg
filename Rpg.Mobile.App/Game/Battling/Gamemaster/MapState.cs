using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Gamemaster;

public enum TerrainType
{
    Grass,
    Rock
}

public class TileState
{
    public TerrainType Type { get; set; } = TerrainType.Grass;
}

public class MapState
{
    public Array2d<TileState> Tiles { get; set; }
    public List<Point> Player1Origins { get; set; }
    public List<Point> Player2Origins { get; set; }

    public int Width => Tiles.Width;
    public int Height => Tiles.Height;

    public MapState(
        Array2d<TileState> tiles,
        List<Point> player1Origins,
        List<Point> player2Origins)
    {
        Tiles = tiles;
        Player1Origins = player1Origins;
        Player2Origins = player2Origins;
    }
}
