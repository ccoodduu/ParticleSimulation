using System.Numerics;

namespace ParticleSimulation;

class ParticleSimulation3D
{
    public readonly ParticleParameters ParticleParameters;

    public readonly SimulationParameters SimulationParameters;
    public int ParticleCount => SimulationParameters.ParticleCount;
    public Vector3 BoxSize => SimulationParameters.BoxSize;
    public float BoxVolume => BoxSize.X * BoxSize.Y * BoxSize.Z;
    public float Density => ParticleCount / BoxVolume; // Partikler pr. volumen

    private readonly Particle3D[] particles;
    public IReadOnlyList<Particle3D> Particles => particles;

    // Linked list til at holde styr på hvilke partikler der er i hvilke celler i gitteret, 
    private readonly int[] gridNext; // Et entry for hver partikel, der holder indekset til den næste partikel i samme celle. Hvis der ikke er nogen næste partikel, er værdien -1.
    private readonly int[] gridHead; // Flat array, et entry for hver celle i gitteret, der holder indekset til den første partikel i cellen.

    private readonly int gridCellsX, gridCellsY, gridCellsZ;

    private readonly Vector3 cellSize; // Størrelsen af hver celle i gitteret

    private float lastVirial;

    private float LennardJonesCutoff2 => LennardJones.GetCutoffDistance2(ParticleParameters.Sigma, SimulationParameters.LennardJonesCutoffMultiplier);
    private float LennardJonesCutoff => LennardJones.GetCutoffDistance(ParticleParameters.Sigma, SimulationParameters.LennardJonesCutoffMultiplier);
    public ParticleSimulation3D(SimulationParameters simulationParameters, ParticleParameters particleParameters)
    {
        var random = new Random();

        SimulationParameters = simulationParameters;
        ParticleParameters = particleParameters;

        particles = new Particle3D[ParticleCount];

        float minCellSize = LennardJonesCutoff;
        gridCellsX = (int)MathF.Max(BoxSize.X / minCellSize, 1);
        gridCellsY = (int)MathF.Max(BoxSize.Y / minCellSize, 1);
        gridCellsZ = (int)MathF.Max(BoxSize.Z / minCellSize, 1);
        cellSize = BoxSize / new Vector3(gridCellsX, gridCellsY, gridCellsZ);

        // Med færre end 3 celler ville den samme nabocelle blive besøgt flere gange, når gitteret wrapper rundt
        if (gridCellsX < 3 || gridCellsY < 3 || gridCellsZ < 3)
            throw new ArgumentException("Kassen skal være mindst 3 cutoff-afstande bred i hver retning.");

        gridHead = new int[gridCellsX * gridCellsY * gridCellsZ].Populate(-1);
        gridNext = new int[ParticleCount].Populate(-1);

        // Spawn partiklerne i midten af hver celle i et 3D gitter, så de ikke overlapper hinanden
        int x = 0;
        int y = 0;
        int z = 0;
        int particlesPerRow = (int)MathF.Ceiling(MathF.Cbrt(ParticleCount));
        Vector3 stepDistance = BoxSize / new Vector3(particlesPerRow);
        for (int i = 0; i < ParticleCount; i++)
        {
            var pos = (new Vector3(x, y, z) + new Vector3(0.5f)) * stepDistance;
            var velocity = random.NextUnitVector3() * simulationParameters.ParticleStartVelocity;
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
            totalKineticEnergy += 0.5f * ParticleParameters.Mass * speedSquared;
        }

        return totalKineticEnergy;
    }

    // Den totale potentielle energi af simuleringen. Den er for høj ift. virkeligheden, fordi den ikke tager højde for partikler udenfor cutoff-afstanden,
    // og energien er blevet offset for at den er 0 ved cutoff.
    // Dette sørger for energibevarelse i simuleringen, men giver ikke den rigtige værdi for den potentielle energi.
    public float GetTotalPotentialEnergy()
    {
        float cutoff2 = LennardJonesCutoff2;
        float sigma2 = ParticleParameters.Sigma * ParticleParameters.Sigma;

        float offset = LennardJones.CalculatePotentialEnergy(cutoff2, sigma2, ParticleParameters.Epsilon);

        float totalPotentialEnergy = 0f;
        ForEachNeighborPair((i, j, difference, d2) =>
        {
            totalPotentialEnergy += LennardJones.CalculatePotentialEnergyWithCutoff(d2, sigma2, ParticleParameters.Epsilon, cutoff2, offset);
        });

        return totalPotentialEnergy;
    }

    // Den korrigerede potentielle energi af simuleringen. Den tager højde for partikler udenfor cutoff-afstanden, og er ikke offset, og giver derfor en mere realistisk værdi for den potentielle energi.
    // Hvis denne bruges til at udregne den totale energi, vil energien ikke længere være bevaret, fordi hver partikels potentielle energi ikke er offset.
    // Dette er samme konvention som NIST's tabeller.
    public float GetCorrectedPotentialEnergy()
    {
        float sigma2 = ParticleParameters.Sigma * ParticleParameters.Sigma;

        float totalPotentialEnergy = 0f;
        ForEachNeighborPair((i, j, difference, d2) =>
        {
            totalPotentialEnergy += LennardJones.CalculatePotentialEnergy(d2, sigma2, ParticleParameters.Epsilon);
        });

        float longRangeCorrection = ParticleCount * LennardJones.CalculateEnergyCorrectionPerParticle(Density, ParticleParameters.Sigma, ParticleParameters.Epsilon, LennardJonesCutoff);
        return totalPotentialEnergy + longRangeCorrection;
    }

    public float GetTemperature()
    {
        float totalKineticEnergy = GetTotalKineticEnergy();
        return GetTemperature(totalKineticEnergy);
    }
    public float GetTemperature(float totalKineticEnergy)
    {
        float temperature = (2f / 3f) * (totalKineticEnergy / particles.Length) / (float)Units.BoltzmannConstant;
        return temperature;
    }

    public float GetPressure()
    {
        return GetPressure(GetTotalKineticEnergy());
    }

    // Virial-trykket er en måde at beregne trykket i en simulering ud fra alle partiklerne, uden at have en væg at måle det på.
    // Kilde: Gould, Tobochnik og Christian, "An Introduction to Computer Simulation Methods", 3rd edition, afsnit 8.7 formel 8.8
    // P*V = NkT + 1/3 * sum(r_ij * F_ij)
    // Temperatur er defineret som T = 2/3 * (E_kin / Nk) sat ind og isoleret for trykket giver det:
    // P = (2 * E_kin + sum(r_ij * F_ij)) / (3 * V)
    // Summen udregnes i Update() metoden, og gemmes i lastVirial, så den ikke skal udregnes igen her.
    public float GetPressure(float totalKineticEnergy)
    {
        return (2f * totalKineticEnergy + lastVirial) / (3f * BoxVolume);
    }

    // Trykket plus long-range correction. Giver mere realistiske værdier, da den tager højde for partikler udenfor cutoff-afstanden.
    // Samme konvention som NIST's tabeller
    public float GetCorrectedPressure(float totalKineticEnergy)
    {
        float longRangeCorrection = LennardJones.CalculatePressureCorrection(Density, ParticleParameters.Sigma, ParticleParameters.Epsilon, LennardJonesCutoff);
        return GetPressure(totalKineticEnergy) + longRangeCorrection;
    }

    // Vektoren fra p2 til p1. Kassen har periodical boundaries, så den nærmeste kopi af p2 bruges (minimum image).
    private Vector3 GetDisplacement(Vector3 p1, Vector3 p2)
    {
        var difference = p1 - p2;
        difference.X = GetNearestImage(difference.X, BoxSize.X);
        difference.Y = GetNearestImage(difference.Y, BoxSize.Y);
        difference.Z = GetNearestImage(difference.Z, BoxSize.Z);
        return difference;
    }

    // Begge partikler ligger i kassen, så afstanden er mindre end kassens bredde, og én forskydning er nok
    private static float GetNearestImage(float difference, float boxLength)
    {
        if (difference > boxLength / 2) return difference - boxLength;
        else if (difference < -boxLength / 2) return difference + boxLength;
        else return difference;
    }

    // Gitteret wrapper rundt, fordi kassen har periodical boundaries. Nabocellen er højst én celle udenfor gitteret.
    private static int GetNeighborCell(int cell, int cellCount)
    {
        if (cell < 0) return cell + cellCount;
        else if (cell >= cellCount) return cell - cellCount;
        else return cell;
    }

    // Udfører funktionen `action` på hvert par af partikler, der er indenfor LennardJonesCutoff i gitteret.
    // Dette bruges til at beregne kræfter og potentielle energier mellem partiklerne.
    private void ForEachNeighborPair(Action<int, int, Vector3, float> action)
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
                int neighborZ = GetNeighborCell(z + zo, gridCellsZ);

                for (int yo = -1; yo <= 1; yo++)
                {
                    int neighborY = GetNeighborCell(y + yo, gridCellsY);

                    for (int xo = -1; xo <= 1; xo++)
                    {
                        int neighborX = GetNeighborCell(x + xo, gridCellsX);

                        int currentNeighborIndex = neighborX + neighborY * gridCellsX + neighborZ * gridCellsX * gridCellsY;
                        int currentNeighborParticleIndex = gridHead[currentNeighborIndex];
                        while (currentNeighborParticleIndex != -1)
                        {
                            int j = currentNeighborParticleIndex;
                            currentNeighborParticleIndex = gridNext[currentNeighborParticleIndex];

                            if (j <= i) continue; // Skip sig selv eller partikler der allerede er blevet behandlet for at undgå dobbelttælling

                            var difference = GetDisplacement(particles[i].Position, particles[j].Position);
                            float d2 = difference.LengthSquared();
                            if (d2 > cutoff2)
                            {
                                continue; // Spring over hvis partiklerne er langt fra hinanden, da det giver en kraft der nærmest er 0
                            }

                            action(i, j, difference, d2); // Udfør handlingen på partikelparret
                        }
                    }
                }
            }
        }
    }

    public void Update()
    {
        float sigma2 = ParticleParameters.Sigma * ParticleParameters.Sigma;

        for (int i = 0; i < particles.Length; i++)
        {
            int gridIndex = GetGridIndex(particles[i].Position);
            gridHead[gridIndex] = -1; // Reset cellen, dette resetter alle celler med partikler i, alle andre er allerede sat til -1, så det er ikke nødvendigt at sætte dem igen
        }

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].ApplyHalfAcceleration(SimulationParameters.TimePerStepPs); // Velocity Verlet integration

            particles[i].Update(BoxSize, SimulationParameters.TimePerStepPs);

            particles[i].ResetAcceleration(); // Nulstil acceleration efter opdatering, så den ikke akkumuleres over tid

            int gridIndex = GetGridIndex(particles[i].Position); // Find cellen som partikel i befinder sig i efter opdateringen
            int currentHead = gridHead[gridIndex]; // Gem den nuværende første partikel i cellen
            gridHead[gridIndex] = i; // Sæt partikel i som den nye første partikel i cellen
            gridNext[i] = currentHead; // Sæt partiklen til at pege på den tidligere første partikel i cellen
        }

        // Loop igennem alle par af partikler og beregn kraften mellem dem
        float virial = 0f;
        ForEachNeighborPair((i, j, difference, d2) =>
        {
            var force = LennardJones.CalculateForce(difference, d2, sigma2, ParticleParameters.Epsilon);
            particles[i].ApplyForce(force, ParticleParameters.Mass);
            particles[j].ApplyForce(-force, ParticleParameters.Mass);
            virial += Vector3.Dot(difference, force);
        });
        lastVirial = virial;

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].ApplyHalfAcceleration(SimulationParameters.TimePerStepPs); // Velocity Verlet integration
        }
    }
}
