namespace ParticleSimulation;

// Alle beregninger i simulationen udøves med enheder der afledes fra:
// Længde: Angstrom (Å)
// Masse: Unit (u)
// Tid: Pikosekund (ps)
// Det giver de afledte enheder:
// Tryk: u / (ps^2 * Å)
// Energi: (u * Å^2) / ps^2
public static class Units
{
    // SI conversions
    public const double MeterPerAngstrom = 1.0e-10;
    public const double KgPerUnit = 1.660540200e-27;
    public const double SecondsPerPs = 1e-12;

    public const double PascalPerPressureUnit = KgPerUnit / (SecondsPerPs * SecondsPerPs * MeterPerAngstrom);
    public const double JoulePerEnergyUnit = (KgPerUnit * MeterPerAngstrom * MeterPerAngstrom) / (SecondsPerPs * SecondsPerPs);

    public const double BoltzmannConstant = 1.380649e-23 / JoulePerEnergyUnit;
}
