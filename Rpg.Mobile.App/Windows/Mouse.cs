using System.Runtime.InteropServices;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Windows;

public class MouseWindowsUser32 : IMouse
{
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out PointI lpPointI);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref PointI lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

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
}

