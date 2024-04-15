using Rpg.Mobile.App.Game.Battling.Backend;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public const int TileSize = 64;
    public MapState State { get; set; }

    private readonly GridComponent _grid;

    public MapComponent(MapState state) 
        : base(new(0, 0, state.ColumnCount * TileSize, state.RowCount * TileSize))
    {
        State = state;
        AddChild(_grid = new(state.ColumnCount, state.RowCount, TileSize));
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        for (var row = 0; row < State.Tiles.GetLength(0); row++)
        {
            for (var col = 0; col < State.Tiles.GetLength(1); col++)
            {
                var tile = State.Tiles[row, col];
                var image = tile.Type == TerrainType.Rock ? State.RockImage : State.GrassImage;
                canvas.DrawImage(image, row * TileSize, col * TileSize, TileSize, TileSize);
            }
        }
    }
}