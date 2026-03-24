using UnityEngine;

/// <summary>
/// Spawns cups at designated locations on the counter
/// </summary>
public class CupSpawner : MonoBehaviour {
    [SerializeField] private Cup cupPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 0.5f;

    private float respawnTimer;
    private bool cupTaken;
    private Cup currentCup;

    void Start() {
        if (spawnPoint == null) {
            spawnPoint = transform;
        }

        SpawnCup();
    }

    void Update() {
        if (!cupTaken && currentCup != null && currentCup.IsHeld) {
            cupTaken = true;
            respawnTimer = respawnDelay;
            currentCup = null;
        }

        if (cupTaken) {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f) {
                SpawnCup();
            }
        }
    }

    private void SpawnCup() {
        if (cupPrefab == null) {
            Debug.LogWarning("CupSpawner: Cup prefab not set", gameObject);
            return;
        }

        currentCup = Instantiate(cupPrefab, spawnPoint.position, Quaternion.identity);
        currentCup.name = "Cup";
        cupTaken = false;
    }
}
