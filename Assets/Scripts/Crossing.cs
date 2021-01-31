using UnityEngine;

public class Crossing : MonoBehaviour {
    public Transform walkwayStart;
    public Transform walkwayEnd;
    public bool inUse;

    int _cars = 0;
    public bool HasCars() {
        return _cars > 0;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.TryGetComponent(out Car car)) {
            _cars++;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.TryGetComponent(out Car car)) {
            _cars--;
        }
    }
}
