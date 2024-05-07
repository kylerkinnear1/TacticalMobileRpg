using Rpg.Mobile.App.Game.Battling.Gamemaster;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class TargetIndicatorComponent : ComponentBase
{
    private readonly IPathCalculator _path;

    public int Range { get; set; } = 1;
    public Point Center { get; set; } = Point.Empty;
    public int TileSize { get; set; }
    public float StrokeWidth { get; set; } = 2f;
    public Color StrokeColor { get; set; } = Colors.White;

    private readonly List<PointF> _vertices = new();

    public TargetIndicatorComponent(int tileSize, RectF bounds, IPathCalculator path) : base(bounds)
    {
        _path = path;
        TileSize = tileSize;
    }

    public override void Update(float deltaTime)
    {
        _vertices.Set(CalculateVertices());
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = StrokeColor;
        canvas.StrokeSize = StrokeWidth;
        foreach (var (v1, v2) in _vertices.Zip(_vertices.Skip(1).Append(_vertices[0])))
        {
            canvas.DrawLine(v1, v2);
        }
    }

    private IEnumerable<PointF> CalculateVertices()
    {
        if (Range != 1)
            throw new Exception("Start simple.");

        var centerSquare = new RectF(Center.X * TileSize, Center.Y * TileSize, TileSize, TileSize);
        yield return new(centerSquare.Left, centerSquare.Top);
        yield return new(centerSquare.Right, centerSquare.Top);
        yield return new(centerSquare.Right, centerSquare.Bottom);
        yield return new(centerSquare.Left, centerSquare.Bottom);
    }
}
