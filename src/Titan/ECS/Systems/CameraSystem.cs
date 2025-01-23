using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Titan.Configurations;
using Titan.Core.Memory;
using Titan.Input;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.ECS.Systems;

public record CameraStateConfig : IConfiguration, IDefault<CameraStateConfig>, IPersistable<CameraStateConfig>
{
    public Vector3 Position { get; init; }
    public Vector3 Target { get; init; }
    public float Yaw { get; init; }
    public float Pitch { get; init; }

    public static CameraStateConfig Default => new()
    {
        Target = Camera.DefaultForward,
        Position = Vector3.Zero
    };
    public static JsonTypeInfo<CameraStateConfig> TypeInfo => TitanSerializationContext.Default.CameraStateConfig;
    public static string Filename => "camera.conf";
}


public record struct Ray(Vector3 Origin, Vector3 Direction);

[UnmanagedResource]
public unsafe partial struct CameraSystem
{
    public readonly ref readonly Camera GetCurrentCamera() => ref *CurrentCamera;

    private Camera DefaultCamera;
    private Camera* CurrentCamera;

    [System(SystemStage.Init)]
    internal static void Startup(ref CameraSystem system, in Window window, IConfigurationManager configurationManager)
    {
        ref var camera = ref system.DefaultCamera;
        camera = Camera.Create(window.Width, window.Height);

        var config = configurationManager.GetConfigOrDefault<CameraStateConfig>();

        camera.MoveTo(config.Position);
        camera.SetTarget(config.Target);
        camera.SetRotation(config.Yaw, config.Pitch, 0);
        
        system.CurrentCamera = MemoryUtils.AsPointer(system.DefaultCamera);
    }


    [System]
    internal static void Update(ref CameraSystem system, in InputState inputState, in Window window)
    {
        ref var camera = ref *system.CurrentCamera;

        //NOTE(Jens): This is not a very nice way to do it, but we need to update movement after all other properties.
        Vector3 movement = default;
        var isInFreeLookMode = camera.Id == system.DefaultCamera.Id;
        if (isInFreeLookMode)
        {
            movement = UpdateFreelookCamera(ref camera, inputState, window);
        }
        camera.RotationMatrix = Matrix4x4.CreateFromYawPitchRoll(camera.Yaw, camera.Pitch, camera.Roll);
        //var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(camera.Yaw, camera.Pitch, camera.Roll);

        camera.Forward = Vector3.Transform(Camera.DefaultForward, camera.RotationMatrix);
        
        
        var rotatationYMatrix = Matrix4x4.CreateRotationY(camera.Yaw);
        camera.Right = Vector3.Transform(Camera.DefaultRight, rotatationYMatrix);
        camera.Up = Vector3.Transform(Camera.DefaultUp, rotatationYMatrix);
        camera.Target = Vector3.Normalize(camera.Forward);

        if (isInFreeLookMode)
        {
            camera.MoveTowardsTarget(movement);
        }

        camera.Target = camera.Position + camera.Target;
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);

        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        camera.ViewProjectionMatrixTransposed = Matrix4x4.Transpose(camera.ViewProjectionMatrix);

        var inverseResult = Matrix4x4.Invert(camera.ViewProjectionMatrix, out camera.InverseViewProjectionMatrix);
        Debug.Assert(inverseResult, "Failed to invert the View Projection Matrix. Why?");
    }

    private static Vector3 UpdateFreelookCamera(ref Camera camera, in InputState inputState, in Window window)
    {
        const float mouseLookMultiplier = 0.002f;
        var speed = inputState.IsKeyDown(KeyCode.Shift) ? 0.2f : 0.1f;

        Vector3 movement = default;

        if (inputState.IsButtonPressed(MouseButton.Right))
        {
            // we hide the cursor (and lock it in place) when moving the camera
            window.ShowCursor(false);
        }

        if (inputState.IsButtonReleased(MouseButton.Right))
        {
            window.ShowCursor(true);
        }

        if (inputState.IsKeyDown(KeyCode.Down) || inputState.IsKeyDown(KeyCode.S))
        {
            movement.Z += -speed;
        }
        if (inputState.IsKeyDown(KeyCode.Up) || inputState.IsKeyDown(KeyCode.W))
        {
            movement.Z -= -speed;
        }

        if (inputState.IsKeyDown(KeyCode.Left) || inputState.IsKeyDown(KeyCode.A))
        {
            movement.X -= speed;
        }

        if (inputState.IsKeyDown(KeyCode.Right) || inputState.IsKeyDown(KeyCode.D))
        {
            movement.X += speed;
        }

        if (inputState.IsKeyDown(KeyCode.V))
        {
            movement.Y += speed;
        }

        if (inputState.IsKeyDown(KeyCode.C))
        {
            movement.Y -= speed;
        }

        if (inputState.IsButtonDown(MouseButton.Right))
        {
            camera.Pitch += inputState.MousePositionDelta.Y * mouseLookMultiplier;
            camera.Pitch = Math.Clamp(camera.Pitch, -Camera.PitchClampValue, Camera.PitchClampValue);

            camera.Yaw += inputState.MousePositionDelta.X * mouseLookMultiplier;

        }

        return movement;
    }

    [System(SystemStage.Shutdown)]
    internal static void Shutdown(in CameraSystem cameraSystem, IConfigurationManager configurationManager)
    {
        ref readonly var camera = ref cameraSystem.DefaultCamera;
        configurationManager.UpdateConfig(new CameraStateConfig
        {
            Pitch = camera.Pitch,
            Position = camera.Position,
            Target = camera.Target,
            Yaw = camera.Yaw
        });
    }

    public readonly void SetFreeLookCamera()
    {
        //TODO(Jens): This is a dirty hack to support swapping camera. I want it to work in some other way in the future.
        Unsafe.AsRef(in this).CurrentCamera = MemoryUtils.AsPointer(DefaultCamera);
    }
    public readonly void SetCamera(ref Camera camera)
    {
        //TODO(Jens): This is a dirty hack to support swapping camera. I want it to work in some other way in the future.
        Unsafe.AsRef(in this).CurrentCamera = MemoryUtils.AsPointer(camera);
    }
}
