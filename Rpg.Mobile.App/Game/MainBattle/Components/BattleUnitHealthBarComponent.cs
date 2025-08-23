using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class BattleUnitHealthBarComponent(BattleUnitComponent.Data _data) 
    : ComponentBase(new(0f, 0f, 30f, 25f))
{
    public Font Font { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        var thisIndex = _data.BattleData.Active.TurnOrderIds.IndexOf(_data.BattleUnit.UnitId);
        var currentIndex = _data.BattleData.Active.ActiveUnitIndex;
        var hasGone = thisIndex < currentIndex;
        canvas.Font = Font;
        canvas.FontSize = hasGone ? 19f : 22f;

        canvas.FillColor = hasGone ? Colors.SlateGray.WithAlpha(.8f) : Colors.Black.WithAlpha(.4f);
        canvas.FontColor = _data.BattleUnit.PlayerId switch
        {
            0 when !hasGone => Colors.Aqua,
            1 when !hasGone => Colors.Orange,
            0 when hasGone => Colors.DarkBlue,
            1 when hasGone => Colors.Brown,
            _ => throw new ArgumentException()
        };

        canvas.StrokeColor = _data.BattleUnit.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.StrokeSize = 1f;

        var bounds = new RectF(PointF.Zero, Bounds.Size);
        canvas.FillRoundedRectangle(bounds, 2f);
        canvas.DrawString($"{_data.BattleUnit.RemainingHealth}", bounds, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}