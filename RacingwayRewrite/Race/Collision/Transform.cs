using System.Numerics;

namespace RacingwayRewrite.Race.Collision;

public class Transform 
{
    public Vector3 Position;
    public Vector3 Scale;
    public Vector3 Rotation;
    
    public Transform(Vector3 position, Vector3 scale, Vector3 rotation)
    {
        this.Position = position;
        this.Scale = scale;
        this.Rotation = rotation;
    }
    
    public Matrix4x4 GetTransformation()
    {
        Matrix4x4 s = Matrix4x4.CreateScale(Scale);
        Matrix4x4 r = Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
        Matrix4x4 t = Matrix4x4.CreateTranslation(Position);
        return s * r * t;
    }
}
