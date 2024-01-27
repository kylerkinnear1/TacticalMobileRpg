namespace Rpg.Mobile.App;

internal class GameSceneView : GraphicsView
{
    public GameSceneView()
    {
        BackgroundColor = Colors.ForestGreen;
        Drawable = new TestDraw();
        Invalidate();
    }
}

public class TestDraw : IDrawable
{
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.Red;
        canvas.DrawLine(dirtyRect.Left, dirtyRect.Top, dirtyRect.Right, dirtyRect.Bottom);
    }
}
