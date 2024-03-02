using Rpg.Mobile.GameSdk;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Game;

public class Sprites
{
    public static readonly Sprites Images = new(new EmbeddedResourceImageLoader(new(typeof(Sprites).Assembly)));

    public IImage ArcherIdle01 { get; }
    public IImage ArcherIdle02 { get; }
    public IImage HealerIdle01 { get; }
    public IImage HealerIdle02 { get; }
    public IImage MageIdle01 { get; }
    public IImage MageIdle02 { get; }
    public IImage NinjaIdle01 { get; }
    public IImage NinjaIdle02 { get; }
    public IImage WarriorIdle01 { get; }
    public IImage WarriorIdle02 { get; }

    public Sprites(IImageLoader loader)
    {
        ArcherIdle01 = loader.Load("ArcherIdle01.png");
        ArcherIdle02= loader.Load("ArcherIdle02.png");
        HealerIdle01 = loader.Load("HealerIdle01.png");
        HealerIdle02 = loader.Load("HealerIdle02.png");
        MageIdle01 = loader.Load("MageIdle01.png");
        MageIdle02 = loader.Load("MageIdle02.png");
        NinjaIdle01 = loader.Load("NinjaIdle01.png");
        NinjaIdle02 = loader.Load("NinjaIdle02.png");
        WarriorIdle01 = loader.Load("WarriorIdle01.png");
        WarriorIdle02 = loader.Load("WarriorIdle02.png");
    }
}
