using UnityEngine;
using System.Collections.Generic;

public class HeatDirector : MonoBehaviour {
    public float heatLevel = 1f;
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public DrinkData[] possibleDrinks; // Kéo các DrinkData asset vào đây
    public Transform[] queuePositions; // Các vị trí xếp hàng cho khách
    [SerializeField] private Transform exitPoint;
    [Header("Order Generator")]
    [SerializeField] private float firstOrderDelay = 3f;
    [SerializeField] private float minSpawnRate = 1.5f;
    [SerializeField] private float maxSpawnRate = 10f;

    private float timer;
    private readonly List<Customer> activeQueue = new List<Customer>();
    private bool queueDirty;

    void Start() {
        timer = firstOrderDelay;
    }

    void Update() {
        heatLevel += Time.deltaTime * 0.01f;
        timer -= Time.deltaTime;

        CleanupQueue();
        if (queueDirty) {
            UpdateQueueTargets();
            queueDirty = false;
        }

        float spawnRate = Mathf.Clamp(5f / heatLevel, minSpawnRate, maxSpawnRate);
        if (UpgradeManager.Instance != null) {
            spawnRate *= UpgradeManager.Instance.ChaosIntervalMultiplier;
        }

        // Giới hạn số khách tối đa theo số vị trí xếp hàng
        int currentCustomers = activeQueue.Count;
        int maxCustomers = queuePositions != null && queuePositions.Length > 0 ? queuePositions.Length : 3;

        if (timer <= 0 && currentCustomers < maxCustomers) {
            SpawnCustomer();
            timer = spawnRate;
        }
    }

    void SpawnCustomer() {
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        GameObject customerObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);

        // Gán đơn hàng ngẫu nhiên
        Customer customer = customerObj.GetComponent<Customer>();
        if (customer != null && possibleDrinks != null && possibleDrinks.Length > 0) {
            customer.order = possibleDrinks[Random.Range(0, possibleDrinks.Length)];
            customer.ConfigureAiTargets(GetQueuePositionByIndex(activeQueue.Count), exitPoint);
            customer.LeftCafe += HandleCustomerLeft;
            activeQueue.Add(customer);
            queueDirty = true;
        }
    }

    private Transform GetQueuePositionByIndex(int index) {
        if (queuePositions == null || queuePositions.Length == 0) {
            return null;
        }

        int clampedIndex = Mathf.Clamp(index, 0, queuePositions.Length - 1);
        return queuePositions[clampedIndex];
    }

    private void HandleCustomerLeft(Customer customer) {
        if (customer != null) {
            customer.LeftCafe -= HandleCustomerLeft;
        }

        activeQueue.Remove(customer);
        queueDirty = true;
    }

    private void CleanupQueue() {
        bool changed = false;
        for (int i = activeQueue.Count - 1; i >= 0; i--) {
            if (activeQueue[i] == null) {
                activeQueue.RemoveAt(i);
                changed = true;
            }
        }

        if (changed) {
            queueDirty = true;
        }
    }

    private void UpdateQueueTargets() {
        for (int i = 0; i < activeQueue.Count; i++) {
            Customer customer = activeQueue[i];
            if (customer == null) {
                continue;
            }

            customer.SetQueueTarget(GetQueuePositionByIndex(i));
        }
    }
}