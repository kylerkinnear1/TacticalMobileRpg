using System.Runtime.InteropServices;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Windows;

public interface IWhereMouse
{
    PointI GetScreenMousePosition();
    PointI GetRelativeClientPosition();
}

// TODO: Get Windows specific out of library.
public class MouseWindowsUser32 : IWhereMouse
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out PointI lpPointI);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref PointI lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern int GetClientRect(IntPtr hWnd, out RECT rect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public PointI GetScreenMousePosition()
    {
        GetCursorPos(out var lpPoint);
        return lpPoint;
    }

    public PointI GetRelativeClientPosition()
    {
        var screenPoint = GetScreenMousePosition();
        var hWnd = GetForegroundWindow();

        var clientPoint = GetScreenMousePosition();
        return ScreenToClient(hWnd, ref clientPoint) ? clientPoint : screenPoint;
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