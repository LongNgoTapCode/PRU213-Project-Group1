using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour {
    public static OrderManager Instance { get; private set; }

    [SerializeField] private int timeoutScorePenalty = 5;

    private readonly Dictionary<int, Customer> activeOrders = new Dictionary<int, Customer>();

    public int CompletedOrders { get; private set; }
    public int FailedOrders { get; private set; }

    public event Action OrdersChanged;

    public IReadOnlyCollection<Customer> ActiveOrders => activeOrders.Values;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterOrder(Customer customer) {
        if (customer == null) {
            return;
        }

        activeOrders[customer.OrderId] = customer;
        OrdersChanged?.Invoke();
    }

    public void CompleteOrder(Customer customer) {
        if (customer == null) {
            return;
        }

        if (activeOrders.Remove(customer.OrderId)) {
            CompletedOrders++;
            OrdersChanged?.Invoke();
        }
    }

    public void FailOrder(Customer customer) {
        if (customer == null) {
            return;
        }

        if (activeOrders.Remove(customer.OrderId)) {
            FailedOrders++;
            GameManager.Instance?.AddScore(-Mathf.Abs(timeoutScorePenalty));
            OrdersChanged?.Invoke();
        }
    }

    public void ResetRunStats() {
        CompletedOrders = 0;
        FailedOrders = 0;
        activeOrders.Clear();
        OrdersChanged?.Invoke();
    }
}
