using System.Collections.Generic;
using UnityEngine;
using MyBox; 

[RequireComponent(typeof(MeshFilter))]
public class BernsteinCoaster : MonoBehaviour
{
    public Transform[] waypoints = null;
    [SerializeField, MinValue(0.005f), MaxValue(0.5f)] private float resolution = 0.01f;
    [Space (8)]
    [SerializeField, MustBeAssigned] private GameObject cart = null; 
    [SerializeField] private Vector3 cartOffest = new Vector3 (0f, 1f, 0f); 
    [SerializeField] private Vector3 cartRotationOffset = new Vector3 (0f, 0f, -90f);
    [SerializeField, PositiveValueOnly] private float speed = 1f;
    [Space (8)]
    [SerializeField, PositiveValueOnly, MaxValue(5f)] float height = 0.2f;
    [SerializeField, PositiveValueOnly, MaxValue(5f)] float width = 2f;

    private float t = 0f;

    private MeshFilter meshFilter = null;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = CreateBezierMesh(waypoints, height, width);
    }

    private void Update()
    {
        t += speed * Time.deltaTime;

        if (t > 1f)
            t -= 1f;

        Vector3 p0 = waypoints[0].position;
        Vector3 p1 = waypoints[1].position;
        Vector3 p2 = waypoints[2].position;
        Vector3 p3 = waypoints[3].position;

        Vector3 point = CalculateBezierPoint(t, p0, p1, p2, p3);
        Vector3 tangent = CalculateBezierTangent(t, p0, p1, p2, p3);

        cart.transform.position = point + cart.transform.rotation * cartOffest;
        cart.transform.rotation = Quaternion.LookRotation(tangent, Vector3.right) * Quaternion.Euler(cartRotationOffset);
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
            for (float t = 0; t <= 1; t += resolution)
            {
                Vector3 point = CalculateBezierPoint(t, controlPoints[i].position, controlPoints[i + 1].position, controlPoints[i + 2].position, controlPoints[i + 3].position);

                Vector3 tangent = CalculateBezierTangent(t, controlPoints[i].position, controlPoints[i + 1].position, controlPoints[i + 2].position, controlPoints[i + 3].position).normalized;

                Vector3 normal = Vector3.Cross(tangent, Vector3.right);

                int leftDown = vertices.Count;
                vertices.Add(point + normal * width - Vector3.right * (height / 2f)); 
                normals.Add(-normal); 

                int rightDown = vertices.Count;
                vertices.Add(point - normal * width - Vector3.right * (height / 2f)); 
                normals.Add(-normal); 

                int leftUp = vertices.Count;
                vertices.Add(point + normal * width + Vector3.right * (height / 2f)); 
                normals.Add(normal); 

                int rightUp = vertices.Count;
                vertices.Add(point - normal * width + Vector3.right * (height / 2f)); 
                normals.Add(normal); 

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
                    Vector3 sideNormal = Vector3.Cross(Vector3.right, tangent); 

                    triangles.Add(prevRightDown);
                    triangles.Add(rightUp);
                    triangles.Add(rightDown);
                    normals[rightDown] = sideNormal; 
                    normals[rightUp] = sideNormal; 

                    triangles.Add(prevRightDown);
                    triangles.Add(prevRightUp);
                    triangles.Add(rightUp);
                    normals[prevRightDown] = sideNormal; 
                    normals[prevRightUp] = sideNormal; 

                    triangles.Add(prevLeftDown);
                    triangles.Add(leftDown);
                    triangles.Add(leftUp);
                    normals[leftDown] = -sideNormal; 
                    normals[leftUp] = -sideNormal; 

                    triangles.Add(prevLeftDown);
                    triangles.Add(leftUp);
                    triangles.Add(prevLeftUp);
                    normals[prevLeftDown] = -sideNormal; 
                    normals[prevLeftUp] = -sideNormal; 
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
        float tt = t * t;
        float ttt = tt * t;

        // Bernstein polynomial form
        Vector3 p = Vector3.zero; 
        p += (-ttt + 3 * tt - 3 * t + 1) * p0; // (-t^3 + 3t^2 - 3t + 1) * p0
        p += (3 * ttt - 6 * tt + 3 * t) * p1; // (3t^3 - 6t^2 + 3t) * p1
        p += (-3 * ttt + 3 * tt) * p2; // (-3t^3 + 3t^2) * p2
        p += ttt * p3; // t^3 * p3

        return p;
    }

    private Vector3 CalculateBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float tt = t * t;

        // Bernstein polynomial form for the derivative
        Vector3 p = Vector3.zero;
        p += (-3 * tt + 6 * t - 3) * p0; // (-3t^2 + 6t - 3) * p0
        p += (9 * tt - 12 * t + 3) * p1; // (9t^2 - 12t + 3) * p1
        p += (-9 * tt + 6 * t) * p2; // (-9t^2 + 6t) * p2
        p += 3 * tt * p3; // 3t^2 * p3

        return p.normalized;
    }

    // Visualise with gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray; 

        if (waypoints != null)
        {
            for (int i = 0; i < waypoints.Length - 3; i += 4)
            {
                Vector3 p0 = waypoints[i].position;
                Vector3 p1 = waypoints[i + 1].position;
                Vector3 p2 = waypoints[i + 2].position;
                Vector3 p3 = waypoints[i + 3].position;

                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(p2, p3);

                for (float t = 0; t <= 1; t += resolution)
                {
                    Vector3 point = CalculateBezierPoint(t, p0, p1, p2, p3);
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }
    }
}
