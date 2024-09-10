using System.Numerics;
using Titan.Input;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.ECS.Systems;

public struct Camera
{
    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;
    public Vector3 Forward;
    public Vector3 Right;

    public float Fov;
    public float AspectRatio;
    public float NearPlane;
    public float FarPlane;

    public Matrix4x4 WorldMatrix;
    public Matrix4x4 ProjectionMatrix;
    public Matrix4x4 ViewMatrix;
    public Matrix4x4 ViewProjectionMatrix;

    public static Camera Create()
    {
        return new Camera
        {
            Target = Vector3.Zero,
            Up = Vector3.UnitY,
            Forward = Vector3.UnitZ,
            Right = Vector3.UnitX,
            Fov = MathF.PI / 4,
            //AspectRatio = 
            NearPlane = 0.1f,
            FarPlane = 1000f,
            WorldMatrix = Matrix4x4.Identity,
        };
    }
}

[UnmanagedResource]
internal partial struct CameraSystem
{
    public Camera DefaultCamera;


    [System(SystemStage.Init)]
    public static void Startup(ref CameraSystem system, in Window window)
    {
        ref var camera = ref system.DefaultCamera;
        camera = Camera.Create();
        camera.Position = Vector3.UnitZ * -10 + Vector3.UnitY * 10;
        camera.AspectRatio = window.Width / (float)window.Height;
        camera.ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.Fov, camera.AspectRatio, camera.NearPlane, camera.FarPlane);
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        camera.ViewProjectionMatrix = Matrix4x4.Transpose(camera.ViewProjectionMatrix);
    }

    [System]
    public static void Update(ref CameraSystem system, in InputState inputState)
    {
        ref var camera = ref system.DefaultCamera;
        //camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;


        if (inputState.IsKeyDown(KeyCode.Down) || inputState.IsKeyDown(KeyCode.S))
        {
            camera.Position -= camera.Forward * 0.1f;
        }
        if (inputState.IsKeyDown(KeyCode.Up) || inputState.IsKeyDown(KeyCode.W))
        {
            camera.Position += camera.Forward * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.Left) || inputState.IsKeyDown(KeyCode.A))
        {
            camera.Position -= camera.Right * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.Right) || inputState.IsKeyDown(KeyCode.D))
        {
            camera.Position += camera.Right * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.V))
        {
            camera.Position += camera.Up * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.C))
        {
            camera.Position -= camera.Up * 0.1f;
        }
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        camera.ViewProjectionMatrix = Matrix4x4.Transpose(camera.ViewProjectionMatrix);
    }
}
