using UnityEditor;
using UnityEngine;

public class IntersectionStraight : Intersection {

    public Road MainRoad;
    public float IntersectionOffset;

    protected override void CheckForCars() {
        if (Log) Debug.Log("Override");
        if (CarQueue.Count > 0) {
            Car car = CarQueue.Dequeue();
            Lane newLane = Random.Range(0, 10) >= 5 ? MainRoad.LaneA : MainRoad.LaneB;
            car.MoveToLaneOffset(newLane, this, IntersectionOffset);
        }
    }
}

[CustomEditor(typeof(IntersectionStraight))]
public class IntersectionStraightInspector : IntersectionInspector {
    IntersectionStraight _intersectionStraight;

    void OnEnable() {
        _intersectionStraight = (IntersectionStraight)target;
    }

    void OnSceneGUI() {
        if (!_intersectionStraight) return;
        DrawLaneLines(_intersectionStraight);
        Vector3 pos = _intersectionStraight.MainRoad.GetPointInPath(_intersectionStraight.IntersectionOffset);
        Handles.color = Color.cyan;
        Handles.DrawSolidDisc(pos, Vector3.up, 0.25f);
    }
}