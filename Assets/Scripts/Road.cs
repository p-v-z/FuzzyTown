using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Road : MonoBehaviour {
    public CinemachinePath Path;

    public Lane LaneA;
    public Lane LaneB;

    public List<Intersection> Intersections;

    public List<IntersectionStraight> OffTurns;

    void Awake() {
        if (Path.m_Waypoints.Length != 2) Debug.LogWarning("Road does not have correct amount of waypoints");
    }

    void Start() {
        FindConnectedIntersections();
    }
    
    public bool hasOffturn { get; private set; }
    void FindConnectedIntersections() {
        //RoadCollection.Instance.Roads.ForEach(road => {
            // check for staragit raod
        //    GetComponent<IntersectionStraight>()
        //});
        var intersections = RoadCollection.Instance.Intersections;
        if (intersections.Count == 0) return;

        intersections.ForEach(intersection => {
            IntersectionStraight straight = intersection.GetComponent<IntersectionStraight>();
            if (straight != null) {
                if (straight.MainRoad == this) {
                    hasOffturn = true;
                    OffTurns.Add(straight);
                    Debug.Log("Road has offtur");
                    //return;
                }
            }
        });
    }

    public Vector3 GetPointInPath(float offset) {
        var pos1 = transform.TransformPoint(Path.m_Waypoints[0].position);
        var pos2 = transform.TransformPoint(Path.m_Waypoints[1].position);
        Vector3 targetPos = LerpByDistance(pos1, pos2, Path.PathLength * offset);
        return targetPos;
    }

    public Vector3 LerpByDistance(Vector3 a, Vector3 b, float x) {
        Vector3 p = x * Vector3.Normalize(b - a) + a;
        return p;
    }

    public Lane GetOppositeLane(Lane lane) {
        return lane == LaneA ? LaneB : LaneA;
    }
}