
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Common;

public class Camera
{
    public Vector3D<float> camPos = new Vector3D<float>(0, 0, 0f);
    public float yaw = 0, pitch = 0;
    public float nearPlane = 0.1f;
    public float farPlane = 100f;
    public float fieldOfView = 45f;
    public enum ProjectionMode
    {
        Orthographic,
        Perspective,
    }
    public ProjectionMode projection = ProjectionMode.Perspective;
    public Vector3D<float> Forward => Vector3D.Transform(new Vector3D<float>(0, 0, 1), Quaternion<float>.CreateFromYawPitchRoll(float.DegreesToRadians(yaw), float.DegreesToRadians(pitch), 0));
    public Vector3D<float> Target => camPos + Forward;
    public Vector3D<float> Direction => Vector3D.Normalize(-Forward);
    Vector3D<float> WorldUp => new(0, 1, 0);
    public Vector3D<float> Right => Vector3D.Normalize(Vector3D.Cross(WorldUp, Direction));
    public Vector3D<float> Up => Vector3D.Normalize(Vector3D.Cross(Direction, Right));
    public Vector3D<float> Left => -Right;
    public Vector3D<float> Backward => Direction;
    public Matrix4X4<float> GetViewMatrix()
    {
        return Matrix4X4.CreateLookAt(camPos, Target, Up);
    }

    public Matrix4X4<float> GetProjectionMatrix(float width, float height)
    {
        if (projection == ProjectionMode.Orthographic)
            return Matrix4X4.CreateOrthographic(width, height, nearPlane, farPlane);
        else if (projection == ProjectionMode.Perspective)
            return Matrix4X4.CreatePerspectiveFieldOfView(float.DegreesToRadians(
                            fieldOfView
                        ), width / height,
            nearPlane, farPlane);
        return Matrix4X4.CreatePerspectiveFieldOfView(float.DegreesToRadians(
                            fieldOfView
                        ), width / height,
            nearPlane, farPlane);
    }

    public Matrix4X4<float> GetVPMatrix(float width, float height)
    {
        return GetViewMatrix() * GetProjectionMatrix(width, height);
    }
}