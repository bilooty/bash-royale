using System;
using System.IO;

namespace faitingu.scripts;

public record struct Vector2Int(int X, int Y)
{
    public static readonly Vector2Int Zero  = new(0, 0);
    public static readonly Vector2Int One   = new(1, 1);
    public static readonly Vector2Int Left  = new(-1, 0);
    public static readonly Vector2Int Right = new(1, 0);
    public static readonly Vector2Int Up    = new(0, -1);
    public static readonly Vector2Int Down  = new(0, 1);

    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2Int operator *(Vector2Int a, int scalar)   => new(a.X * scalar, a.Y * scalar);
    public static Vector2Int operator *(int scalar, Vector2Int a)   => new(a.X * scalar, a.Y * scalar);
    public static Vector2Int operator /(Vector2Int a, int scalar)   => new(a.X / scalar, a.Y / scalar);
    public static Vector2Int operator -(Vector2Int a)               => new(-a.X, -a.Y);

    public int ManhattanLength => Math.Abs(X) + Math.Abs(Y);
    public long LengthSquared  => (long)X * X + (long)Y * Y;
    

    public override string ToString() => $"({X}, {Y})";

    public double DistanceTo(Vector2Int other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
   
}