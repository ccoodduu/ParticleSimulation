namespace ParticleSimulation;

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

    // Masserne er IUPAC's standard-atommasser.
    // Kilderne angiver epsilon som ε/k i kelvin, så den ganges med Boltzmanns konstant for at få simuleringens energienhed.

    // Svehla, R. A. (1962). Estimated viscosities and thermal conductivities of gases at high temperatures. NASA Technical Report R-132, tabel I.
    public static ParticleParameters Helium => Create(4.0026f, 2.551f, 10.22f);

    // Rutkai, G., Thol, M., Span, R. & Vrabec, J. (2017). How well does the Lennard-Jones potential represent the thermodynamic properties of noble gases?
    // Molecular Physics 115, 1104-1121, tabel 1 (LJ). https://doi.org/10.1080/00268976.2016.1246760
    public static ParticleParameters Neon => Create(20.180f, 2.801f, 33.921f);
    public static ParticleParameters Argon => Create(39.948f, 3.3952f, 116.79f);
    public static ParticleParameters Krypton => Create(83.798f, 3.6274f, 162.58f);

    private static ParticleParameters Create(float mass, float sigma, float epsilonOverBoltzmann)
    {
        return new ParticleParameters
        {
            Radius = sigma / 2f,
            Mass = mass,
            Sigma = sigma,
            Epsilon = epsilonOverBoltzmann * (float)Units.BoltzmannConstant
        };
    }
}
