using System.Numerics;
using MessagePack;

namespace RacingwayRewrite.Race.Collision.Shapes;

[MessagePackObject]
public class Cube : Shape
{
    protected override Vector3[] Vertices { get; } =
    [
        new (-1, 0, -1),
        new (-1, 0, 1),
        new (1, 0, 1),
        new (1, 0, -1),
        new (-1, 1, -1),
        new (-1, 1, 1),
        new (1, 1, 1),
        new (1, 1, -1)
    ];
    
    public Cube(Vector3 position, Vector3 scale, Vector3 rotation) : base(position, scale, rotation)
    {
        Transform = new(position, scale, rotation);
    }

    public Cube(Transform transform) : base(transform)
    {
        Transform = transform;
    }

    public override bool PointInside(Vector3 point)
    {
        // Get the inverse transformation of the cube
        if (!Matrix4x4.Invert(Transform.GetTransformation(), out var inverse))
        {
            Plugin.Chat.Error(inverse.ToString());
            return false;
        }
        
        // Transform the point
        Vector3 transformed = Vector3.Transform(point, inverse);
        
        // Do AABB check against base vertices
        return transformed.X >= Vertices[0].X &&
               transformed.X <= Vertices[6].X &&
               transformed.Y >= Vertices[0].Y &&
               transformed.Y <= Vertices[6].Y &&
               transformed.Z >= Vertices[0].Z && 
               transformed.Z <= Vertices[6].Z;
    }
}
