﻿namespace Rpg.Mobile.App.Game.UserInterface;

public class ButtonComponent : TextboxComponent
{
    private readonly Action<IEnumerable<PointF>>[] _handlers;

    public ButtonComponent(RectF bounds, string label, params Action<IEnumerable<PointF>>[] handlers) : base(bounds, label)
    {
        Label = label;
        _handlers = handlers;

        BackColor = Colors.WhiteSmoke;
        TextColor = Colors.Black;
    }

    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        foreach (var handler in _handlers)
            handler(touches);
    }
}
