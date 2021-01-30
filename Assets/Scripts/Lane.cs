using UnityEngine;

public class Lane : MonoBehaviour {
    public bool PositiveDirection = true;
    public Road Road;
    public LaneType Type;
    public Intersection IntersectionAtEnd;

    public Vector3 GetStartPoint() {
        var wp = Type == LaneType.A ? 0 : 1;
        var waypointPos = Road.Path.m_Waypoints[wp].position;
        var point = Road.transform.TransformPoint(waypointPos);
        return point;
    }
}

public enum LaneType { A, B }