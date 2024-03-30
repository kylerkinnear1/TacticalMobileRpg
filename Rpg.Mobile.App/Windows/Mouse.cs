using System.Runtime.InteropServices;

namespace Rpg.Mobile.App.Windows;

public interface IWhereMouse
{
    PointI GetScreenMousePosition();
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
}

[StructLayout(LayoutKind.Sequential)]
public struct PointI
{
    public int X;
    public int Y;
}
