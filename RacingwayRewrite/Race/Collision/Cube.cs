﻿using System.Numerics;

namespace RacingwayRewrite.Race.Collision;

public class Cube(Vector3 position, Vector3 scale, Vector3 rotation) : Shape(position, scale, rotation)
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

    public override bool PointInside(Vector3 point)
    {
        // Get the inverse transformation of the cube
        Matrix4x4 inverse = Transform.GetInverseTransformation();
        
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
