using System.Runtime.InteropServices;

namespace Rpg.Mobile.GameSdk.Inputs;

public interface IMouse
{
    PointI GetScreenMousePosition();
    PointI GetRelativeClientPosition();
}

[StructLayout(LayoutKind.Sequential)]
public struct PointI
{
    public int X;
    public int Y;

    public static readonly PointI Zero;
}
