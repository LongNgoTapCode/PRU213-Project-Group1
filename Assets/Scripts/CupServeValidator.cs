using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Validates cup contents against customer order and handles serving
/// </summary>
public class CupServeValidator : MonoBehaviour {
    public static CupServeValidator Instance { get; private set; }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Serves cup to customer and validates if it matches their order
    /// </summary>
    public bool TryServeCupToCustomer(Cup cup, Customer customer) {
        if (cup == null || customer == null) {
            return false;
        }

        // Get required ingredients
        var requiredIngredients = customer.GetRequiredIngredients();
        var cupContents = cup.Contents;

        // True if exact match (unordered)
        bool isCorrect = RecipeMatcher.IsExactMatch(cupContents, requiredIngredients);

        if (isCorrect) {
            // Award coins to player
            int reward = GetReward(customer);
            GameManager.Instance?.AddCoins(reward);
            Debug.Log($"CORRECT! Customer {customer.DisplayOrderName} served. Reward: {reward} coins");
            
            // Complete the order
            OrderManager.Instance?.CompleteOrder(customer);
            customer.Satisfy(); // Make customer leave happy
            
            return true;
        } else {
            // Wrong order - lose reputation
            Debug.Log($"WRONG ORDER! Customer wanted {string.Join(", ", requiredIngredients)} but got {string.Join(", ", cupContents)}");
            GameManager.Instance?.LoseReputation();
            OrderManager.Instance?.FailOrder(customer);
            customer.Leave(); // Make customer leave unhappy
            
            return false;
        }
    }

    private int GetReward(Customer customer) {
        // Base reward from customer data + precision bonus
        int reward = 10; // Base
        
        // Bonus for serving before patience runs out
        if (customer.RemainingTime > customer.MaxPatience * 0.5f) {
            reward += 5; // Still plenty of time
        }

        return reward;
    }
}
