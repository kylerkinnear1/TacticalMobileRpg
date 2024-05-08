using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Common;

public class MenuComponent : ComponentBase
{
    public List<ButtonComponent> Buttons = new();

    public float Spacing { get; set; } = 20f;
    public float ButtonHeight { get; set; } = 40f;
    public Color FillColor { get; set; } = Colors.Navy;
    public Color StrokeColor { get; set; } = Colors.GhostWhite;
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
        var bottomButton = Buttons.LastOrDefault();
        var button = new ButtonComponent(
            new(Spacing,
                bottomButton?.Bounds.Bottom + Spacing ?? Spacing,
                Bounds.Width - (Spacing * 2),
                ButtonHeight),
            label,
            onClick);

        AddChild(button);
        Buttons.Add(button);
    }

    public void SetButtons(params ButtonState[] buttons)
    {
        var currentButtons = Buttons.ToList();
        foreach (var button in currentButtons)
            ChildList.Remove(button);

        Buttons.Clear();
        foreach (var button in buttons)
            AddButton(button.Label, button.Handler);

        Bounds = new(Bounds.Location, new(Bounds.Width, Buttons.LastOrDefault()?.Bounds.Bottom + Spacing ?? Bounds.Bottom));
    }
}

public record ButtonState(string Label, Action<IEnumerable<PointF>> Handler);
