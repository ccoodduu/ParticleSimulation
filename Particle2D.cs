using Raylib_cs;
using System.Numerics;

namespace ParticleSimulation;

class Particle2D
{
    float radius;
    float Mass => radius * radius;
    Vector2 position;
    Vector2 velocity;
    Color color;
    ParticleContainer2D container;

    public Particle2D(ParticleContainer2D container, Vector2 position, Vector2 velocity, float radius)
    {
        this.container = container;
        this.position = position;
        this.velocity = velocity;
        this.radius = radius;
        this.color = Color.White;
    }

    public void Update()
    {
        position += velocity * Globals.SimulationStepTimePs;

        if (position.X < radius)
        {
            velocity *= new Vector2(-1, 1);
        }
        if (position.X > container.BoxSize.X - radius)
        {
            velocity *= new Vector2(-1, 1);
        }
        if (position.Y < radius)
        {
            velocity *= new Vector2(1, -1);
        }
        if (position.Y > container.BoxSize.Y - radius)
        {
            velocity *= new Vector2(1, -1);
        }
    }

    public void Draw()
    {
        var screenPos = container.WorldToScreenPos(position);
        Raylib.DrawCircle(screenPos.X, screenPos.Y, radius * container.WorldScale, color);
    }
}

