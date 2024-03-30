using System.Runtime.InteropServices;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Windows;

public interface IWhereMouse
{
    PointI GetScreenMousePosition();
    PointI GetScreenMousePosition(Window? window);
    PointI GetScreenMousePosition(VisualElement? element);
}

// TODO: Get Windows specific out of library.
public class MouseWindowsUser32 : IWhereMouse
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out PointI lpPointI);

    public PointI GetScreenMousePosition()
    {
        GetCursorPos(out var lpPoint);
        return lpPoint;
    }

    public PointI GetScreenMousePosition(Window? window)
    {
        if (window is null)
            return PointI.Zero;

        var screenPosition = GetScreenMousePosition();
        var relativeX = screenPosition.X - window.X;
        var relativeY = screenPosition.Y - window.Y;
        return new PointI { X = relativeX.RoundToInt(), Y = relativeY.RoundToInt() };
    }

    // Found and tried this, but the performance is awful.
    public PointI GetScreenMousePosition(VisualElement? element)
    {
        var position = GetAncestors(element)
            .Aggregate(
                new Point(0, 0),
                (last, element) => new Point(last.X + element.X, last.Y + element.Y));

        return new PointI { X = position.X.RoundToInt(), Y = position.Y.RoundToInt() };
    }

    private static IEnumerable<VisualElement> GetAncestors(VisualElement element)
    {
        var visualParent = element;
        while (visualParent != null)
        {
            yield return element;
            visualParent = element.Parent as VisualElement;
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct PointI
{
    public int X;
    public int Y;

    public static readonly PointI Zero;
}
