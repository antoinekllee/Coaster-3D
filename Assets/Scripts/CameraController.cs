using UnityEngine;
using MyBox;
using DG.Tweening;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField, PositiveValueOnly] private float panSpeed = 20f;
    [SerializeField, PositiveValueOnly] private float rotateSpeed = 50f;
    [SerializeField, PositiveValueOnly] private float zoomSpeed = 1000f;
    [SerializeField, PositiveValueOnly] private float minZoomDistance = 5f;
    [SerializeField, PositiveValueOnly] private float maxZoomDistance = 50f;

    private Vector3 lastPanPosition;
    private bool isPanning;

    private Vector3 pivotPoint;
    private bool isRotating;

    [SerializeField, MustBeAssigned] private Transform waypointsParent = null; 
    [SerializeField, ReadOnly] Transform[] waypoints;
    private bool isResetting = false; 

    [SerializeField] private GameObject firstPersonCam = null;
    private bool firstPersonCamActive = false;

    private void Start()
    {
        List<Transform> activeWaypoints = new List<Transform>();   
        for (int i = 0; i < waypointsParent.childCount; i++)
        {
            Transform child = waypointsParent.GetChild(i);
            if(child.gameObject.activeInHierarchy)
                activeWaypoints.Add(child);
        }
        waypoints = activeWaypoints.ToArray();

        Recenter(waypoints);
    }

    void Update()
    {
        if (Input.GetKeyDown (KeyCode.Alpha1))
        {
            firstPersonCamActive = true; 
            firstPersonCam.SetActive(true);
        }
        else if (Input.GetKeyDown (KeyCode.Alpha2))
        {
            firstPersonCamActive = false; 
            firstPersonCam.SetActive(false);
        }

        if (firstPersonCamActive)
            return; 

        if (isResetting)
        {
            isPanning = false;
            isRotating = false;

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            BeginPan();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndPan();
        }

        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            pivotPoint = ScreenPointToWorldPointOnPlane(Input.mousePosition, Vector3.zero, transform.rotation);
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        if (isPanning)
        {
            Pan();
        }

        if (isRotating)
        {
            Rotate();
        }

        Zoom(Input.GetAxis("Mouse ScrollWheel"));
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            Recenter(waypoints);
        }
    }

    void BeginPan()
    {
        lastPanPosition = Input.mousePosition;
        isPanning = true;
    }

    void EndPan()
    {
        isPanning = false;
    }

    void Pan()
    {
        Vector3 newMousePosition = Input.mousePosition;
        Vector3 mouseDeltaInScreenSpace = newMousePosition - lastPanPosition;
        Vector3 scaledDelta = mouseDeltaInScreenSpace * panSpeed * Time.deltaTime;
        Vector3 panInWorldSpace = scaledDelta.x * transform.right + scaledDelta.y * transform.up;

        transform.position -= panInWorldSpace;
        lastPanPosition = newMousePosition;
    }

    void Rotate()
    {
        Vector3 newPosition = Input.mousePosition;
        Vector3 positionDifference = newPosition - lastPanPosition;

        transform.RotateAround(pivotPoint, Vector3.up, positionDifference.x * rotateSpeed * Time.deltaTime);
        transform.RotateAround(pivotPoint, transform.right, -positionDifference.y * rotateSpeed * Time.deltaTime);

        lastPanPosition = newPosition;
    }

    void Zoom(float increment)
    {
        Vector3 newPos = transform.position + transform.forward * increment * zoomSpeed * Time.deltaTime;

        if (newPos.magnitude >= minZoomDistance && newPos.magnitude <= maxZoomDistance)
            transform.position = newPos; 
    }

    private Vector3 ScreenPointToWorldPointOnPlane(Vector3 screenPoint, Vector3 planeOrigin, Quaternion planeRotation)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Plane plane = new Plane(planeRotation * Vector3.back, planeOrigin);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    Bounds GetBounds(Transform[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
            return new Bounds();

        var bounds = new Bounds(waypoints[0].position, Vector3.zero);

        for (int i = 1; i < waypoints.Length; i++)
        {
            bounds.Encapsulate(waypoints[i].position);
        }

        return bounds;
    }

    void Recenter(Transform[] waypoints)
    {
        var bounds = GetBounds(waypoints);
        Vector3 desiredPosition = bounds.center;

        // Set rotation to be flat
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        // Position camera to fit all waypoints
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView); 
        float distance = bounds.extents.magnitude / cameraView; 
        desiredPosition -= distance * transform.forward; 

        // Smoothly move camera to desired position using DOTween
        isResetting = true;
        transform.DOMove(desiredPosition, 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => isResetting = false);
    }
}
