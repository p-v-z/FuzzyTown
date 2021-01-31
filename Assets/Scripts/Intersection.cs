using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Intersection : MonoBehaviour {
    public static bool Log = false;

    public bool inUse = false;
    public List<Lane> LaneEntrances;
    public readonly Queue<Car> CarQueue = new Queue<Car>();

    void Awake() {
        // Connect lane exits to this intersection
        LaneEntrances.ForEach(lane => {
                var other = lane.Road.GetOppositeLane(lane);
                other.IntersectionAtEnd = this;
            }
        );

        InvokeRepeating("CheckForCars", 1f, Random.Range(0.9f, 1.1f));  //1s delay, repeat every 1s
    }

    protected virtual void CheckForCars() {
        if (Log) Debug.Log("Base");
        if (CarQueue.Count > 0) {
            Car car = CarQueue.Dequeue();
            Lane newLane = GetLaneFromOtherRoad(car.CurrentRoad);
            car.MoveToLane(newLane, this);
        }
    }

    public void QueueCar(Car target) {
        CarQueue.Enqueue(target);
    }
    
    public Lane GetLaneFromOtherRoad(Road from) {
        if (LaneEntrances.Count > 1) {
            // Return other random Lane (or only lane)
            List<Lane> other = (from t in LaneEntrances where !t.Road.Equals(@from) select t).ToList();
            int items = other.Count;
            return items > 1 ? other[Random.Range(0, items)] : other[0];
        } Debug.LogWarning("No entrances");
        return null;

    }
}

[CustomEditor(typeof(Intersection))]
public class IntersectionInspector : Editor {
    Intersection _intersection;

    void OnEnable() {
        _intersection = (Intersection) target;
    }
    void OnSceneGUI() {
        DrawLaneLines(_intersection);
    }

    protected void DrawLaneLines<T>(T intersection) where T: Intersection {
        if (intersection.LaneEntrances.Count > 0) {
            intersection.LaneEntrances.ForEach(lane => {
                if (!lane) return;
                Handles.color = lane.PositiveDirection ? Color.green : Color.red;
                Handles.DrawLine(intersection.transform.position, lane.GetStartPoint(), 10f);
            });
        }
    }
}