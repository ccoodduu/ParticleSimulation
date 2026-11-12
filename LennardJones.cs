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
        return CalculateForce(p1 - p2, sigma2, epsilon);
    }
    public static Vector3 CalculateForce(Vector3 difference, float sigma2, float epsilon)
    {
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

    // Når vi bruger en cutoff distance, skal vi også tage højde for at potentialet ikke går til nul ved cutoff.
    // Derfor beregner vi offsettet som potentialet ved cutoff og trækker det fra potentialet mellem de to partikler.
    // Dette giver ikke en realistisk potentiel energi, men det er den der skal bruges for at opfylde energibevarelse i simulationen,
    // ellers vil der brat forsvinde eller opstå energi det moment 2 partikler bevæger sig ind eller uden for cutoff.
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

    // Long-range correction
    // Med en cutoff distance bliver der ikke taget højde for partikler der er længere væk end cutoff.
    // Det er især et problem for den potentielle energi og trykket ved høj densitet.
    // Trykket bliver for højt, da partiklerne holder for lidt sammen på hinanden, grundet manglende tiltrækning fra partiklerne længere væk.
    // Derfor er trykket fra virialet for højt.
    // Den potentielle energi bliver for høj, fordi der er en negativ energi i tiltrækningen til partiklerne længere væk, som ikke tages højde for.
    // Energien fra tiltrækningen er negativ, fordi det kræver energi at trække partiklerne fra hinanden, og U er 0, når de er uendeligt langt fra hinanden.

    // For at rette op på dette kan man lave en long-range correction, som er en analytisk beregning af den potentielle energi og trykket fra partiklerne længere væk end cutoff.
    // Den antager at partiklerne længere væk end cutoff er jævnt fordelt med en densitet ρ.
    // Ved at integrere Lennard-Jones potentialet fra cutoff til uendelig kan man beregne den potentielle energi og trykket fra partiklerne længere væk end cutoff.

    // Det giver følgende formler:
    // U_LRC = 8/3 * pi * density * epsilon * sigma^3 * ( 1/3 * (sigma / cutoff)^9 - (sigma / cutoff)^3 )
    // Dette er den potentielle energi per partikel, som skal lægges til den beregnede potentielle energi for at få den korrekte totale potentielle energi.
    public static float CalculateEnergyCorrectionPerParticle(float density, float sigma, float epsilon, float cutoff)
    {
        float sigma3 = sigma * sigma * sigma;
        float sigmaOverCutoff3 = sigma3 / (cutoff * cutoff * cutoff);
        float sigmaOverCutoff9 = sigmaOverCutoff3 * sigmaOverCutoff3 * sigmaOverCutoff3;

        return 8f / 3f * MathF.PI * density * epsilon * sigma3 * (sigmaOverCutoff9 / 3f - sigmaOverCutoff3);
    }

    // P_LRC = 16/3 * pi * density^2 * epsilon * sigma^3 * (2/3 * (sigma/cutoff)^9 - (sigma/cutoff)^3)
    public static float CalculatePressureCorrection(float density, float sigma, float epsilon, float cutoff)
    {
        float sigma3 = sigma * sigma * sigma;
        float sigmaOverCutoff3 = sigma3 / (cutoff * cutoff * cutoff);
        float sigmaOverCutoff9 = sigmaOverCutoff3 * sigmaOverCutoff3 * sigmaOverCutoff3;

        return 16f / 3f * MathF.PI * density * density * epsilon * sigma3 * (2f / 3f * sigmaOverCutoff9 - sigmaOverCutoff3);
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

