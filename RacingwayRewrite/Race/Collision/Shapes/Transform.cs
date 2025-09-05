using System.Numerics;
using Dalamud.Bindings.ImGuizmo;
using MessagePack;

namespace RacingwayRewrite.Race.Collision.Shapes;

[MessagePackObject]
public class Transform(Vector3 position, Vector3 scale, Vector3 rotation)
{
    [Key(0)] public Vector3 Position = position;
    [Key(1)] public Vector3 Scale = scale;
    [Key(2)] public Vector3 Rotation = rotation;
    
    public Matrix4x4 GetTransformation()
    {        
        // var s = Matrix4x4.CreateScale(Scale);
        // var rY = Matrix4x4.CreateRotationY(Rotation.Y);
        // var rX = Matrix4x4.CreateRotationX(Rotation.X);
        // var rZ = Matrix4x4.CreateRotationZ(Rotation.Z);
        // var t = Matrix4x4.CreateTranslation(Position);
        //
        // return s * rY * rX * rZ * t;
        Matrix4x4 mat = Matrix4x4.Identity;
        ImGuizmo.RecomposeMatrixFromComponents(ref Position.X, ref Rotation.X, ref Scale.X, ref mat.M11);
        return mat;
    }
}
