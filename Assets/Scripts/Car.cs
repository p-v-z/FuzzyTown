using Cinemachine;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Car : MonoBehaviour {
    [Header("Car Settings")]
    public bool MovingForward = true;
    public float Speed = 2f;
    public float CautionSize = 0.3f;
    public GameObject ModelContainer;

    public Road CurrentRoad { get; private set; }

    [Header("Gears")]
    [SerializeField] bool _moving;
    [SerializeField] bool _switchingLanes;
    [SerializeField] float _cartSpeed;
    [SerializeField] CustomCart _cart;
    [SerializeField] CarConfig _config;
    [SerializeField] Transform _collisionChecker;

    [Header("Road")]
    [SerializeField] float _pathLength;
    [SerializeField] Lane _currentLane;
    [SerializeField] Lane _targetLane;
    [SerializeField] Intersection _intersectionFrom;

    bool _hitCenter = false;
    bool _shouldTurnOff = false;
    float _turnOffOffset;
    IntersectionStraight _intersectionStraight;

    void Awake() {
        if (_config) {
            Instantiate(_config.carPrefab, ModelContainer.transform);
            Speed = _config.carSpeed;
        }
        if (CheckForTrack()) StartCar();
    }

    Lane CheckForTrack() {
        if (_cart.m_Path) {
            var road = _cart.m_Path.GetComponent<Road>();
            var lane = MovingForward ? road.LaneA : road.LaneB;
            return PutCarOnLane(lane);
        }

        return null;
    }

    public void StopCar(bool stopForCollision = false) {
        _moving = false;
        _cart.m_Speed = 0f;
        _cart.enabled = false;

        if (stopForCollision) {
            StartCoroutine(TryMoveRoutine());
        }
    }

    IEnumerator TryMoveRoutine() {
        yield return new WaitForSeconds(0.5f);

        if (!ObstacleExists()) {
            StartCar();
        } else {
            StartCoroutine(TryMoveRoutine());
        }
    }

    public void StartCar() {
        _moving = true;
        _cart.m_Speed = _cartSpeed;
        _cart.enabled = true;
    }

    void Update() {
        if (!_moving) return;

        // Stop the car if there is an obstacle 
        if (ObstacleExists()) {
            StopCar(true);
            return;
        }

        if (_switchingLanes) {
            Vector3 targetPos;
            if (_offset > 0f) { // turning into the middle of a lane
                var straight = _intersectionFrom.GetComponent<IntersectionStraight>();
                targetPos = straight.MainRoad.GetPointInPath(straight.offset);
            } else {
                if (_hitCenter) {
                    targetPos = _targetLane.GetStartPoint();
                } else {
                    targetPos = _intersectionFrom.transform.position;
                }
            }

            // Move our position a step closer to the target.
            float step = Speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

            // Look towards movement direction
            Vector3 targetDirection = targetPos - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

            // Check if the position are approximately equal.
            if (Vector3.Distance(transform.position, targetPos) < 0.001f) {
                if (_hitCenter) {
                    PutCarOnLaneFromIntersection(_targetLane, _offset);
                } else {
                    _hitCenter = true;
                }
            }
        } else { // Car is moving forward
            if (_currentLane == null) { Debug.LogWarning("No lane"); return; }

            if (_shouldTurnOff) {
                if (!IsAtTurnoff()) return;
                _shouldTurnOff = false;
                StopCar();
                GetOffLane();
                MoveToLane(_intersectionStraight.LaneEntrances.First(), _intersectionStraight);
            } else {
                // Check for the end of the path
                if (!IsAtEnd()) return;
                StopCar();
                PlaceCarInQueue();
            }
        }
    }

    bool IsAtEnd() => MovingForward ? _cart.m_Position >= _pathLength : _cart.m_Position <= 0;
    bool IsAtTurnoff() => MovingForward ? _cart.m_Position >= _turnOffOffset : _cart.m_Position <= _turnOffOffset;

    void PlaceCarInQueue() {
        if (_currentLane.IntersectionAtEnd) {
            Debug.Log("Put car on intersection");
            _currentLane.IntersectionAtEnd.QueueCar(this);
            _cart.enabled = false;
            _cart.m_Path = null;
        } else Debug.Log("No intersection - stop");

    }

    bool ObstacleExists() {
        Collider[] hitColliders = Physics.OverlapSphere(_collisionChecker.position, CautionSize);
        bool colFound = false;
        foreach (var col in hitColliders) {
            switch (col.tag) {
                case "Car":
                    Car otherCar = col.GetComponent<Car>();
                    if (otherCar != this && otherCar._currentLane == _currentLane) {
                        colFound = true;
                    }
                    break;
                case "Obstacle":
                    Crossing crossing = col.GetComponentInChildren<Crossing>();
                    if (crossing != null) {
                        if (crossing.inUse) {
                            colFound = true;
                        }
                    }
                    break;
            }
        }

        return colFound;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_collisionChecker.position, CautionSize);
    }

    float _offset;
    public void MoveToLaneOffset(Lane target, Intersection from, float offset) {
        _offset = offset;
        MoveToLane(target, from);
    }

    public void MoveToLane(Lane target, Intersection from) {
        Debug.Log("Start moving to new lane");
        _targetLane = target;
        _switchingLanes = true;
        _intersectionFrom = from;
        _intersectionFrom.inUse = true;

        GetOffLane();
        StartCar();
    }

    void GetOffLane() {
        _currentLane = null;
        _cart.m_Path = null;
    }

    void PutCarOnLaneFromIntersection(Lane target, float offset = 0f) {
        _intersectionFrom.inUse = false;
        _switchingLanes = false;
        _targetLane = null;

        if (offset > 0f) {
            PutCarOnLane(target, offset);
        } else {
            PutCarOnLane(target);
        }
    }

    public Lane PutCarOnLane(Lane target, float offset) {
        var lane = PutCarOnLane(target);
        _cart.m_Position = _pathLength * offset;
        _offset = 0f;
        return lane;
    }

    public Lane PutCarOnLane(Lane target) {

        // Set lane and place car on GameObject
        _currentLane = target;
        if (CurrentRoad != _currentLane.Road) {
            CurrentRoad = _currentLane.Road;
            CheckForOffturn();
        }

        // Set path
        CinemachinePath path = CurrentRoad.Path;
        _pathLength = path.PathLength;
        _cart.m_Path = path;

        // Set cart speed, position, and direction
        MovingForward = _currentLane.PositiveDirection;
        _cartSpeed = Speed * (MovingForward ? 1 : -1);
        _cart.m_Speed = _cartSpeed;
        _cart.m_Position = MovingForward ? 0 : _pathLength;
        return _currentLane;
    }

    void CheckForOffturn() {
        if (CurrentRoad.hasOffturn && Random.Range(0, 10) >= 7) {
            _intersectionStraight = CurrentRoad.OffTurns.First();
            _turnOffOffset = _intersectionStraight.offset * CurrentRoad.Path.PathLength;
            _shouldTurnOff = true;
            Debug.Log($"Car will turn off at {_turnOffOffset}");
        }
    }
}
