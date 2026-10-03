using System.Numerics;

namespace ParticleSimulation;

public readonly struct Vector2Int(int x, int y)
{
    public readonly int X = x;
    public readonly int Y = y;

    public Vector2 ToVector2() => new(X, Y);
    public float Length() => MathF.Sqrt(X * X + Y * Y);

    public static Vector2Int operator +(Vector2Int left, Vector2Int right) => new(left.X + right.X, left.Y + right.Y);
    public static Vector2Int operator -(Vector2Int left, Vector2Int right) => new(left.X - right.X, left.Y - right.Y);
    public static Vector2Int operator *(Vector2Int left, Vector2Int right) => new(left.X * right.X, left.Y * right.Y);
    public static Vector2Int operator /(Vector2Int left, Vector2Int right) => new(left.X / right.X, left.Y / right.Y);

}

public static class Extensions
{
    public static Vector2Int RoundToVector2Int(this Vector2 vector) => new((int)MathF.Round(vector.X), (int)MathF.Round(vector.Y));
    public static Vector2 NextUnitVector(this Random random)
    {
        var angle = (float)random.NextDouble() * MathF.Tau;
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }
    public static Vector3 NextUnitVector3(this Random random)
    {
        var angle = (float)random.NextDouble() * MathF.Tau;
        var z = (float)random.NextDouble() * 2 - 1;
        var r = MathF.Sqrt(1 - z * z);
        return new Vector3(r * MathF.Cos(angle), r * MathF.Sin(angle), z);
    }

    public static T[] Populate<T>(this T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
        return arr;
    }
}
