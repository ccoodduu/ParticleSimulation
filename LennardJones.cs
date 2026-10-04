using System.Numerics;

namespace ParticleSimulation;

static class LennardJones
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

