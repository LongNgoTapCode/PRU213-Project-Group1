using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class OrderDashboardUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI orderListText;
    [SerializeField] private string title = "ORDERS";
    [SerializeField] private bool sortByRemainingTime = true;
    [SerializeField] private float refreshInterval = 0.2f;

    private readonly StringBuilder builder = new StringBuilder(256);
    private readonly List<Customer> ordersBuffer = new List<Customer>(16);
    private float refreshTimer;
    private string lastRenderedText = string.Empty;

    void OnEnable() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OrdersChanged += Refresh;
        }

        refreshTimer = 0f;
        Refresh();
    }

    void OnDisable() {
        if (OrderManager.Instance != null) {
            OrderManager.Instance.OrdersChanged -= Refresh;
        }
    }

    void Update() {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f) {
            refreshTimer = refreshInterval;
            Refresh();
        }
    }

    public void Refresh() {
        if (orderListText == null) {
            return;
        }

        builder.Clear();
        builder.AppendLine(title);

        if (OrderManager.Instance == null || OrderManager.Instance.ActiveOrders.Count == 0) {
            builder.Append("(empty)");
            SetTextIfChanged(builder.ToString());
            return;
        }

        ordersBuffer.Clear();
        ordersBuffer.AddRange(OrderManager.Instance.ActiveOrders);

        if (sortByRemainingTime) {
            ordersBuffer.Sort((a, b) => a.RemainingTime.CompareTo(b.RemainingTime));
        }

        for (int i = 0; i < ordersBuffer.Count; i++) {
            Customer order = ordersBuffer[i];
            int seconds = Mathf.CeilToInt(order.RemainingTime);
            builder.Append(i + 1).Append('.').Append(' ')
                .Append(order.DisplayOrderName)
                .Append("  [")
                .Append(seconds)
                .Append("s]");

            if (i < ordersBuffer.Count - 1) {
                builder.AppendLine();
            }
        }

        SetTextIfChanged(builder.ToString());
    }

    private void SetTextIfChanged(string nextText) {
        if (nextText == lastRenderedText) {
            return;
        }

        lastRenderedText = nextText;
        orderListText.text = nextText;
    }
}
