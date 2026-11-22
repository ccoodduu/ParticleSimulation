using System.Globalization;

namespace ParticleSimulation;

public struct Measurement
{
    public float KineticEnergy;
    public float PotentialEnergy;
    public float Pressure;
    public float Temperature;
    public float Volume;
    public int ParticleCount;

    public Measurement Add(Measurement m2)
    {
        return new Measurement
        {
            KineticEnergy = this.KineticEnergy + m2.KineticEnergy,
            PotentialEnergy = this.PotentialEnergy + m2.PotentialEnergy,
            Pressure = this.Pressure + m2.Pressure,
            Temperature = this.Temperature + m2.Temperature,
            Volume = m2.Volume,
            ParticleCount = m2.ParticleCount
        };
    }

    public Measurement Divide(int c)
    {
        return new Measurement
        {
            KineticEnergy = this.KineticEnergy / c,
            PotentialEnergy = this.PotentialEnergy / c,
            Pressure = this.Pressure / c,
            Temperature = this.Temperature / c,
            Volume = this.Volume,
            ParticleCount = this.ParticleCount
        };
    }

    // CSV-filen er i SI-enheder
    public const string CsvHeader = "KineticEnergy (J);PotentialEnergy (J);Pressure (Pa);Temperature (K);Volume (m^3);ParticleCount";

    public override string ToString()
    {
        return String.Format(
            "Ekin = {0:G6} u·Å²/ps², Epot = {1:G6} u·Å²/ps², P = {2:G6} u/(Å·ps²), T = {3:G6} K, V = {4:G6} Å³, N = {5}",
            KineticEnergy, PotentialEnergy, Pressure, Temperature, Volume, ParticleCount);
    }

    public string ToCsvLine()
    {
        return String.Join(';',
            (KineticEnergy * Units.JoulePerEnergyUnit).ToString(CultureInfo.InvariantCulture),
            (PotentialEnergy * Units.JoulePerEnergyUnit).ToString(CultureInfo.InvariantCulture),
            (Pressure * Units.PascalPerPressureUnit).ToString(CultureInfo.InvariantCulture),
            Temperature.ToString(CultureInfo.InvariantCulture),
            (Volume * Math.Pow(Units.MeterPerAngstrom, 3)).ToString(CultureInfo.InvariantCulture),
            ParticleCount.ToString(CultureInfo.InvariantCulture));
    }
}

public struct MeasurementSettings
{
    public int StepToMeasureAt;
    public int StepsToAverageOver;
    public int StepsBetweenMeasurements = 1; // Målingen af den potentielle energi gennemløber alle nabopar en ekstra gang, så det er dyrt at måle i hvert step
    public float? TargetTemperature = null; // K. Hvis den er sat, findes startfarten automatisk, og ParticleStartVelocity ignoreres
    public float TemperatureTolerance = 1f; // K. Hvor tæt prøvekørslerne skal ramme TargetTemperature
    public int TemperatureSearchStepsToAverageOver = 10000; // Hvor mange steps prøvekørslerne måler temperaturen over

    public MeasurementSettings() { }
}

public static class Experimenter
{
    public static Measurement[] RunExperiments(SimulationParameters simulationParameters, ParticleParameters particleParameters, MeasurementSettings measurementSettings, Func<SimulationParameters, ParticleParameters, MeasurementSettings, int, (SimulationParameters, ParticleParameters, MeasurementSettings)> experimentsCreator, int numExperiments, int maxThreads)
    {
        Measurement[] measurements = new Measurement[numExperiments];

        Parallel.For(0, numExperiments, new ParallelOptions { MaxDegreeOfParallelism = maxThreads }, i =>
        {
            var (sp, pp, ms) = experimentsCreator(simulationParameters, particleParameters, measurementSettings, i);

            var measurement = RunExperiment(sp, pp, ms);
            measurements[i] = measurement;
        });

        return measurements;
    }

    public static Measurement RunExperiment(SimulationParameters simulationParameters, ParticleParameters particleParameters, MeasurementSettings measurementSettings)
    {
        if (measurementSettings.TargetTemperature is float targetTemperature)
        {
            simulationParameters.ParticleStartVelocity = FindStartVelocity(simulationParameters, particleParameters, targetTemperature, measurementSettings);
        }

        ParticleSimulation3D simulation = new(simulationParameters, particleParameters);

        for (int i = 0; i < measurementSettings.StepToMeasureAt; i++)
        {
            simulation.Update();
        }

        Measurement measurementTotal = new Measurement();
        int measurementCount = 0;

        for (int i = 0; i < measurementSettings.StepsToAverageOver; i++)
        {
            if (i % measurementSettings.StepsBetweenMeasurements == 0)
            {
                var measurement = simulation.TakeMeasurement();
                measurementTotal = measurementTotal.Add(measurement);
                measurementCount++;
            }

            simulation.Update();
        }

        Measurement averageMeasurement = measurementTotal.Divide(measurementCount);

        return averageMeasurement;
    }

    // Simuleringen bevarer energien, så temperaturen kan ikke vælges direkte. Den afhænger af startfarten og af den potentielle energi,
    // der frigives, når startgitteret går i opløsning. Derfor findes startfarten med korte prøvekørsler, før den lange måling køres.
    private static float FindStartVelocity(SimulationParameters simulationParameters, ParticleParameters particleParameters, float targetTemperature, MeasurementSettings measurementSettings)
    {
        const int maxAttempts = 10;
        float temperatureTolerance = measurementSettings.TemperatureTolerance;
        var searchSettings = new MeasurementSettings
        {
            StepToMeasureAt = 20000,
            StepsToAverageOver = measurementSettings.TemperatureSearchStepsToAverageOver,
            StepsBetweenMeasurements = 10
        };

        float startTemperature = targetTemperature;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            simulationParameters.ParticleStartVelocity = GetStartVelocity(startTemperature, particleParameters.Mass);

            float temperatureError = RunExperiment(simulationParameters, particleParameters, searchSettings).Temperature - targetTemperature;
            if (MathF.Abs(temperatureError) <= temperatureTolerance) break;

            // Blev det for varmt, startes næste forsøg lige så meget koldere
            startTemperature -= temperatureError;
        }

        return simulationParameters.ParticleStartVelocity;
    }

    // Udregn farten for en partikel med en given temperatur og masse, det er bare temperatur-udregningen omvendt.
    // Bruges til at komme med et bud på startfarten, når der ønskes en bestemt temperatur. 
    private static float GetStartVelocity(float temperature, float mass)
    {
        return MathF.Sqrt(3f * (float)Units.BoltzmannConstant * temperature / mass);
    }

    private static Measurement TakeMeasurement(this ParticleSimulation3D simulation)
    {
        var kineticEnergy = simulation.GetTotalKineticEnergy();

        return new Measurement
        {
            Pressure = simulation.GetCorrectedPressure(kineticEnergy),
            KineticEnergy = kineticEnergy,
            PotentialEnergy = simulation.GetCorrectedPotentialEnergy(),
            Temperature = simulation.GetTemperature(kineticEnergy),
            Volume = simulation.BoxVolume,
            ParticleCount = simulation.ParticleCount
        };
    }
}
