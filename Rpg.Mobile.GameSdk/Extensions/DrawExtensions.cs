﻿using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Extensions;

public static class DrawExtensions
{
    public static void Draw(this ICanvas canvas, IImage image, float scale = 1f) =>
        canvas.DrawImage(image, 0, 0, image.Width * scale, image.Height * scale);

    public static void Draw(this ICanvas canvas, IImage image, PointF pos, float scale = 1f) => 
        canvas.DrawImage(image, pos.X, pos.Y, image.Width * scale, image.Height * scale);

    public static RectF GetBounds(this IImage image, float scale = 1f) =>
        new(0, 0, image.Width * scale, image.Height * scale);

    public static RectF Translate(this RectF bounds, PointF position) =>
        new(bounds.X + position.X, bounds.Y + position.Y, bounds.Width, bounds.Height);

    public static RectF Translate(this RectF bounds, float x, float y) =>
        new(bounds.X + x, bounds.Y + y, bounds.Width, bounds.Height);
}
