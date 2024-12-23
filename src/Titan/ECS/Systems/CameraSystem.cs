using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using Titan.Configurations;
using Titan.Input;
using Titan.Resources;
using Titan.Systems;
using Titan.UI;
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

public struct Camera
{
    /// <summary>
    /// Clamp value to avoid camera flipping
    /// </summary>
    public const float PitchClampValue = 89.9f* (MathF.PI / 180.0f);
    public static readonly Vector3 DefaultForward = -Vector3.UnitZ;
    public static readonly Vector3 DefaultUp = Vector3.UnitY;
    public static readonly Vector3 DefaultRight = Vector3.UnitX;
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
    public Matrix4x4 InverseViewProjectionMatrix;
    // This is what we use on the GPU side because of Row vs Column major
    public Matrix4x4 ViewProjectionMatrixTransposed;

    public float Pitch;
    public float Yaw;
    public float Roll;

    public static Camera Create()
    {
        return new Camera
        {
            Position = Vector3.Zero,
            Target = DefaultForward,
            Up = DefaultUp,
            Forward = -DefaultForward,
            Right = DefaultRight,
            Fov = MathF.PI / 4,
            NearPlane = 0.1f,
            FarPlane = 1000f,
            WorldMatrix = Matrix4x4.Identity,
        };
    }
}

[UnmanagedResource]
public partial struct CameraSystem
{
    public Camera DefaultCamera;

    [System(SystemStage.Init)]
    internal static void Startup(ref CameraSystem system, in Window window, IConfigurationManager configurationManager)
    {
        ref var camera = ref system.DefaultCamera;
        camera = Camera.Create();
        
        var config = configurationManager.GetConfigOrDefault<CameraStateConfig>();

        camera.Position = config.Position;
        camera.Target = config.Target;
        camera.Pitch = config.Pitch;
        camera.Yaw = config.Yaw;
        
        //camera.Position = Vector3.UnitZ * -10 + Vector3.UnitY * 10;
        camera.AspectRatio = window.Width / (float)window.Height;
        //camera.ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.Fov, camera.AspectRatio, camera.NearPlane, camera.FarPlane);
        camera.ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.Fov, camera.AspectRatio, camera.NearPlane, camera.FarPlane);

        //camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        camera.ViewProjectionMatrixTransposed = Matrix4x4.Transpose(camera.ViewProjectionMatrix);
        var inverseResult = Matrix4x4.Invert(camera.ViewProjectionMatrix, out camera.InverseViewProjectionMatrix);
        Debug.Assert(inverseResult, "Failed to invert the View Projection Matrix. Why?");
    }

    [System]
    internal static void Update(ref CameraSystem system, in InputState inputState, UIManager uiManager, in Window window)
    {
        const float mouseLookMultiplier = 0.002f;
        var speed = inputState.IsKeyDown(KeyCode.Shift) ? 0.2f : 0.1f;

        ref var camera = ref system.DefaultCamera;

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

        var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(camera.Yaw, camera.Pitch, camera.Roll);
        camera.Target = Vector3.Normalize(Vector3.Transform(Camera.DefaultForward, rotationMatrix));

        var rotatationYMatrix = Matrix4x4.CreateRotationY(camera.Yaw);
        camera.Right = Vector3.Transform(Camera.DefaultRight, rotatationYMatrix);
        camera.Up = Vector3.Transform(Camera.DefaultUp, rotatationYMatrix);
        camera.Forward = Vector3.Transform(Camera.DefaultForward, rotationMatrix);

        camera.Position += movement.X * camera.Right;
        camera.Position += movement.Z * camera.Forward;
        camera.Position += movement.Y * camera.Up;
        camera.Target = camera.Position + camera.Target;
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);

        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        camera.ViewProjectionMatrixTransposed = Matrix4x4.Transpose(camera.ViewProjectionMatrix);
        
        var inverseResult = Matrix4x4.Invert(camera.ViewProjectionMatrix, out camera.InverseViewProjectionMatrix);
        Debug.Assert(inverseResult, "Failed to invert the View Projection Matrix. Why?");
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
}
