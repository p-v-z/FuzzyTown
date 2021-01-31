using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour {
    public Animator anim;
    public Vector3 currentTarget;
    public bool onA;

    bool _walking = false;
    Crossing _crossing;
    Vector3 _posA;
    Vector3 _posB;

    void Awake() {
        _crossing = GetComponentInParent<Crossing>();
        _posA = _crossing.walkwayStart.position;
        _posB = _crossing.walkwayEnd.position;
        StartCoroutine(WaitRoutine());
    }

    IEnumerator WaitRoutine() {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        if (_crossing.HasCars()) {
            Debug.Log("Crossing occupied, try again soon");
            StartCoroutine(WaitRoutine());
        } else {
            WalkAcrossRoad();
        }
    }

    public void WalkAcrossRoad() {
        Debug.Log("Walk across road");
        onA = !onA;
        currentTarget = onA ? _posB : _posA;
        _crossing.inUse = true;
        _walking = true;
        anim.SetFloat("Speed_f", 0.5f);
    }

    const float Speed = 0.5f;
    void Update() {
        if (_walking) {
            // Move our position a step closer to the target.
            float step = Speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, step);

            if (transform.position == currentTarget) {
                _walking = false;
                _crossing.inUse = false;
                anim.SetFloat("Speed_f", 0.0f);
                StartCoroutine(WaitRoutine());
            }
        }
    }
}
