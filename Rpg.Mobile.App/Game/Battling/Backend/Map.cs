using Rpg.Mobile.GameSdk;


namespace Rpg.Mobile.App.Game.Battling.Backend;

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
    public IImage GrassImage { get; set; }
    public IImage RockImage { get; set; }

    public Array2d<TileState> Tiles { get; }

    public List<BattleUnitState> TurnOrder { get; set; } = new();
    public Dictionary<BattleUnitState, Point> UnitCoordinates { get; set; } = new();

    public int Width => Tiles.Width;
    public int Height => Tiles.Height;

    public MapState(IImage grassImage, IImage rockImage, Array2d<TileState> tiles)
    {
        GrassImage = grassImage;
        RockImage = rockImage;
        Tiles = tiles;
    }

    public static MapState New(int width, int height)
    {
        var tiles = new Array2d<TileState>(width, height);
        for (var i = 0; i < tiles.Data.Length; i++)
            tiles[i] = new();
        
        var state = new MapState(Sprites.Images.Grass03, Sprites.Images.Rock01, tiles);
        return state;
    }
}
