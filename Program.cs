using ParticleSimulation;
using Raylib_cs;
using System.Numerics;

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
Raylib.InitWindow(Globals.ScreenWidth, Globals.ScreenHeight, "Particle Simulation");
Raylib.SetTargetFPS(Globals.Fps);

var particleContainer = new ParticleContainer3D(1000, new Vector3(100, 100, 100), ParticleParameters.Argon);
var renderer = new Renderer3D(particleContainer);

const int maxTicksPerFrame = 20;

float unusedSimulationTime = 0;
while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();
    unusedSimulationTime += dt;

    int ticks = 0;
    while (unusedSimulationTime >= Globals.TickTime && ticks < maxTicksPerFrame)
    {
        unusedSimulationTime -= Globals.TickTime;
        Update();
        ticks++;
    }

    if (ticks >= maxTicksPerFrame) unusedSimulationTime = 0;

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

    Raylib.DrawText("Total Kinetic Energy: " + particleContainer.GetTotalKineticEnergy().ToString("F2"), 10, 30, 20, Color.White);
    Raylib.DrawText("Total Potential Energy: " + particleContainer.GetTotalPotentialEnergy().ToString("F2"), 10, 60, 20, Color.White);
    Raylib.DrawText("Total Energy: " + (particleContainer.GetTotalKineticEnergy() + particleContainer.GetTotalPotentialEnergy()).ToString("F2"), 10, 90, 20, Color.White);

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
        public const int Tps = 500;

        public const float FrameTime = 1f / Fps;
        public const float TickTime = 1f / Tps;
    }
}
