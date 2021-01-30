using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CarConfig", order = 1)]
public class CarConfig : ScriptableObject {
    public GameObject carPrefab;
    public float carSpeed;
}
