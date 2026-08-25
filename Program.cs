using System.Numerics;
using Raylib_cs;

const int screenWidth = 1280;
const int screenHeight = 720;

Raylib.InitWindow(screenWidth, screenHeight, "Particle Simulation");
Raylib.SetTargetFPS(60);

var random = new Random();
var particles = new List<Particle>();

for (int i = 0; i < 200; i++)
{
    particles.Add(new Particle(
        new Vector2(random.Next(screenWidth), random.Next(screenHeight)),
        new Vector2(random.Next(-100, 100), random.Next(-100, 100))));
}

while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();

    foreach (var p in particles)
    {
        p.Position += p.Velocity * dt;

        if (p.Position.X < 0 || p.Position.X > screenWidth) p.Velocity.X *= -1;
        if (p.Position.Y < 0 || p.Position.Y > screenHeight) p.Velocity.Y *= -1;
    }

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    foreach (var p in particles)
    {
        Raylib.DrawCircleV(p.Position, 3, Color.SkyBlue);
    }

    Raylib.DrawFPS(10, 10);
    Raylib.EndDrawing();
}

Raylib.CloseWindow();

class Particle(Vector2 position, Vector2 velocity)
{
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
}
