using Rpg.Mobile.App.Game.Battling.Backend;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public const int TileSize = 64;
    public MapState State { get; set; }

    private readonly GridComponent _grid;

    public MapComponent(MapState state) 
        : base(new(0, 0, state.Width * TileSize, state.Height * TileSize))
    {
        State = state;
        AddChild(_grid = new(state.Width, state.Height, TileSize));
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        State.Tiles.Each((x, y) =>
        {
            var image = State.Tiles[x, y].Type == TerrainType.Rock ? State.RockImage : State.GrassImage;
            canvas.DrawImage(image, y * TileSize, x * TileSize, TileSize, TileSize);
        });
    }
}