using System.Collections.Generic;
using UnityEngine;

public class Coaster : MonoBehaviour
{
    public Transform[] waypoints;
    public float width = 1f;
    public float height = 1f;

    private MeshFilter meshFilter;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = CreateBezierMesh(waypoints, width, height);
    }

    private Mesh CreateBezierMesh(Transform[] controlPoints, float width, float height)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();  // The list to store the calculated normals

        int prevLeftDown = -1;
        int prevRightDown = -1;
        int prevLeftUp = -1;
        int prevRightUp = -1;

        for (int i = 0; i < controlPoints.Length - 3; i += 4)
        {
            for (float t = 0; t <= 1; t += 0.01f)
            {
                Vector3 point = CalculateBezierPoint(t, controlPoints[i].position, controlPoints[i + 1].position, controlPoints[i + 2].position, controlPoints[i + 3].position);

                Vector3 tangent = CalculateBezierTangent(t, controlPoints[i].position, controlPoints[i + 1].position, controlPoints[i + 2].position, controlPoints[i + 3].position).normalized;

                Vector3 normal = Vector3.Cross(tangent, Vector3.up);

                // Create vertices for the bottom rectangle
                int leftDown = vertices.Count;
                vertices.Add(point + normal * width / 2); // Bottom-left
                normals.Add(-normal); // Normal for bottom-left

                int rightDown = vertices.Count;
                vertices.Add(point - normal * width / 2); // Bottom-right
                normals.Add(-normal); // Normal for bottom-right

                // Create vertices for the top rectangle
                int leftUp = vertices.Count;
                vertices.Add(point + normal * width / 2 + Vector3.up * height); // Top-left
                normals.Add(normal); // Normal for top-left

                int rightUp = vertices.Count;
                vertices.Add(point - normal * width / 2 + Vector3.up * height); // Top-right
                normals.Add(normal); // Normal for top-right

                // Link the triangles with the previous segment
                if(prevLeftDown != -1)
                {
                    // Bottom
                    triangles.Add(prevLeftDown);
                    triangles.Add(rightDown);
                    triangles.Add(leftDown);

                    triangles.Add(prevLeftDown);
                    triangles.Add(prevRightDown);
                    triangles.Add(rightDown);

                    // Top
                    triangles.Add(prevLeftUp);
                    triangles.Add(leftUp);
                    triangles.Add(rightUp);

                    triangles.Add(prevLeftUp);
                    triangles.Add(rightUp);
                    triangles.Add(prevRightUp);

                    // Sides
                    Vector3 sideNormal = Vector3.Cross(Vector3.up, tangent); // Normal for sides

                    triangles.Add(prevRightDown);
                    triangles.Add(rightUp);
                    triangles.Add(rightDown);
                    normals[rightDown] = sideNormal; // Normal for right side bottom
                    normals[rightUp] = sideNormal; // Normal for right side top

                    triangles.Add(prevRightDown);
                    triangles.Add(prevRightUp);
                    triangles.Add(rightUp);
                    normals[prevRightDown] = sideNormal; // Normal for right side bottom
                    normals[prevRightUp] = sideNormal; // Normal for right side top

                    triangles.Add(prevLeftDown);
                    triangles.Add(leftDown);
                    triangles.Add(leftUp);
                    normals[leftDown] = -sideNormal; // Normal for left side bottom
                    normals[leftUp] = -sideNormal; // Normal for left side top

                    triangles.Add(prevLeftDown);
                    triangles.Add(leftUp);
                    triangles.Add(prevLeftUp);
                    normals[prevLeftDown] = -sideNormal; // Normal for left side bottom
                    normals[prevLeftUp] = -sideNormal; // Normal for left side top
                }

                prevLeftDown = leftDown;
                prevRightDown = rightDown;
                prevLeftUp = leftUp;
                prevRightUp = rightUp;
            }

            // Close the start of the track
            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(1);

            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(3);

            // Close the end of the track
            int vCount = vertices.Count;
            triangles.Add(vCount - 4);
            triangles.Add(vCount - 3);
            triangles.Add(vCount - 2);

            triangles.Add(vCount - 2);
            triangles.Add(vCount - 3);
            triangles.Add(vCount - 1);
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray(); // Assign the calculated normals to the mesh

        return mesh;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t) ^ 3  * p0
        p += 3 * uu * t * p1; // 3 * (1 - t)^2 * t * p1
        p += 3 * u * tt * p2; // 3 * (1 - t) * t^2 * p2
        p += ttt * p3; // t^3 * p3

        return p;
    }

    private Vector3 CalculateBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = -3 * uu * p0; // -3 * (1-t)^2 * p0
        p += 3 * (3 * tt - 4 * t + 1) * p1; // 3 * (3t^2 - 4t + 1) * p1
        p += 3 * (2 * t - 3 * tt) * p2; // 3 * (2t - 3t^2) * p2
        p += 3 * tt * p3; // 3t^2 * p3

        return p.normalized;
    }
}
