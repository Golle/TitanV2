using System.Diagnostics;
using System.Numerics;

namespace Titan.ECS.Systems;


/// <summary>
/// The camera struct with all information to move a camera in any direction.
/// <remarks>Use Camera.Create() function to create a camera instance, it set's an ID that is used to determine which camera is active.</remarks>
/// </summary>
public struct Camera
{
    /// <summary>
    /// Clamp value to avoid camera flipping
    /// </summary>
    public const float PitchClampValue = 89.9f * (MathF.PI / 180.0f);
    public static readonly Vector3 DefaultForward = -Vector3.UnitZ;
    public static readonly Vector3 DefaultUp = Vector3.UnitY;
    public static readonly Vector3 DefaultRight = Vector3.UnitX;

    public int Id;
    private static int _nextId;

    // Position, direction etc
    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;
    public Vector3 Forward;
    public Vector3 Right;

    // The perspective
    public float FieldOfView;
    public float AspectRatio;
    public float NearPlane;
    public float FarPlane;

    // Cached matrixes, some are updated each frame.
    public Matrix4x4 RotationMatrix;
    public Matrix4x4 WorldMatrix;
    public Matrix4x4 ProjectionMatrix;
    public Matrix4x4 ViewMatrix;
    public Matrix4x4 ViewProjectionMatrix;
    public Matrix4x4 InverseViewProjectionMatrix;
    // This is what we use on the GPU side because of Row vs Column major
    public Matrix4x4 ViewProjectionMatrixTransposed;

    // Rotation, maybe change to something else?
    public float Pitch;
    public float Yaw;
    public float Roll;


    public static Camera CreateOrhorgraphic(int width, int height, float nearPlane = 0.1f, float farPlane = 1000f)
    {
        var camera = new Camera
        {
            Id = Interlocked.Increment(ref _nextId),
            Position = Vector3.Zero,
            Target = DefaultForward,
            Up = DefaultUp,
            Forward = -DefaultForward,
            Right = DefaultRight,
            FieldOfView = 0,
            NearPlane = nearPlane,
            FarPlane = farPlane,
            WorldMatrix = Matrix4x4.Identity,
            AspectRatio = width / (float)height
        };

        camera.ProjectionMatrix = Matrix4x4.CreateOrthographic(width, height, nearPlane, farPlane);
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        var inverseResult = Matrix4x4.Invert(camera.ViewProjectionMatrix, out camera.InverseViewProjectionMatrix);
        Debug.Assert(inverseResult);
        return camera;

    }
    public static Camera Create(int width, int height, float nearPlane = 0.1f, float farPlane = 1000f, float fieldOfView = MathF.PI / 4f)
    {
        var camera = new Camera
        {
            Id = Interlocked.Increment(ref _nextId),
            Position = Vector3.Zero,
            Target = DefaultForward,
            Up = DefaultUp,
            Forward = -DefaultForward,
            Right = DefaultRight,
            FieldOfView = fieldOfView,
            NearPlane = nearPlane,
            FarPlane = farPlane,
            WorldMatrix = Matrix4x4.Identity,
            AspectRatio = width / (float)height
        };

        camera.ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, camera.AspectRatio, camera.NearPlane, camera.FarPlane);
        camera.ViewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.Up);
        camera.ViewProjectionMatrix = camera.WorldMatrix * camera.ViewMatrix * camera.ProjectionMatrix;
        var inverseResult = Matrix4x4.Invert(camera.ViewProjectionMatrix, out camera.InverseViewProjectionMatrix);
        Debug.Assert(inverseResult);
        return camera;
    }


    public void MoveTo(in Vector3 position)
        => Position = position;
    public void Move(in Vector3 distance)
        => Position += distance;

    public void MoveTowardsTarget(in Vector3 distance)
    {
        Position += distance.X * Right;
        Position += distance.Z * Forward;
        Position += distance.Y * Up;
    }

    public void SetTarget(in Vector3 direction)
        => Forward = Target = Vector3.Normalize(direction);

    public void SetRotation(float yaw, float pitch, float roll)
    {
        Yaw = yaw;
        Pitch = pitch;
        Roll = roll;
        RotationMatrix = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
    }

    public void SetRotation(in Quaternion quaternion)
        => RotationMatrix = Matrix4x4.CreateFromQuaternion(quaternion);

    public void SetRotation(in Vector3 axis, float angle)
        => RotationMatrix = Matrix4x4.CreateFromAxisAngle(axis, angle);

    /// <summary>
    /// Create a Ray from the specified screen coordinates. Default is in the center of the screen.
    /// <remarks>Use NDC to create the ray.</remarks>
    /// </summary>
    /// <param name="screenCoordinates">Screen coordinates in NDC, 0,0 is the default.</param>
    /// <returns>A <see cref="Ray"/> with Origin and a Direction</returns>
    public readonly Ray CreateRay(in Vector2 screenCoordinates = default)
    {
        var x = screenCoordinates.X;
        var y = screenCoordinates.Y;

        // Clip-space coordinates for near and far planes
        var clipSpaceNear = new Vector4(x, y, -1.0f, 1.0f);
        var clipSpaceFar = new Vector4(x, y, 1000.0f, 1.0f);
        // Transform clip-space to world-space
        //var inverseViewProjection = Matrix4x4.Invert(camera.ViewProjectionMatrix, out var result) ? result : Matrix4x4.Identity;
        var worldSpaceNear = Vector4.Transform(clipSpaceNear, InverseViewProjectionMatrix);
        var worldSpaceFar = Vector4.Transform(clipSpaceFar, InverseViewProjectionMatrix);

        // Convert to homogeneous coordinates
        worldSpaceNear /= worldSpaceNear.W;
        worldSpaceFar /= worldSpaceFar.W;

        // Calculate ray origin and direction
        var origin = new Vector3(worldSpaceNear.X, worldSpaceNear.Y, worldSpaceNear.Z);
        var direction = -Vector3.Normalize(new Vector3(
            worldSpaceFar.X - worldSpaceNear.X,
            worldSpaceFar.Y - worldSpaceNear.Y,
            worldSpaceFar.Z - worldSpaceNear.Z));

        return new(origin, direction);
    }
}
