using UnityEngine;

public class HeatDirector : MonoBehaviour {
    public float heatLevel = 1f;
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public DrinkData[] possibleDrinks; // Kéo các DrinkData asset vào đây
    public Transform[] queuePositions; // Các vị trí xếp hàng cho khách
    private float timer;
    private int nextQueueIndex = 0;

    void Start() {
        timer = 3f; // Đợi 3 giây trước khi spawn khách đầu tiên
    }

    void Update() {
        heatLevel += Time.deltaTime * 0.01f;
        timer -= Time.deltaTime;

        float spawnRate = Mathf.Clamp(5f / heatLevel, 1.5f, 10f);

        // Giới hạn số khách tối đa theo số vị trí xếp hàng
        int currentCustomers = GameObject.FindGameObjectsWithTag("Customer").Length;
        int maxCustomers = queuePositions != null && queuePositions.Length > 0 ? queuePositions.Length : 3;

        if (timer <= 0 && currentCustomers < maxCustomers) {
            SpawnCustomer();
            timer = spawnRate;
        }
    }

    void SpawnCustomer() {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        // Tìm vị trí xếp hàng trống
        if (queuePositions != null && queuePositions.Length > 0) {
            spawnPos = queuePositions[nextQueueIndex % queuePositions.Length].position;
            nextQueueIndex++;
        }

        GameObject customerObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);

        // Gán đơn hàng ngẫu nhiên
        Customer customer = customerObj.GetComponent<Customer>();
        if (customer != null && possibleDrinks != null && possibleDrinks.Length > 0) {
            customer.order = possibleDrinks[Random.Range(0, possibleDrinks.Length)];
        }
    }
}