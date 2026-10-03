using System.Numerics;

namespace ParticleSimulation;

class ParticleContainer3D
{
    private readonly Particle3D[] particles;
    public IReadOnlyList<Particle3D> Particles => particles;

    public Vector3 BoxSize { get; private set; }
    public readonly Random random;

    public ParticleParameters parameters;

    // Linked list til at holde styr på hvilke partikler der er i hvilke celler i gitteret, 
    private readonly int[] gridNext; // Et entry for hver partikel, der holder indekset til den næste partikel i samme celle. Hvis der ikke er nogen næste partikel, er værdien -1.
    private readonly int[] gridHead; // Flat array, et entry for hver celle i gitteret, der holder indekset til den første partikel i cellen.

    private readonly int gridCellsX, gridCellsY, gridCellsZ;

    private readonly Vector3 cellSize; // Størrelsen af hver celle i gitteret

    private float LennardJonesCutoff2 => LennardJonesPotential.GetCutoffDistance2(parameters.Sigma, 3);
    private float LennardJonesCutoff => LennardJonesPotential.GetCutoffDistance(parameters.Sigma, 3);
    public ParticleContainer3D(int numParticles, Vector3 worldSize, ParticleParameters parameters)
    {
        random = new Random();

        BoxSize = worldSize;
        this.parameters = parameters;

        particles = new Particle3D[numParticles];

        float minCellSize = LennardJonesCutoff;
        gridCellsX = (int)MathF.Max(BoxSize.X / minCellSize, 1);
        gridCellsY = (int)MathF.Max(BoxSize.Y / minCellSize, 1);
        gridCellsZ = (int)MathF.Max(BoxSize.Z / minCellSize, 1);
        cellSize = BoxSize / new Vector3(gridCellsX, gridCellsY, gridCellsZ);

        gridHead = new int[gridCellsX * gridCellsY * gridCellsZ].Populate(-1);
        gridNext = new int[numParticles].Populate(-1);

        // Spawn partiklerne i midten af hver celle i et 3D gitter, så de ikke overlapper hinanden og har lige stor afstand til alle vægge
        int x = 0;
        int y = 0;
        int z = 0;
        int particlesPerRow = (int)MathF.Ceiling(MathF.Cbrt(numParticles));
        Vector3 stepDistance = BoxSize / new Vector3(particlesPerRow);
        for (int i = 0; i < numParticles; i++)
        {
            var pos = (new Vector3(x, y, z) + new Vector3(0.5f)) * stepDistance;
            var velocity = random.NextUnitVector3() * 3.5f;
            particles[i] = new Particle3D(pos, velocity);

            var gridIndex = GetGridIndex(pos); // Find cellen som partikel i befinder sig i
            var currentHead = gridHead[gridIndex]; // Gem den nuværende første partikel i cellen
            gridHead[gridIndex] = i; // Sæt partikel i som den nye første partikel i cellen
            gridNext[i] = currentHead; // Sæt partiklen til at pege på den tidligere første partikel i cellen

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

    private int GetGridIndex(Vector3 position)
    {
        int x = Math.Clamp((int)(position.X / cellSize.X), 0, gridCellsX - 1);
        int y = Math.Clamp((int)(position.Y / cellSize.Y), 0, gridCellsY - 1);
        int z = Math.Clamp((int)(position.Z / cellSize.Z), 0, gridCellsZ - 1);
        return x + y * gridCellsX + z * gridCellsX * gridCellsY;
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
        float sigma2 = parameters.Sigma * parameters.Sigma;

        float offset = LennardJonesPotential.CalculatePotentialEnergy(cutoff2, sigma2, parameters.Epsilon);

        float totalPotentialEnergy = 0f;
        ForEachNeighborPair((i, j, d2) =>
        {
            totalPotentialEnergy += LennardJonesPotential.CalculatePotentialEnergyWithCutoff(d2, sigma2, parameters.Epsilon, cutoff2, offset);
        });

        return totalPotentialEnergy;
    }

    public float GetTemperature()
    {
        float totalKineticEnergy = GetTotalKineticEnergy();
        return GetTemperature(totalKineticEnergy);
    }
    public float GetTemperature(float totalKineticEnergy)
    {
        float temperature = 1.5f * (totalKineticEnergy / particles.Length) * 0.8314456946f;
        return temperature;
    }

    // Udfører funktionen `action` på hvert par af partikler, der er indenfor LennardJonesCutoff i gitteret.
    // Dette bruges til at beregne kræfter og potentielle energier mellem partiklerne.
    private void ForEachNeighborPair(Action<int, int, float> action)
    {
        float cutoff2 = LennardJonesCutoff2;

        for (int i = 0; i < particles.Length; i++)
        {
            var gridIndex = GetGridIndex(particles[i].Position); // Find cellen som partikel i befinder sig i
            int x = gridIndex % gridCellsX;
            int y = (gridIndex / gridCellsX) % gridCellsY;
            int z = gridIndex / (gridCellsX * gridCellsY);

            for (int zo = -1; zo <= 1; zo++)
            {
                int neighborZ = z + zo;
                if (neighborZ < 0 || neighborZ >= gridCellsZ) continue; // Skip celler udenfor gitteret

                for (int yo = -1; yo <= 1; yo++)
                {
                    int neighborY = y + yo;
                    if (neighborY < 0 || neighborY >= gridCellsY) continue; // Skip celler udenfor gitteret

                    for (int xo = -1; xo <= 1; xo++)
                    {
                        int neighborX = x + xo;

                        if (neighborX < 0 || neighborX >= gridCellsX) continue; // Skip celler udenfor gitteret

                        int currentNeighborIndex = neighborX + neighborY * gridCellsX + neighborZ * gridCellsX * gridCellsY;
                        int currentNeighborParticleIndex = gridHead[currentNeighborIndex];
                        while (currentNeighborParticleIndex != -1)
                        {
                            int j = currentNeighborParticleIndex;
                            currentNeighborParticleIndex = gridNext[currentNeighborParticleIndex];

                            if (j <= i) continue; // Skip sig selv eller partikler der allerede er blevet behandlet for at undgå dobbelttælling

                            float d2 = Vector3.DistanceSquared(particles[i].Position, particles[j].Position);
                            if (d2 > cutoff2)
                            {
                                continue; // Spring over hvis partiklerne er langt fra hinanden, da det giver en kraft der nærmest er 0
                            }

                            action(i, j, d2); // Udfør handlingen på partikelparret
                        }
                    }
                }
            }
        }
    }

    public void Update()
    {
        float sigma2 = parameters.Sigma * parameters.Sigma;

        for (int i = 0; i < particles.Length; i++)
        {
            int gridIndex = GetGridIndex(particles[i].Position);
            gridHead[gridIndex] = -1; // Reset cellen, dette resetter alle celler med partikler i, alle andre er allerede sat til -1, så det er ikke nødvendigt at sætte dem igen
        }

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].ApplyHalfAcceleration(parameters.Mass); // Velocity verlet integration
            particles[i].Update(BoxSize); // Flytter partiklerne og håndterer kollisioner med væggene
            particles[i].ResetAcceleration(); // Nulstil acceleration efter opdatering, så den ikke akkumuleres over tid

            int gridIndex = GetGridIndex(particles[i].Position); // Find cellen som partikel i befinder sig i efter opdateringen
            int currentHead = gridHead[gridIndex]; // Gem den nuværende første partikel i cellen
            gridHead[gridIndex] = i; // Sæt partikel i som den nye første partikel i cellen
            gridNext[i] = currentHead; // Sæt partiklen til at pege på den tidligere første partikel i cellen
        }

        // Loop igennem alle par af partikler og beregn kraften mellem dem
        ForEachNeighborPair((i, j, d2) =>
        {
            var force = LennardJonesPotential.CalculateForce(particles[i].Position, particles[j].Position, sigma2, parameters.Epsilon);
            particles[i].ApplyForce(force, parameters.Mass);
            particles[j].ApplyForce(-force, parameters.Mass);
        });

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].ApplyHalfAcceleration(parameters.Mass); // Velocity verlet integration
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
