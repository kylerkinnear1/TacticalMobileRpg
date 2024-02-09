﻿using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Extensions;

public static class FloatExtensions
{
    public static float Mod(this float value, float mod, float tolerance = .1f)
    {
        throw new NotImplementedException();
    }

    public static float Equal(this float a, float b, float tolerance = .1f)
    {
        throw new NotImplementedException();
    }
}

public static class DrawExtensions
{
    public static void Draw(this ICanvas canvas, IImage image, PointF pos, float scale = 1f) => 
        canvas.DrawImage(image, pos.X, pos.Y, image.Width * scale, image.Height * scale);
}
