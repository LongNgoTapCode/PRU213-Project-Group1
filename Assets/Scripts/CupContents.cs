using System.Collections.Generic;
using UnityEngine;

public class CupContents : MonoBehaviour {
    [SerializeField] private List<string> ingredients = new List<string>();

    public IReadOnlyList<string> Ingredients => ingredients;

    public void SetIngredients(IEnumerable<string> values) {
        ingredients.Clear();
        if (values == null) {
            return;
        }

        foreach (string value in values) {
            if (!string.IsNullOrWhiteSpace(value)) {
                ingredients.Add(value.Trim());
            }
        }
    }

    public void AddIngredient(string ingredient) {
        if (!string.IsNullOrWhiteSpace(ingredient)) {
            ingredients.Add(ingredient.Trim());
        }
    }

    public void Clear() {
        ingredients.Clear();
    }
}
