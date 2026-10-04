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
