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

    public void ApplyHalfAcceleration()
    {
        Velocity += Acceleration * (Globals.SimulationStepTimePs / 2f);
    }

    public void ResetAcceleration()
    {
        Acceleration = Vector3.Zero;
    }

    // Opdaterer partiklens position og håndterer kollision med væggene, returnerer magnituden af hastighedsændringen som følge af væg-kollisioner.
    public float Update(Vector3 boxSize)
    {
        Position += Velocity * Globals.SimulationStepTimePs;

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

static class LennardJonesPotential
{
    // Beregn kraften på p1 fra p2 ved hjælp af Lennard-Jones potentialet (også den negative kraft på p2)
    // Kraften svarer til den negative hældning af den potentielle energi. Dvs. F = -U'(r)
    // Det giver følgende formel: F(r) = (24 * epsilon) / r * (2 * (sigma / r)^12 - (sigma / r)^6)
    // Det er bare kraftens størrelse. For at få kraften som en vektor, skal man gange med enhedsvektoren mellem de to partikler.
    // Den fås ved (p1 - p2) / |p1 - p2| hvor |p1 - p2| også er r.
    // Sammensat bliver kraften derfor: F = (p1 - p2) / r * (24 * epsilon) / r * (2 * (sigma / r)^12 - (sigma / r)^6)
    // Som kan forenkles til: F = (p1 - p2) * (24 * epsilon) / r^2 * (2 * (sigma / r)^12 - (sigma / r)^6)
    // Hvilket er smart fordi det er billigere at beregne r^2 end r.
    public static Vector3 CalculateForce(Vector3 p1, Vector3 p2, float sigma2, float epsilon)
    {
        Vector3 difference = p1 - p2;
        float distance2 = difference.LengthSquared(); // Beregn afstanden i anden mellem de to partikler (r^2). Afstanden kaldes ofte r i formlen for Lennard-Jones potentialet.

        float sigmaOverR2 = sigma2 / distance2;

        float sigmaOverR6 = sigmaOverR2 * sigmaOverR2 * sigmaOverR2;
        float sigmaOverR12 = sigmaOverR6 * sigmaOverR6;

        Vector3 force = difference * ((24 * epsilon) / distance2 * (2 * sigmaOverR12 - sigmaOverR6));

        return force;
    }

    // Beregn den potentielle energi mellem to partikler ved hjælp af Lennard-Jones potentialet
    // Formlen for Lennard-Jones potentialet er: U(r) = 4 * epsilon * ((sigma / r)^12 - (sigma / r)^6)
    public static float CalculatePotentialEnergy(Vector3 p1, Vector3 p2, float sigma2, float epsilon)
    {
        float distance2 = Vector3.DistanceSquared(p1, p2); // Beregn afstanden i anden mellem de to partikler (r^2). Afstanden kaldes ofte r i formlen for Lennard-Jones potentialet. 

        return CalculatePotentialEnergy(distance2, sigma2, epsilon);
    }
    public static float CalculatePotentialEnergy(float distance2, float sigma2, float epsilon)
    {
        float sigmaOverR2 = sigma2 / distance2;

        float sigmaOverR6 = sigmaOverR2 * sigmaOverR2 * sigmaOverR2;
        float sigmaOverR12 = sigmaOverR6 * sigmaOverR6;

        float potentialEnergy = 4 * epsilon * (sigmaOverR12 - sigmaOverR6);

        return potentialEnergy;
    }

    public static float CalculatePotentialEnergyWithCutoff(Vector3 p1, Vector3 p2, float sigma2, float epsilon, float cutoff2)
    {
        float offset = CalculatePotentialEnergy(cutoff2, sigma2, epsilon);
        return CalculatePotentialEnergyWithCutoff(p1, p2, sigma2, epsilon, cutoff2, offset);
    }
    public static float CalculatePotentialEnergyWithCutoff(Vector3 p1, Vector3 p2, float sigma2, float epsilon, float cutoff2, float offset)
    {
        float distance2 = Vector3.DistanceSquared(p1, p2); // Beregn afstanden i anden mellem de to partikler (r^2). Afstanden kaldes ofte r i formlen for Lennard-Jones potentialet.

        return CalculatePotentialEnergyWithCutoff(distance2, sigma2, epsilon, cutoff2, offset);
    }
    public static float CalculatePotentialEnergyWithCutoff(float distance2, float sigma2, float epsilon, float cutoff2, float offset)
    {
        if (distance2 > cutoff2) return 0f;

        float sigmaOverR2 = sigma2 / distance2;

        float sigmaOverR6 = sigmaOverR2 * sigmaOverR2 * sigmaOverR2;
        float sigmaOverR12 = sigmaOverR6 * sigmaOverR6;

        float potentialEnergy = 4 * epsilon * (sigmaOverR12 - sigmaOverR6);

        return potentialEnergy - offset;
    }

    public static float GetCutoffDistance(float sigma, float scalar)
    {
        return sigma * scalar;
    }

    public static float GetCutoffDistance2(float sigma, float scalar)
    {
        float s = GetCutoffDistance(sigma, scalar);
        return s * s;
    }
}
