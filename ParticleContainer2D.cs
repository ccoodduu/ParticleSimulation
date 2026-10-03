using Raylib_cs;
using System.Numerics;

namespace ParticleSimulation;

class ParticleContainer2D
{
    List<Particle2D> particles;

    public Vector2 BoxSize { get; private set; }
    public readonly Random random;

    public ParticleContainer2D(int numParticles, Vector2 worldSize)
    {
        random = new Random();

        BoxSize = worldSize;

        particles = new List<Particle2D>();
        const float radius = 0.5f;
        var spawnArea = BoxSize - new Vector2(radius * 2);
        for (int i = 0; i < numParticles; i++)
        {
            var pos = new Vector2(radius + (float)random.NextDouble() * spawnArea.X, radius + (float)random.NextDouble() * spawnArea.Y);
            var velocty = random.NextUnitVector() * 10;
            particles.Add(new Particle2D(this, pos, velocty, radius));
        }
    }

    public void Update()
    {
        foreach (var particle in particles)
        {
            particle.Update();
        }
    }

    public void Draw()
    {
        var topLeft = this.WorldToScreenPos(new Vector2(0f));
        var bottomRight = this.WorldToScreenPos(BoxSize);
        var size = bottomRight - topLeft;

        Raylib.DrawRectangleLines(topLeft.X, topLeft.Y, size.X, size.Y, Color.White);

        foreach (var particle in particles)
        {
            particle.Draw();
        }
    }

    public Vector2Int WorldToScreenPos(Vector2 position)
    {
        Vector2 screenSize = new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());

        Vector2 centerOffset = (screenSize - new Vector2(WorldScale) * BoxSize) / 2;

        return (position * WorldScale + centerOffset).RoundToVector2Int();
    }

    public float WorldScale
    {
        get
        {
            Vector2 screenSize = new Vector2(Raylib.GetRenderWidth(), Raylib.GetRenderHeight());
            Vector2 boxScales = screenSize / BoxSize;

            return MathF.Min(boxScales.X, boxScales.Y);
        }
    }
}

