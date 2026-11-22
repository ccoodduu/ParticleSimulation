using System.Numerics;

namespace ParticleSimulation;

public static class Extensions
{
    // Tilfældig retning, jævnt fordelt over enhedskuglen.
    // Kilde: Weisstein, E. W. "Sphere Point Picking", MathWorld, ligning (6)-(8). https://mathworld.wolfram.com/SpherePointPicking.html
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
