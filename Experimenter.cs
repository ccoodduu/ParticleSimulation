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

    public const string CsvHeader = "KineticEnergy;PotentialEnergy;Pressure;Temperature;Volume;ParticleCount";

    public override string ToString()
    {
        return String.Format(
            "Ekin = {0:G6} u·Å²/ps², Epot = {1:G6} u·Å²/ps², P = {2:G6} u/(Å·ps²), T = {3:G6} K, V = {4:G6} Å³, N = {5}",
            KineticEnergy, PotentialEnergy, Pressure, Temperature, Volume, ParticleCount);
    }

    public string ToCsvLine()
    {
        return String.Join(';',
            KineticEnergy.ToString(CultureInfo.InvariantCulture),
            PotentialEnergy.ToString(CultureInfo.InvariantCulture),
            Pressure.ToString(CultureInfo.InvariantCulture),
            Temperature.ToString(CultureInfo.InvariantCulture),
            Volume.ToString(CultureInfo.InvariantCulture),
            ParticleCount.ToString(CultureInfo.InvariantCulture));
    }
}

public struct MeasurementSettings
{
    public int StepToMeasureAt;
    public int StepsToAverageOver;
}

public static class Experimenter
{
    public static Measurement[] RunExperiments(SimulationParameters simulationParameters, ParticleParameters particleParameters, MeasurementSettings measurementSettings, Func<SimulationParameters, ParticleParameters, MeasurementSettings, int, (SimulationParameters, ParticleParameters, MeasurementSettings)> experimentsCreator, int numExperiments)
    {
        Measurement[] measurements = new Measurement[numExperiments];

        for (int i = 0; i < numExperiments; i++)
        {
            var (sp, pp, ms) = experimentsCreator(simulationParameters, particleParameters, measurementSettings, i);

            var measurement = RunExperiment(sp, pp, ms);
            measurements[i] = measurement;
        }

        return measurements;
    }

    public static Measurement RunExperiment(SimulationParameters simulationParameters, ParticleParameters particleParameters, MeasurementSettings measurementSettings)
    {
        ParticleSimulation3D simulation = new(simulationParameters, particleParameters);

        for (int i = 0; i < measurementSettings.StepToMeasureAt; i++)
        {
            simulation.Update();
        }

        Measurement measurementTotal = new Measurement();

        for (int i = 0; i < measurementSettings.StepsToAverageOver; i++)
        {
            var measurement = simulation.TakeMeasurement();
            measurementTotal = measurementTotal.Add(measurement);

            simulation.Update();
        }

        Measurement averageMeasurement = measurementTotal.Divide(measurementSettings.StepsToAverageOver);

        return averageMeasurement;
    }

    private static Measurement TakeMeasurement(this ParticleSimulation3D simulation)
    {
        var kineticEnergy = simulation.GetTotalKineticEnergy();

        return new Measurement
        {
            Pressure = simulation.GetPressure(),
            KineticEnergy = kineticEnergy,
            PotentialEnergy = simulation.GetTotalPotentialEnergy(),
            Temperature = simulation.GetTemperature(kineticEnergy),
            Volume = simulation.BoxVolume,
            ParticleCount = simulation.ParticleCount
        };
    }
}
