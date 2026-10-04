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

    // Opdaterer partiklens position og håndterer kollision med væggene, returnerer magnituden af hastighedsændringen som følge af væg-kollisioner.
    public float Update(Vector3 boxSize, float dt)
    {
        Position += Velocity * dt;

        return HandleBoundaryCollision(boxSize);
    }

    // Partiklerne er punktpartikler, så væggene ligger ved 0 og boxSize, returnere magnituden af hastighedsændringen som følge af kollisionerne.
    private float HandleBoundaryCollision(Vector3 boxSize)
    {
        float velocityChange = 0f;
        var newVelocity = Velocity;
        var newPosition = Position;

        if (Position.X < 0)
        {
            newVelocity.X *= -1;
            newPosition.X = -Position.X;
            velocityChange += -Velocity.X * 2;
        }
        if (Position.X > boxSize.X)
        {
            newVelocity.X *= -1;
            newPosition.X = 2 * boxSize.X - Position.X;
            velocityChange += Velocity.X * 2;
        }
        if (Position.Y < 0)
        {
            newVelocity.Y *= -1;
            newPosition.Y = -Position.Y;
            velocityChange += -Velocity.Y * 2;
        }
        if (Position.Y > boxSize.Y)
        {
            newVelocity.Y *= -1;
            newPosition.Y = 2 * boxSize.Y - Position.Y;
            velocityChange += Velocity.Y * 2;
        }
        if (Position.Z < 0)
        {
            newVelocity.Z *= -1;
            newPosition.Z = -Position.Z;
            velocityChange += -Velocity.Z * 2;
        }
        if (Position.Z > boxSize.Z)
        {
            newVelocity.Z *= -1;
            newPosition.Z = 2 * boxSize.Z - Position.Z;
            velocityChange += Velocity.Z * 2;
        }

        Velocity = newVelocity;
        Position = newPosition;

        return velocityChange;
    }
}
