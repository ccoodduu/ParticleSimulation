using Raylib_cs;
using System.Numerics;

namespace ParticleSimulation;

class Renderer3D
{
    private Camera3D camera;
    public Camera3D Camera => camera;
    private readonly ParticleSimulation3D simulation;

    public Vector2 cameraAngle;
    public float cameraDistance;

    private Vector3 Center => simulation.BoxSize / 2;

    private Mesh particleBillboardTriangleMesh;
    private Shader particleShader;
    private Material particleMaterial;
    private readonly Matrix4x4[] transforms;

    public Renderer3D(ParticleSimulation3D simulation)
    {
        this.simulation = simulation;

        cameraDistance = MathF.Max(simulation.BoxSize.X, MathF.Max(simulation.BoxSize.Y, simulation.BoxSize.Z)) * 2f;
        cameraAngle = new Vector2(MathF.PI / 4, 0.615479709f);

        var cameraPosition = GetCameraPosition(cameraAngle.X, cameraAngle.Y, cameraDistance);

        camera = new Camera3D(position: cameraPosition, target: Center, up: Vector3.UnitY, fovY: 45f, projection: CameraProjection.Perspective);

        particleBillboardTriangleMesh = Raylib.GenMeshPoly(3, 2f);
        transforms = new Matrix4x4[simulation.Particles.Count];

        particleMaterial = Raylib.LoadMaterialDefault();

        particleShader = Raylib.LoadShader($"resources/shaders/particle.vs", $"resources/shaders/particle.fs");
        unsafe
        {
            particleShader.Locs[(int)ShaderLocationIndex.MatrixModel] = Raylib.GetShaderLocationAttrib(particleShader, "instanceTransform");
        }

        particleMaterial.Shader = particleShader;
        unsafe
        {
            particleMaterial.Maps[(int)MaterialMapIndex.Diffuse].Color = Color.Red;
        }
    }

    private Vector3 GetCameraPosition(float longitude, float latitude, float distance)
    {
        var x = distance * MathF.Cos(latitude) * MathF.Sin(longitude);
        var y = distance * MathF.Sin(latitude);
        var z = distance * MathF.Cos(latitude) * MathF.Cos(longitude);

        return new Vector3(x, y, z) + Center;
    }

    public void Draw()
    {
        camera.Position = GetCameraPosition(cameraAngle.X, cameraAngle.Y, cameraDistance);

        Raylib.BeginMode3D(Camera);
        // Draw the bounding box
        Raylib.DrawCubeWires(simulation.BoxSize / 2, simulation.BoxSize.X, simulation.BoxSize.Y, simulation.BoxSize.Z, Color.White);

        // Draw particles

        for (int i = 0; i < simulation.Particles.Count; i++)
        {
            var forward = Vector3.Normalize(simulation.Particles[i].Position - camera.Position);
            var matrix = Matrix4x4.CreateScale(simulation.ParticleParameters.Radius) *
                Matrix4x4.CreateRotationX(-MathF.PI / 2) *
                Matrix4x4.CreateBillboard(simulation.Particles[i].Position, camera.Position, camera.Up, forward);

            transforms[i] = Matrix4x4.Transpose(matrix);
        }

        unsafe
        {
            fixed (Matrix4x4* modelMatricesPtr = transforms)
            {
                Raylib.DrawMeshInstanced(particleBillboardTriangleMesh, particleMaterial, modelMatricesPtr, simulation.Particles.Count);
            }
        }

        Raylib.EndMode3D();
    }
}
