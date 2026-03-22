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

    void Start() {
        if (spawnPoint == null) {
            spawnPoint = transform;
        }

        SpawnCup();
    }

    void Update() {
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

        Cup cup = Instantiate(cupPrefab, spawnPoint.position, Quaternion.identity);
        cup.name = "Cup";
        cupTaken = false;

        // Optional: notify that cup is available
        if (cup.GetComponent<Collider2D>() != null) {
            cup.GetComponent<Collider2D>().enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        Cup cup = col.GetComponent<Cup>();
        if (cup != null) {
            cup.GetComponent<Collider2D>().enabled = false;
            cupTaken = true;
            respawnTimer = respawnDelay;
        }
    }
}
