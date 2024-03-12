using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rpg.Mobile.GameSdk;

public record SpliceOptions(
    string InputPath,
    string OutputPath,
    string FilePrefix = "Tile",
    int TileWidth = 16,
    int TileHeight = 16);

public class SpriteSplicer
{
    public void GenerateTiles(SpliceOptions options)
    {
        using var image = Image.Load<Rgba32>(options.InputPath);
        
        for (var y = 0; y < image.Height; y += options.TileHeight)
        {
            for (var x = 0; x < image.Width; x += options.TileWidth)
            {
                var tileX = x;
                var tileY = y;
                using var tile = image.Clone(ctx => ctx.Crop(new(tileX, tileY, options.TileWidth, options.TileHeight)));
                var tileName = $"{options.OutputPath}{Path.DirectorySeparatorChar}{options.FilePrefix}_{tileX:000}-{tileY:000}.png";
                tile.Save(tileName);
            }
        }
    }
}