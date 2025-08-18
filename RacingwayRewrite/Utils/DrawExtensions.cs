using System.Numerics;
using Pictomancy;
using RacingwayRewrite.Race.Collision;

namespace RacingwayRewrite.Utils;

public static class DrawExtensions
{
    public static void AddCube(this PctDrawList drawList, Cube cube, uint col)
    {
        // Arrays of indices for each face
        int[,] faces =
        {
            { 0, 1, 2, 3 }, // bottom face
            { 7, 6, 5, 4 }  // top face
        };

        // Get transformed vertices of cube
        Vector3[] points = cube.GetTransformedVerts();
        
        drawList.AddQuad(points[faces[0, 0]], points[faces[0, 1]], points[faces[0, 2]], points[faces[0, 3]], col);
        
        drawList.AddPathLine(points[faces[0, 0]], points[faces[1, 3]], col);
        drawList.AddPathLine(points[faces[0, 1]], points[faces[1, 2]], col);
        drawList.AddPathLine(points[faces[0, 2]], points[faces[1, 1]], col);
        drawList.AddPathLine(points[faces[0, 3]], points[faces[1, 0]], col);
        
        drawList.AddQuad(points[faces[1, 0]], points[faces[1, 1]], points[faces[1, 2]], points[faces[1, 3]], col);
    }
    
    public static void AddCubeFilled(this PctDrawList drawList, Cube cube, uint col)
    {
        // Arrays of indices for each face
        int[,] faces =
        {
            { 0, 1, 2, 3 }, // bottom face
            { 7, 6, 5, 4 }, // top face
            { 3, 7, 4, 0 }, // front face
            { 1, 5, 6, 2 }, // back face
            { 0, 4, 5, 1 }, // left face
            { 2, 6, 7, 3 }  // right face
        };

        // Get transformed vertices of cube
        Vector3[] points = cube.GetTransformedVerts();

        for (int i = 0; i < faces.GetLength(0); i++)
        {
            Vector3 p1 = points[faces[i, 0]];
            Vector3 p2 = points[faces[i, 1]];
            Vector3 p3 = points[faces[i, 2]];
            Vector3 p4 = points[faces[i, 3]];

            drawList.AddQuadFilled(p1, p2, p3, p4, col);
        }
    }
    
    public static void AddPathLine(this PctDrawList drawList, Vector3 p1, Vector3 p2, uint col)
    {
        drawList.PathLineTo(p1);
        drawList.PathLineTo(p2);
        drawList.PathStroke(col, PctStrokeFlags.Closed);
    }
}
