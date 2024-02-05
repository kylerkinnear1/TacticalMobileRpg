using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.States;

public class BattleUnit
{
    public PointF Location { get; set; }
    public IImage Sprite { get; set; }

    public BattleUnit(PointF location, IImage sprite)
    {
        Location = location;
        Sprite = sprite;
    }
}
