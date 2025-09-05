using System.Numerics;
using MessagePack;

namespace RacingwayRewrite.Race.Collision.Shapes;

[MessagePackObject]
[Union(0, typeof(Cube))]
public abstract class Shape
{
    [Key(0)] public Transform Transform { get; set; }
    protected abstract Vector3[] Vertices { get; }

    public Shape(Vector3 position, Vector3 scale, Vector3 rotation)
    {
        Transform = new(position, scale, rotation);
    }

    public Shape(Transform transform)
    {
        Transform = transform;
    }
    
    public Vector3[] GetTransformedVerts()
    {
        Vector3[] transformed = new Vector3[Vertices.Length];
        Matrix4x4 matrix = Transform.GetTransformation();
        
        // Transform vertices
        for (int i = 0; i < Vertices.Length; i++)
        {
            transformed[i] = Vector3.Transform(Vertices[i], matrix);
        }
        
        return transformed;
    }
    
    public abstract bool PointInside(Vector3 point);
}
