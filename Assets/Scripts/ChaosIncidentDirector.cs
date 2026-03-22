using UnityEngine;

public enum WorkstationIncidentType {
    None,
    MachineBroken,
    MilkSpill,
    OutOfBeans
}

public class ChaosIncidentDirector : MonoBehaviour {
    [SerializeField] private Workstation[] targetWorkstations;
    [SerializeField] private bool autoFindWorkstations = true;
    [SerializeField] private float minIncidentInterval = 14f;
    [SerializeField] private float maxIncidentInterval = 30f;
    [SerializeField] private float minIncidentDuration = 5f;
    [SerializeField] private float maxIncidentDuration = 10f;

    private float timer;

    void Start() {
        if (autoFindWorkstations || targetWorkstations == null || targetWorkstations.Length == 0) {
            targetWorkstations = FindObjectsByType<Workstation>(FindObjectsSortMode.None);
        }

        ResetTimer();
    }

    void Update() {
        if (targetWorkstations == null || targetWorkstations.Length == 0) {
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) {
            return;
        }

        TryTriggerRandomIncident();
        ResetTimer();
    }

    private void TryTriggerRandomIncident() {
        Workstation target = targetWorkstations[Random.Range(0, targetWorkstations.Length)];
        if (target == null || target.HasActiveIncident) {
            return;
        }

        WorkstationIncidentType type = (WorkstationIncidentType)Random.Range(1, 4);
        float duration = Random.Range(minIncidentDuration, maxIncidentDuration);
        target.TryTriggerIncident(type, duration);
    }

    private void ResetTimer() {
        float interval = Random.Range(minIncidentInterval, maxIncidentInterval);
        if (UpgradeManager.Instance != null) {
            interval *= UpgradeManager.Instance.ChaosIntervalMultiplier;
        }

        timer = interval;
    }
}
