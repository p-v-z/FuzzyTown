using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour {
    public static bool Log = false;

    [SerializeField] Animator _anim;

    bool _onA;
    bool _walking = false;
    Vector3 _posA;
    Vector3 _posB;
    Vector3 _currentTarget;
    Crossing _crossing;

    void Awake() {
        _crossing = GetComponentInParent<Crossing>();
        _posA = _crossing.walkwayStart.position;
        _posB = _crossing.walkwayEnd.position;
        StartCoroutine(WaitRoutine());
    }

    IEnumerator WaitRoutine() {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        if (_crossing.HasCars()) {
            if (Log) Debug.Log("Crossing occupied, try again soon");
            StartCoroutine(WaitRoutine());
        } else {
            WalkAcrossRoad();
        }
    }

    public void WalkAcrossRoad() {
        if (Log) Debug.Log("Walk across road");
        _onA = !_onA;
        _currentTarget = _onA ? _posB : _posA;
        _crossing.inUse = true;
        _walking = true;
        _anim.SetFloat("Speed_f", 0.5f);
    }

    const float Speed = 0.5f;
    void Update() {
        if (_walking) {
            // Move our position a step closer to the target.
            float step = Speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, _currentTarget, step);

            if (transform.position == _currentTarget) {
                _walking = false;
                _crossing.inUse = false;
                _anim.SetFloat("Speed_f", 0.0f);
                StartCoroutine(WaitRoutine());
            }
        }
    }
}
