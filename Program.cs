using ParticleSimulation;
using Raylib_cs;
using System.Numerics;

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
Raylib.InitWindow(Globals.ScreenWidth, Globals.ScreenHeight, "Particle Simulation");
Raylib.SetTargetFPS(Globals.Fps);

var particleContainer = new ParticleContainer3D(1000, new Vector3(100, 100, 100), ParticleParameters.Argon);
var renderer = new Renderer3D(particleContainer);

const int maxStepsPerFrame = Globals.Sps / Globals.Fps * 2;

int totalStepCount = 0;
float unusedTime = 0;
while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();
    unusedTime += dt;

    int ticks = 0;
    while (unusedTime >= Globals.RealStepTime && ticks < maxStepsPerFrame)
    {
        unusedTime -= Globals.RealStepTime;
        Update();
        totalStepCount++;
        ticks++;
    }

    if (ticks >= maxStepsPerFrame) unusedTime = 0;

    Raylib.BeginDrawing();

    Draw();

    Raylib.DrawFPS(10, 10);
    Raylib.EndDrawing();
}

Raylib.CloseWindow();

void Update()
{
    particleContainer.Update();
}

void Draw()
{
    Raylib.ClearBackground(Color.Black);

    renderer.Draw();

    float kineticEnergy = particleContainer.GetTotalKineticEnergy();
    float potentialEnergy = particleContainer.GetTotalPotentialEnergy();
    float totalEnergy = kineticEnergy + potentialEnergy;
    float temperatureK = particleContainer.GetTemperature(kineticEnergy);
    float temperatureC = temperatureK - 273.15f;
    float pressure = particleContainer.GetPressure(); // Enheden er u / (Å * ps^2)
    float pressureKPa = pressure * 1.660540200e7f / 1000f;

    Raylib.DrawText("Total Kinetic Energy: " + kineticEnergy.ToString("F2"), 10, 30, 20, Color.White);
    Raylib.DrawText("Total Potential Energy: " + potentialEnergy.ToString("F2"), 10, 60, 20, Color.White);
    Raylib.DrawText("Total Energy: " + totalEnergy.ToString("F2"), 10, 90, 20, Color.White);
    Raylib.DrawText("Temperature: " + temperatureK.ToString("F2") + " K (" + temperatureC.ToString("F2") + " °C)", 10, 120, 20, Color.White);
    Raylib.DrawText("Simulation Time: " + (totalStepCount * Globals.SimulationStepTimePs).ToString("F0") + " ps", 10, 150, 20, Color.White);
    Raylib.DrawText("Pressure: " + pressureKPa.ToString("F0") + " kPa", 10, 180, 20, Color.White);

    renderer.cameraDistance *= MathF.Pow(0.95f, Raylib.GetMouseWheelMove());

    if (Raylib.IsMouseButtonDown(MouseButton.Left))
    {
        var mouseDelta = Raylib.GetMouseDelta();
        renderer.cameraAngle.X -= mouseDelta.X * 0.001f;
        renderer.cameraAngle.Y += mouseDelta.Y * 0.001f;

        renderer.cameraAngle.X %= MathF.Tau;
        renderer.cameraAngle.Y = MathF.Max(-MathF.PI / 2f + 0.0001f, MathF.Min(MathF.PI / 2f - 0.0001f, renderer.cameraAngle.Y));
    }
}



namespace ParticleSimulation
{
    public static class Globals
    {
        public const int ScreenWidth = 1280;
        public const int ScreenHeight = 720;

        public const int Fps = 60;
        public const int Sps = 1000;

        public const float RealFrameTime = 1f / Fps;
        public const float RealStepTime = 1f / Sps;

        public const float SimulationStepTimePs = 0.005f;
        public const float PsPerSecond = 1_000_000_000_000f;
        public const float SimulationStepTimeSeconds = SimulationStepTimePs / PsPerSecond;
    }
}
