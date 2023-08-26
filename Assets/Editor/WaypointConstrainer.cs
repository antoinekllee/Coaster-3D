using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    private Vector3 lastPosition;
    private Transform waypointParent;

    void OnEnable()
    {
        lastPosition = ((Waypoint)target).transform.position;
        waypointParent = ((Waypoint)target).transform.parent;
    }

    void OnSceneGUI()
    {
        Waypoint waypoint = (Waypoint)target;
        if (lastPosition != waypoint.transform.position)
        {
            int index = System.Array.IndexOf(waypointParent.GetComponentsInChildren<Transform>(), waypoint.transform) - 1; // Account for the parent transform
            
            if (index > 0 && index < waypointParent.childCount - 2) // Also adjusted for the parent transform
            {
                // If it's one before the joint point (e.g., 3rd, 6th, 9th,...)
                if (index % 3 == 2)
                {
                    Transform jointPoint = waypointParent.GetChild(index + 1);
                    Transform afterJointPoint = waypointParent.GetChild(index + 2);
                    Vector3 newDir = jointPoint.position - waypoint.transform.position;
                    afterJointPoint.position = jointPoint.position + newDir;
                }
                // If it's one after the joint point (e.g., 5th, 8th, 11th,...)
                else if ((index - 1) % 3 == 0)
                {
                    Transform jointPoint = waypointParent.GetChild(index - 1);
                    Transform beforeJointPoint = waypointParent.GetChild(index - 2);
                    Vector3 newDir = jointPoint.position - waypoint.transform.position;
                    beforeJointPoint.position = jointPoint.position + newDir;
                }
            }
            lastPosition = waypoint.transform.position;
        }
    }
}
