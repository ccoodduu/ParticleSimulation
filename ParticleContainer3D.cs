using System.Numerics;

namespace ParticleSimulation;

class ParticleContainer3D
{
    private readonly Particle3D[] particles;
    public IReadOnlyList<Particle3D> Particles => particles;

    public Vector3 BoxSize { get; private set; }
    public readonly Random random;

    public ParticleParameters parameters;

    private float LennardJonesCutoff2 => LennardJonesPotential.GetCutoffDistance2(parameters.Sigma, 3);
    public ParticleContainer3D(int numParticles, Vector3 worldSize, ParticleParameters parameters)
    {
        random = new Random();

        BoxSize = worldSize;
        this.parameters = parameters;

        particles = new Particle3D[numParticles];
        const float radius = 0.5f;
        var spawnArea = BoxSize - new Vector3(radius * 2);

        int x = 0;
        int y = 0;
        int z = 0;
        int particlesPerRow = (int)MathF.Ceiling(MathF.Cbrt(numParticles));
        Vector3 stepDistance = spawnArea / new Vector3(particlesPerRow);
        for (int i = 0; i < numParticles; i++)
        {
            var pos = new Vector3(radius) + new Vector3(x, y, z) * stepDistance;
            var velocity = random.NextUnitVector3() * 3.5f;
            particles[i] = new Particle3D(pos, velocity);

            x++;
            if (x >= particlesPerRow)
            {
                x = 0;
                y++;
                if (y >= particlesPerRow)
                {
                    y = 0;
                    z++;
                }
            }
        }
    }

    public float GetTotalKineticEnergy()
    {
        float totalKineticEnergy = 0f;
        for (int i = 0; i < particles.Length; i++)
        {
            var speedSquared = particles[i].Velocity.LengthSquared();
            totalKineticEnergy += 0.5f * parameters.Mass * speedSquared;
        }

        return totalKineticEnergy;
    }

    public float GetTotalPotentialEnergy()
    {
        float cutoff2 = LennardJonesCutoff2;
        float totalPotentialEnergy = 0f;
        for (int i = 0; i < particles.Length; i++)
        {
            for (int j = i + 1; j < particles.Length; j++)
            {
                totalPotentialEnergy += LennardJonesPotential.CalculatePotentialEnergyWithCutoff(particles[i].Position, particles[j].Position, parameters.Sigma, parameters.Epsilon, cutoff2);
            }
        }
        return totalPotentialEnergy;
    }

    public float GetTemperature()
    {
        float totalKineticEnergy = GetTotalKineticEnergy();
        float temperature = 1.5f * (totalKineticEnergy / particles.Length) * 0.8314456946f;
        return temperature;
    }

    public void Update()
    {
        float cutoff2 = LennardJonesCutoff2;

        // Loop igennem alle par af partikler og beregn kraften mellem dem
        for (int i = 0; i < particles.Length; i++)
        {
            for (int j = i + 1; j < particles.Length; j++)
            {
                if (Vector3.DistanceSquared(particles[i].Position, particles[j].Position) > cutoff2)
                {
                    continue; // Spring over hvis partiklerne er langt fra hinanden, da det giver en kraft der nærmest er 0
                }

                var force = LennardJonesPotential.CalculateForce(particles[i].Position, particles[j].Position, parameters.Sigma, parameters.Epsilon);
                particles[i].ApplyForce(force, parameters.Mass);
                particles[j].ApplyForce(-force, parameters.Mass);
            }
        }

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Update(BoxSize, parameters.Radius);
        }
    }
}

public struct ParticleParameters
{
    public float Radius; // Kun visuelt. Partiklens radius i angstoms (Å)
    public float Mass; // Partiklens masse i units (u)
    public float Sigma; // Partiklens sigma i angstoms (Å)
    public float Epsilon; // Partiklens epsilon i enheder af energi (u*Å^2/ps^2)

    public ParticleParameters()
    {
        Radius = 0.5f;
        Mass = 1f;
        Sigma = 1f;
        Epsilon = 1f;
    }

    public static ParticleParameters Default => new();

    // TODO: Skriv kilde på disse værdier.
    public static ParticleParameters Xenon => new()
    {
        Radius = 4.10f / 2f,
        Mass = 131.29f,
        Sigma = 4.10f,
        Epsilon = 177f
    };
    public static ParticleParameters Argon => new()
    {
        Radius = 3.405f / 2f,
        Mass = 39.948f,
        Sigma = 3.405f,
        Epsilon = 99.6f
    };

}
