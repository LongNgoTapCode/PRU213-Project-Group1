using UnityEngine;

public class ChaosCrowdMover : MonoBehaviour {
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float waitAtPointSeconds = 0.4f;
    [SerializeField] private bool loop = true;

    private int currentWaypointIndex;
    private float waitTimer;

    void Update() {
        if (waypoints == null || waypoints.Length == 0) {
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        if (target == null) {
            AdvanceWaypoint();
            return;
        }

        if (waitTimer > 0f) {
            waitTimer -= Time.deltaTime;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) <= 0.05f) {
            waitTimer = waitAtPointSeconds;
            AdvanceWaypoint();
        }
    }

    private void AdvanceWaypoint() {
        if (waypoints == null || waypoints.Length == 0) {
            return;
        }

        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length) {
            currentWaypointIndex = loop ? 0 : waypoints.Length - 1;
        }
    }
}
