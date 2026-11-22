using System.Globalization;
using System.Numerics;

namespace ParticleSimulation;

public static class Experiments
{
    // Et forsøg ved en bestemt temperatur (K) og tæthed (partikler pr. Å³)
    private readonly record struct Condition(float Temperature, float NumberDensity);

    // Tætheden i mol/l, som i NIST's filer
    private static Condition FromMolPerLiter(float temperature, float molPerLiter)
    {
        return new(temperature, (float)(molPerLiter * 1000 * Units.AvogadroConstant * Math.Pow(Units.MeterPerAngstrom, 3)));
    }

    // Reducerede enheder, som i NIST LJ_PURE: T* = kT/ε, ρ* = ρσ³
    private static Condition FromReducedUnits(float reducedTemperature, float reducedDensity)
    {
        var argon = ParticleParameters.Argon;
        return new(reducedTemperature * argon.Epsilon / (float)Units.BoltzmannConstant, reducedDensity / (argon.Sigma * argon.Sigma * argon.Sigma));
    }

    // NIST LJ_PURE, NVT Monte Carlo, damp-tilstande: https://mmlapps.nist.gov/srs/LJ_PURE/mc.htm
    private static readonly Condition[] NistVaporConditions =
    [
        FromReducedUnits(0.85f, 0.001f),
        FromReducedUnits(0.85f, 0.003f),
        FromReducedUnits(0.85f, 0.005f),
        FromReducedUnits(0.85f, 0.007f),
        FromReducedUnits(0.85f, 0.009f),
        FromReducedUnits(0.90f, 0.001f),
        FromReducedUnits(0.90f, 0.003f),
        FromReducedUnits(0.90f, 0.005f),
        FromReducedUnits(0.90f, 0.007f),
        FromReducedUnits(0.90f, 0.009f),
    ];

    // NIST Chemistry WebBook, isotermer for helium: ../NIST/helium 180K.csv og helium 300K.csv
    private static readonly Condition[] HeliumConditions =
    [
        FromMolPerLiter(180f, 0.066764f),
        FromMolPerLiter(180f, 0.61838f),
        FromMolPerLiter(180f, 1.2706f),
        FromMolPerLiter(180f, 3.1665f),
        FromMolPerLiter(180f, 6.1388f),
        FromMolPerLiter(180f, 8.9012f),
        FromMolPerLiter(180f, 11.561f),
        FromMolPerLiter(180f, 13.968f),
        FromMolPerLiter(180f, 16.229f),
        FromMolPerLiter(180f, 20.374f),
        FromMolPerLiter(180f, 24.093f),
        FromMolPerLiter(180f, 27.460f),
        FromMolPerLiter(180f, 33.355f),
        FromMolPerLiter(180f, 38.397f),
        FromMolPerLiter(300f, 0.040072f),
        FromMolPerLiter(300f, 0.37220f),
        FromMolPerLiter(300f, 0.76734f),
        FromMolPerLiter(300f, 1.9310f),
        FromMolPerLiter(300f, 3.8014f),
        FromMolPerLiter(300f, 5.5918f),
        FromMolPerLiter(300f, 7.3642f),
        FromMolPerLiter(300f, 9.0101f),
        FromMolPerLiter(300f, 10.593f),
        FromMolPerLiter(300f, 13.585f),
        FromMolPerLiter(300f, 16.372f),
        FromMolPerLiter(300f, 18.979f),
        FromMolPerLiter(300f, 23.732f),
        FromMolPerLiter(300f, 27.972f),
    ];

    // NIST Chemistry WebBook, isotermer for neon: ../NIST/neon 180K.csv og neon 300K.csv
    private static readonly Condition[] NeonConditions =
    [
        FromMolPerLiter(180f, 0.066787f),
        FromMolPerLiter(180f, 0.62034f),
        FromMolPerLiter(180f, 1.2787f),
        FromMolPerLiter(180f, 3.2136f),
        FromMolPerLiter(180f, 6.2943f),
        FromMolPerLiter(180f, 9.1813f),
        FromMolPerLiter(180f, 11.957f),
        FromMolPerLiter(180f, 14.449f),
        FromMolPerLiter(180f, 16.764f),
        FromMolPerLiter(180f, 20.909f),
        FromMolPerLiter(180f, 24.492f),
        FromMolPerLiter(180f, 27.612f),
        FromMolPerLiter(180f, 32.799f),
        FromMolPerLiter(180f, 36.985f),
        FromMolPerLiter(300f, 0.040072f),
        FromMolPerLiter(300f, 0.37223f),
        FromMolPerLiter(300f, 0.76745f),
        FromMolPerLiter(300f, 1.9313f),
        FromMolPerLiter(300f, 3.7998f),
        FromMolPerLiter(300f, 5.5821f),
        FromMolPerLiter(300f, 7.3368f),
        FromMolPerLiter(300f, 8.9546f),
        FromMolPerLiter(300f, 10.498f),
        FromMolPerLiter(300f, 13.380f),
        FromMolPerLiter(300f, 16.019f),
        FromMolPerLiter(300f, 18.444f),
        FromMolPerLiter(300f, 22.750f),
        FromMolPerLiter(300f, 26.458f),
    ];

    // NIST Chemistry WebBook, isotermer for argon: ../NIST/argon 180K.csv og argon 300K.csv
    private static readonly Condition[] ArgonConditions =
    [
        FromMolPerLiter(180f, 0.067088f),
        FromMolPerLiter(180f, 0.64779f),
        FromMolPerLiter(180f, 1.4039f),
        FromMolPerLiter(180f, 4.2262f),
        FromMolPerLiter(180f, 12.172f),
        FromMolPerLiter(180f, 18.503f),
        FromMolPerLiter(180f, 21.449f),
        FromMolPerLiter(180f, 23.201f),
        FromMolPerLiter(180f, 24.481f),
        FromMolPerLiter(180f, 26.340f),
        FromMolPerLiter(180f, 27.706f),
        FromMolPerLiter(180f, 28.796f),
        FromMolPerLiter(180f, 30.500f),
        FromMolPerLiter(180f, 31.825f),
        FromMolPerLiter(300f, 0.040115f),
        FromMolPerLiter(300f, 0.37593f),
        FromMolPerLiter(300f, 0.78315f),
        FromMolPerLiter(300f, 2.0294f),
        FromMolPerLiter(300f, 4.1626f),
        FromMolPerLiter(300f, 6.3038f),
        FromMolPerLiter(300f, 8.4391f),
        FromMolPerLiter(300f, 10.361f),
        FromMolPerLiter(300f, 12.103f),
        FromMolPerLiter(300f, 15.038f),
        FromMolPerLiter(300f, 17.352f),
        FromMolPerLiter(300f, 19.212f),
        FromMolPerLiter(300f, 22.045f),
        FromMolPerLiter(300f, 24.152f),
    ];

    // NIST Chemistry WebBook, isotermer for krypton: ../NIST/krypton 180K.csv og krypton 300K.csv
    private static readonly Condition[] KryptonConditions =
    [
        FromMolPerLiter(180f, 0.067455f),
        FromMolPerLiter(180f, 0.68841f),
        FromMolPerLiter(180f, 1.6632f),
        FromMolPerLiter(180f, 22.631f),
        FromMolPerLiter(180f, 23.473f),
        FromMolPerLiter(180f, 24.121f),
        FromMolPerLiter(180f, 24.674f),
        FromMolPerLiter(180f, 25.132f),
        FromMolPerLiter(180f, 25.538f),
        FromMolPerLiter(180f, 26.233f),
        FromMolPerLiter(180f, 26.821f),
        FromMolPerLiter(180f, 27.333f),
        FromMolPerLiter(180f, 28.200f),
        FromMolPerLiter(180f, 28.922f),
        FromMolPerLiter(300f, 0.040171f),
        FromMolPerLiter(300f, 0.38097f),
        FromMolPerLiter(300f, 0.80555f),
        FromMolPerLiter(300f, 2.1915f),
        FromMolPerLiter(300f, 4.9125f),
        FromMolPerLiter(300f, 8.0010f),
        FromMolPerLiter(300f, 10.974f),
        FromMolPerLiter(300f, 13.217f),
        FromMolPerLiter(300f, 14.898f),
        FromMolPerLiter(300f, 17.235f),
        FromMolPerLiter(300f, 18.840f),
        FromMolPerLiter(300f, 20.056f),
        FromMolPerLiter(300f, 21.859f),
        FromMolPerLiter(300f, 23.193f),
    ];

    public static void NistVaporExperiment()
    {
        var measurementSettings = new MeasurementSettings
        {
            StepToMeasureAt = 50000,
            StepsToAverageOver = 100000,
            StepsBetweenMeasurements = 10
        };
        RunAtConditions(NistVaporConditions, ParticleParameters.Argon, measurementSettings, "nist_vapor_experiment.csv");
    }

    public static void NobleGasIsothermExperiment()
    {
        var measurementSettings = new MeasurementSettings
        {
            StepToMeasureAt = 50000,
            StepsToAverageOver = 50000,
            StepsBetweenMeasurements = 10
        };
        RunAtConditions(HeliumConditions, ParticleParameters.Helium, measurementSettings, "helium_isotherm_experiment.csv");
        RunAtConditions(NeonConditions, ParticleParameters.Neon, measurementSettings, "neon_isotherm_experiment.csv");
        RunAtConditions(ArgonConditions, ParticleParameters.Argon, measurementSettings, "argon_isotherm_experiment.csv");
        RunAtConditions(KryptonConditions, ParticleParameters.Krypton, measurementSettings, "krypton_isotherm_experiment.csv");
    }

    // Argon ved 180 K med længere gennemsnit og skarpere temperatur, fordi den havde meget store udsving i tryk.
    public static void Argon180Experiment()
    {
        var measurementSettings = new MeasurementSettings
        {
            StepToMeasureAt = 50000,
            StepsToAverageOver = 200000,
            StepsBetweenMeasurements = 10,
            TemperatureTolerance = 0.25f,
            TemperatureSearchStepsToAverageOver = 20000
        };
        var conditions = ArgonConditions.Where(c => c.Temperature == 180f).ToArray();
        RunAtConditions(conditions, ParticleParameters.Argon, measurementSettings, "argon_180K_precise_experiment.csv");
    }

    private static void RunAtConditions(Condition[] conditions, ParticleParameters particleParameters, MeasurementSettings measurementSettings, string path)
    {
        const int particleCount = 1000;

        var simulationParameters = new SimulationParameters(particleCount, Vector3.Zero, 0f);

        // Ved samme temperatur har atomerne samme kinetiske energi, så lette atomer bevæger sig hurtigere (v ~ 1/√m).
        // Tidssteppet gøres mindre, så de flytter sig lige så langt pr. step som argon med 0,005 ps.
        simulationParameters.TimePerStepPs = MathF.Min(0.005f, 0.005f * MathF.Sqrt(particleParameters.Mass / ParticleParameters.Argon.Mass));

        var measurements = Experimenter.RunExperiments(
            simulationParameters,
            particleParameters,
            measurementSettings,
            (sp, pp, ms, i) =>
            {
                sp.BoxSize = new Vector3(MathF.Cbrt(particleCount / conditions[i].NumberDensity));
                ms.TargetTemperature = conditions[i].Temperature;
                Console.WriteLine($"{path}: kører forsøg {i + 1}/{conditions.Length} (T = {conditions[i].Temperature} K, V = {sp.BoxSize.X * sp.BoxSize.Y * sp.BoxSize.Z:G5} Å³)...");
                return (sp, pp, ms);
            },
            conditions.Length,
            Environment.ProcessorCount);

        var lines = new List<string> { "TargetTemperature;" + Measurement.CsvHeader };
        for (int i = 0; i < measurements.Length; i++)
        {
            Console.WriteLine($"T = {conditions[i].Temperature} K: {measurements[i]}");
            lines.Add(conditions[i].Temperature.ToString(CultureInfo.InvariantCulture) + ";" + measurements[i].ToCsvLine());
        }

        File.WriteAllLines(path, lines);
        Console.WriteLine($"Resultater gemt i {Path.GetFullPath(path)}");
    }
}
