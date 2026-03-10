using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance;
    public List<string> currentHoldings = new List<string>();

    void Awake() { Instance = this; }

    public void AddIngredient(string ing) {
        if (currentHoldings.Count < 5) {
            currentHoldings.Add(ing);
            Debug.Log("Holding: " + string.Join(", ", currentHoldings));
        } else {
            Debug.Log("Hands full! Max 5 ingredients.");
        }
    }

    public void TrashFlip() {
        currentHoldings.Clear();
        Debug.Log("Cleared hands!");
    }

    void Update() {
        if (Mouse.current == null) return;

        // Click trái: tương tác với Workstation hoặc Customer
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // Dùng OverlapPointAll để tìm tất cả collider tại vị trí click
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);

            Debug.Log("Click at " + worldPos + " - Found " + colliders.Length + " colliders");

            foreach (Collider2D col in colliders) {
                Debug.Log("  Hit: " + col.gameObject.name);

                // Thử tương tác Workstation
                Workstation ws = col.GetComponent<Workstation>();
                if (ws != null) {
                    Debug.Log("  -> Workstation clicked: " + ws.ingredientOutput);
                    ws.HandleClick();
                    return;
                }

                // Thử tương tác Customer
                Customer customer = col.GetComponent<Customer>();
                if (customer != null) {
                    Debug.Log("  -> Customer clicked! Holdings: " + string.Join(", ", currentHoldings));
                    bool success = customer.ReceiveDrink(currentHoldings);
                    if (success) currentHoldings.Clear();
                    return;
                }
            }
        }

        // Click phải để bỏ hết nguyên liệu (TrashFlip)
        if (Mouse.current.rightButton.wasPressedThisFrame) {
            TrashFlip();
        }
    }
}