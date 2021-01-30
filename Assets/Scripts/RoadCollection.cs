using System.Collections.Generic;
using System.Linq;

public class RoadCollection : Singleton<RoadCollection> {
    public List<Road> Roads;
    public List<Intersection> Intersections;

    void Awake() {
        Roads = FindObjectsOfType<Road>().ToList();
        Intersections = FindObjectsOfType<Intersection>().ToList();
    }
}
