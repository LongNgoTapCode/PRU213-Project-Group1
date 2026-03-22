using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecipeDefinition {
    public string drinkName;
    public List<string> ingredients = new List<string>();
}

[Serializable]
public class RecipeCollection {
    public List<RecipeDefinition> recipes = new List<RecipeDefinition>();
}

public class RecipeBook : MonoBehaviour {
    [SerializeField] private TextAsset recipeFile;

    private readonly Dictionary<string, RecipeDefinition> recipesByName = new Dictionary<string, RecipeDefinition>(StringComparer.OrdinalIgnoreCase);

    public bool IsLoaded { get; private set; }

    void Awake() {
        Reload();
    }

    public void Reload() {
        recipesByName.Clear();
        IsLoaded = false;

        if (recipeFile == null || string.IsNullOrWhiteSpace(recipeFile.text)) {
            Debug.LogWarning("RecipeBook: recipe file is missing or empty.");
            return;
        }

        RecipeCollection parsed = JsonUtility.FromJson<RecipeCollection>(recipeFile.text);
        if (parsed == null || parsed.recipes == null) {
            Debug.LogWarning("RecipeBook: failed to parse recipe file.");
            return;
        }

        for (int i = 0; i < parsed.recipes.Count; i++) {
            RecipeDefinition recipe = parsed.recipes[i];
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.drinkName)) {
                continue;
            }

            recipesByName[recipe.drinkName.Trim()] = recipe;
        }

        IsLoaded = true;
    }

    public bool TryGetRecipe(string drinkName, out RecipeDefinition recipe) {
        recipe = null;
        if (!IsLoaded || string.IsNullOrWhiteSpace(drinkName)) {
            return false;
        }

        return recipesByName.TryGetValue(drinkName.Trim(), out recipe);
    }
}
