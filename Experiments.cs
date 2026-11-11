using System.Globalization;
using System.Numerics;

namespace ParticleSimulation;

public static class Experiments
{
    public static void StartVelocityExperiment()
    {
        float[] startVelocities = [2f, 3f, 4f, 5f, 6f]; // Å/ps

        var simulationParameters = new SimulationParameters(1000, new Vector3(100), startVelocities[0]);
        var measurementSettings = new MeasurementSettings
        {
            StepToMeasureAt = 20000,
            StepsToAverageOver = 10000
        };

        var measurements = Experimenter.RunExperiments(
            simulationParameters,
            ParticleParameters.Argon,
            measurementSettings,
            (sp, pp, ms, i) =>
            {
                sp.ParticleStartVelocity = startVelocities[i];
                Console.WriteLine($"Kører forsøg {i + 1}/{startVelocities.Length} (v0 = {startVelocities[i]} Å/ps)...");
                return (sp, pp, ms);
            },
            startVelocities.Length);

        var lines = new List<string> { "StartVelocity;" + Measurement.CsvHeader };
        for (int i = 0; i < measurements.Length; i++)
        {
            Console.WriteLine($"v0 = {startVelocities[i]} Å/ps: {measurements[i]}");
            lines.Add(startVelocities[i].ToString(CultureInfo.InvariantCulture) + ";" + measurements[i].ToCsvLine());
        }

        const string path = "startvelocity_experiment.csv";
        File.WriteAllLines(path, lines);
        Console.WriteLine($"Resultater gemt i {Path.GetFullPath(path)}");
    }

    // Reducerede enheder: T* = kT/ε, ρ* = ρσ³, U* = U/(Nε), p* = pσ³/ε
    private readonly record struct NistReference(float Temperature, float Density, float PotentialEnergy, float Pressure);

    // NIST LJ_PURE, NVT Monte Carlo, damp-tilstande: https://mmlapps.nist.gov/srs/LJ_PURE/mc.htm
    private static readonly NistReference[] NistVaporReferences =
    [
        new(0.85f, 0.001f, -1.0317e-02f, 8.4402e-04f),
        new(0.85f, 0.003f, -3.1019e-02f, 2.4965e-03f),
        new(0.85f, 0.005f, -5.1901e-02f, 4.1003e-03f),
        new(0.85f, 0.007f, -7.2834e-02f, 5.6565e-03f),
        new(0.85f, 0.009f, -9.3973e-02f, 7.1641e-03f),
        new(0.90f, 0.001f, -9.9165e-03f, 8.9429e-04f),
        new(0.90f, 0.003f, -2.9787e-02f, 2.6485e-03f),
        new(0.90f, 0.005f, -4.9771e-02f, 4.3569e-03f),
        new(0.90f, 0.007f, -6.9805e-02f, 6.0193e-03f),
        new(0.90f, 0.009f, -8.9936e-02f, 7.6363e-03f),
    ];

    public static void NistVaporExperiment()
    {
        const int particleCount = 1000;
        const float temperatureTolerance = 0.003f;
        const int maxAttempts = 4;

        var particleParameters = ParticleParameters.Argon;
        var measurementSettings = new MeasurementSettings
        {
            StepToMeasureAt = 500000,
            StepsToAverageOver = 500000
        };

        var lines = new string[NistVaporReferences.Length];

        Parallel.For(0, NistVaporReferences.Length, i =>
        {
            var reference = NistVaporReferences[i];
            float boxSide = MathF.Cbrt(particleCount / reference.Density) * particleParameters.Sigma;

            // Startgitteret har U = 0, så energibevarelse giver (3/2)T*₀ = (3/2)T* + U*
            float startTemperature = reference.Temperature + 2f / 3f * reference.PotentialEnergy;

            for (int attempt = 1; ; attempt++)
            {
                float startVelocity = MathF.Sqrt(3f * startTemperature * particleParameters.Epsilon / particleParameters.Mass);
                var simulationParameters = new SimulationParameters(particleCount, new Vector3(boxSide), startVelocity);

                var measurement = Experimenter.RunExperiment(simulationParameters, particleParameters, measurementSettings);

                float temperature = measurement.Temperature * (float)Units.BoltzmannConstant / particleParameters.Epsilon;
                float potentialEnergy = measurement.PotentialEnergy / (particleCount * particleParameters.Epsilon);
                float pressure = measurement.Pressure * MathF.Pow(particleParameters.Sigma, 3) / particleParameters.Epsilon;
                float virialPressure = measurement.VirialPressure * MathF.Pow(particleParameters.Sigma, 3) / particleParameters.Epsilon;
                float temperatureError = temperature - reference.Temperature;

                Console.WriteLine($"T* = {reference.Temperature}, ρ* = {reference.Density}, forsøg {attempt}: v0 = {startVelocity:F4} Å/ps, målt T* = {temperature:F4}, U* = {potentialEnergy:G5} (NIST {reference.PotentialEnergy:G5}), p* = {pressure:G5}, virial p* = {virialPressure:G5} (NIST {reference.Pressure:G5})");

                if (MathF.Abs(temperatureError) > temperatureTolerance && attempt < maxAttempts)
                {
                    startTemperature -= temperatureError;
                    continue;
                }

                float cutoff = simulationParameters.LennardJonesCutoffMultiplier;
                float expectedPotentialEnergyDeviation = ExpectedPotentialEnergyDeviation(reference.Density, cutoff);
                float expectedPressureDeviation = ExpectedPressureDeviation(reference.Density, cutoff);

                lines[i] = string.Join(';', new float[]
                {
                    reference.Temperature, reference.Density, particleCount, boxSide, startVelocity, attempt,
                    temperature, potentialEnergy, pressure, virialPressure,
                    reference.PotentialEnergy, reference.Pressure,
                    potentialEnergy - reference.PotentialEnergy, expectedPotentialEnergyDeviation,
                    pressure - reference.Pressure, expectedPressureDeviation,
                }.Select(v => v.ToString(CultureInfo.InvariantCulture)));
                break;
            }
        });

        const string header = "TargetT;Density;ParticleCount;BoxSideAngstrom;StartVelocity;Attempts;T;U;P;VirialP;NistU;NistP;UDeviation;ExpectedUDeviation;PDeviation;ExpectedPDeviation";

        const string path = "nist_vapor_experiment.csv";
        File.WriteAllLines(path, lines.Prepend(header));
        Console.WriteLine($"Resultater gemt i {Path.GetFullPath(path)}");
    }

    // Simuleringen bruger et shiftet potentiale uden long-range correction (LRC), mens NIST bruger et ushiftet potentiale med LRC.
    // Antager g(r) = 1 inden for cutoff, så hver partikel har ½ρ(4/3)πr_c³ naboer, der hver løftes med -U(r_c).
    private static float ExpectedPotentialEnergyDeviation(float density, float cutoff)
    {
        float cutoffPotential = 4f * (MathF.Pow(cutoff, -12) - MathF.Pow(cutoff, -6));
        float shift = -cutoffPotential * 2f / 3f * MathF.PI * MathF.Pow(cutoff, 3) * density;
        float longRangeCorrection = 8f / 3f * MathF.PI * density * (MathF.Pow(cutoff, -9) / 3f - MathF.Pow(cutoff, -3));
        return shift - longRangeCorrection;
    }

    private static float ExpectedPressureDeviation(float density, float cutoff)
    {
        float longRangeCorrection = 16f / 3f * MathF.PI * density * density * (2f / 3f * MathF.Pow(cutoff, -9) - MathF.Pow(cutoff, -3));
        return -longRangeCorrection;
    }
}
