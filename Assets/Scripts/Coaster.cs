using System.Collections.Generic;
using UnityEngine;
using MyBox; 

[RequireComponent(typeof(MeshFilter))]
public class Coaster : MonoBehaviour
{
    public Transform[] waypoints = null;
    [SerializeField, MinValue(0.005f), MaxValue(0.5f)] private float resolution = 0.01f;
    [Space(8)]
    [SerializeField, MustBeAssigned] private GameObject cart = null; 
    [SerializeField] private Vector3 cartOffest = new Vector3 (0f, 1f, 0f); 
    [SerializeField] private Vector3 cartRotationOffset = new Vector3 (0f, 0f, -90f);
    [SerializeField, PositiveValueOnly] private float speed = 1f;
    [Space(8)]
    [SerializeField, PositiveValueOnly, MaxValue(5f)] float height = 0.3f;
    [SerializeField, PositiveValueOnly, MaxValue(5f)] float width = 2f;

    private int currWaypointIndex = 0;
    private float t = 0f;

    private MeshFilter meshFilter = null;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = CreateBezierMesh(GetWaypointPositions(waypoints), height, width);
    }

    private void Update()
    {
        t += speed * Time.deltaTime;

        if (t > 1f)
        {
            t -= 1f;
            currWaypointIndex += 4;

            if (currWaypointIndex >= waypoints.Length - 3)
            {
                currWaypointIndex = 0;
                cart.transform.position = waypoints[0].position + cartOffest;
                cart.transform.rotation = Quaternion.Euler(cartRotationOffset);
            }
        }

        Vector3 point = DeCasteljau(GetWaypointPositions(waypoints), t);
        Vector3 tangent = CalculateBezierTangent(GetWaypointPositions(waypoints), t).normalized;

        cart.transform.rotation = Quaternion.Euler(cartRotationOffset) * Quaternion.LookRotation(tangent, Vector3.up);
        cart.transform.position = point + cart.transform.rotation * cartOffest;
    }

    private Mesh CreateBezierMesh(Vector3[] controlPoints, float width, float height)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();  // The list to store the calculated normals

        int prevLeftDown = -1;
        int prevRightDown = -1;
        int prevLeftUp = -1;
        int prevRightUp = -1;
    
        for (float t = 0; t <= 1; t += resolution)
        {
            Vector3 point = DeCasteljau(controlPoints, t);

            Vector3 tangent = CalculateBezierTangent(controlPoints, t).normalized;

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

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray(); // Assign the calculated normals to the mesh

        return mesh;
    }

    private Vector3 DeCasteljau(Vector3[] controlPoints, float t)
    {
        if (controlPoints.Length == 1) return controlPoints[0];

        Vector3[] newPoints = new Vector3[controlPoints.Length - 1];
        for (int i = 0; i < newPoints.Length; i++)
        {
            newPoints[i] = Vector3.Lerp(controlPoints[i], controlPoints[i + 1], t);
        }

        return DeCasteljau(newPoints, t);
    }

    private Vector3 CalculateBezierTangent(Vector3[] controlPoints, float t)
    {
        int n = controlPoints.Length - 1; // The degree of the bezier curve
        Vector3 tangent = Vector3.zero;

        for (int i = 0; i < n; i++)
        {
            Vector3 term = n * (controlPoints[i + 1] - controlPoints[i]) * Mathf.Pow(1 - t, n - i - 1) * Mathf.Pow(t, i);
            tangent += term;
        }

        return tangent.normalized;
    }

    private Vector3[] GetWaypointPositions (Transform[] waypoints)
    {
        Vector3[] positions = new Vector3[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
            positions[i] = waypoints[i].position;
        return positions;
    }

    // Visualize with gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        if (waypoints != null)
        {
            for (int i = 0; i < waypoints.Length - 1; i += 1)
            {
                for (float t = 0; t <= 1; t += resolution)
                {
                    Vector3 point = DeCasteljau(GetWaypointPositions(waypoints), t);
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }
    }
}