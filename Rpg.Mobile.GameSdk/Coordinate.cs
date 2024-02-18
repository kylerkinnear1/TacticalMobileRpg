namespace Rpg.Mobile.GameSdk;

public record Coordinate(int X, int Y)
{
    public static readonly Coordinate Zero = new(0, 0);
    public static Coordinate operator +(Coordinate a) => a;
    public static Coordinate operator -(Coordinate a) => new(-a.X, a.Y);
    public static Coordinate operator +(Coordinate a, Coordinate b) => new(a.X + b.X, a.Y + b.Y);
    public static Coordinate operator -(Coordinate a, Coordinate b) => new(a.X - b.X, a.Y - b.Y);
    public static Coordinate operator *(Coordinate a, Coordinate b) => new(a.X * b.X, a.Y * b.Y);
    public static Coordinate operator /(Coordinate a, Coordinate b) => new(a.X / a.Y, a.Y / b.Y);
}

public record CoordinateF(float X, float Y)
{
    public static readonly CoordinateF Zero = new(0f, 0f);
    public static CoordinateF operator +(CoordinateF a) => a;
    public static CoordinateF operator -(CoordinateF a) => new(-a.X, a.Y);
    public static CoordinateF operator +(CoordinateF a, CoordinateF b) => new(a.X + b.X, a.Y + b.Y);
    public static CoordinateF operator -(CoordinateF a, CoordinateF b) => new(a.X - b.X, a.Y - b.Y);
    public static CoordinateF operator *(CoordinateF a, CoordinateF b) => new(a.X * b.X, a.Y * b.Y);
    public static CoordinateF operator /(CoordinateF a, CoordinateF b) => new(a.X / a.Y, a.Y / b.Y);
}
