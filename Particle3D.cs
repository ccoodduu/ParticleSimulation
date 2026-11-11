using System.Numerics;

namespace ParticleSimulation;

struct Particle3D(Vector3 position, Vector3 velocity)
{
    public Vector3 Position { get; private set; } = position;
    public Vector3 Velocity { get; private set; } = velocity;

    public Vector3 Acceleration { get; private set; } = Vector3.Zero;

    public void ApplyForce(Vector3 force, float mass)
    {
        Acceleration += force / mass;
    }

    public void ApplyHalfAcceleration(float dt)
    {
        Velocity += Acceleration * (dt / 2f);
    }

    public void ResetAcceleration()
    {
        Acceleration = Vector3.Zero;
    }

    // Flytter partiklen og sætter den ind på den modsatte side, hvis den forlader kassen
    public void Update(Vector3 boxSize, float dt)
    {
        var position = Position + Velocity * dt;
        position.X = Wrap(position.X, boxSize.X);
        position.Y = Wrap(position.Y, boxSize.Y);
        position.Z = Wrap(position.Z, boxSize.Z);
        Position = position;
    }

    private static float Wrap(float coordinate, float boxLength)
    {
        if (coordinate < 0) return coordinate + boxLength;
        else if (coordinate >= boxLength) return coordinate - boxLength;
        else return coordinate;
    }
}
