using System.Numerics;

namespace ParticleSimulation;

public struct SimulationParameters(int particleCount, Vector3 boxSize, float particleStartVelocity)
{
    public float TimePerStepPs = 0.005f;
    public Vector3 BoxSize = boxSize;
    public int ParticleCount = particleCount;
    public float LennardJonesCutoffMultiplier = 3f;
    public float ParticleStartVelocity = particleStartVelocity;
}
