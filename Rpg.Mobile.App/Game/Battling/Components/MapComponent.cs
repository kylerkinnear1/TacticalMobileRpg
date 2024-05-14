using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public const int TileSize = 64;
    public MapState State { get; set; }

    public IImage GrassImage { get; set; } = Sprites.Images.Grass03;
    public IImage RockImage { get; set; } = Sprites.Images.Rock01;

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
            var image = State.Tiles[x, y].Type == TerrainType.Rock ? RockImage : GrassImage;
            canvas.DrawImage(image, x * TileSize, y * TileSize, TileSize, TileSize);
        });
    }
}