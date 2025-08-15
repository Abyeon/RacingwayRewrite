using System.Numerics;

namespace RacingwayRewrite.Race.Collision;

public class Cube
{
    public Vector3 Position;
    public Vector3 Scale;
    public Vector3 Rotation;
    
    public readonly Vector3[] Vertices =
    [
        new Vector3(-1, 0, -1),
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 0, -1),
        new Vector3(-1, 1, -1),
        new Vector3(-1, 1, 1),
        new Vector3(1, 1, 1),
        new Vector3(1, 1, -1)
    ];

    public Cube(Vector3 position, Vector3 scale, Vector3 rotation)
    {
        Position = position;
        Scale = scale;
        Rotation = rotation;
    }
    
    public Vector3[] TransformedVerts()
    {
        Vector3[] transformed = new Vector3[Vertices.Length];
        
        Matrix4x4 scale = Matrix4x4.CreateScale(Scale);
        Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
        Matrix4x4 translation = Matrix4x4.CreateTranslation(Position);
        Matrix4x4 transformation = scale * rotation * translation;
        
        // Transform vertices
        for (int i = 0; i < Vertices.Length; i++)
        {
            transformed[i] = Vector3.Transform(Vertices[i], transformation);
        }
        
        return transformed;
    }
}
