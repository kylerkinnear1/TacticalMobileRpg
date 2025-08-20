using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class MapComponent : ComponentBase
{
    public const int TileWidth = 64;
    public MapData State { get; set; }

    public IImage GrassImage { get; set; } = Sprites.Images.Grass03;
    public IImage RockImage { get; set; } = Sprites.Images.Rock01;

    private readonly GridComponent _grid;

    public MapComponent(MapData state) 
        : base(new(0, 0, state.Width() * TileWidth, state.Height() * TileWidth))
    {
        State = state;
        AddChild(_grid = new(state.Width(), state.Height(), TileWidth));
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        State.Tiles.Each((x, y) =>
        {
            var image = State.Tiles[x, y].Type == TerrainType.Rock ? RockImage : GrassImage;
            canvas.DrawImage(image, x * TileWidth, y * TileWidth, TileWidth, TileWidth);
        });
    }
}