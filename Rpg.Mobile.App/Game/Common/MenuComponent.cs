using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Common;

public class MenuComponent : ComponentBase
{
    private List<ButtonComponent> _buttons = new();

    public float Spacing { get; set; } = 10f;
    public float ButtonHeight { get; set; } = 40f;
    public Color FillColor { get; set; } = Colors.Navy;
    public Color StrokeColor { get; set; } = Colors.Black;
    public float StrokeSize { get; set; } = 2f;
    public float CornerRadius { get; set; } = 2f;

    public MenuComponent(RectF bounds) : base(bounds)
    {
        IgnoreCamera = true;
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = FillColor;
        canvas.StrokeColor = StrokeColor;
        canvas.StrokeSize = StrokeSize;
        canvas.FillRoundedRectangle(0, 0, Bounds.Width, Bounds.Height, CornerRadius);
        canvas.DrawRoundedRectangle(0, 0, Bounds.Width, Bounds.Height, CornerRadius);
    }

    public void AddButton(string label, Action<IEnumerable<PointF>> onClick)
    {
        var bottomButton = _buttons.LastOrDefault();
        var button = new ButtonComponent(
            new(Spacing,
                bottomButton?.Bounds.Bottom + Spacing ?? Spacing,
                Bounds.Width - (Spacing * 2),
                ButtonHeight),
            label,
            onClick);

        AddChild(button);
        _buttons.Add(button);
    }

    public void SetButtons(params ButtonState[] buttons)
    {
        var currentButtons = _buttons.ToList();
        foreach (var button in currentButtons)
            ChildList.Remove(button);

        _buttons.Clear();
        foreach (var button in buttons)
            AddButton(button.Label, button.Handler);
    }
}

public record ButtonState(string Label, Action<IEnumerable<PointF>> Handler);
