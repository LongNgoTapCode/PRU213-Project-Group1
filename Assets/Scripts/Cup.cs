using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Cup : MonoBehaviour {
    [SerializeField] private CupData cupData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Header("Product Preview")]
    [SerializeField] private List<DrinkData> knownRecipes = new List<DrinkData>();
    [SerializeField] private bool requireExactIngredientOrder = true;
    
    private readonly List<string> contents = new List<string>();
    private Rigidbody2D body;
    private bool isHeld;
    private Transform handAnchor;
    private Sprite defaultCupSprite;
    private Color defaultCupColor = Color.white;

    public CupData CupData => cupData;
    public IReadOnlyList<string> Contents => contents;
    public bool IsHeld => isHeld;
    public bool IsFull => contents.Count >= cupData.capacity;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (GetComponent<Collider2D>() == null) {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.8f);
        }
    }

    void Start() {
        if (cupData != null && spriteRenderer != null) {
            if (cupData.cupSprite != null) {
                spriteRenderer.sprite = cupData.cupSprite;
            }
            spriteRenderer.color = cupData.cupColor;
        }

        if (spriteRenderer != null) {
            defaultCupSprite = spriteRenderer.sprite;
            defaultCupColor = spriteRenderer.color;
        }

        RefreshVisualByContents();
    }

    void Update() {
        if (isHeld && handAnchor != null) {
            transform.position = handAnchor.position;
            transform.rotation = handAnchor.rotation;
        }
    }

    public bool TryAddIngredient(string ingredient) {
        if (IsFull || string.IsNullOrWhiteSpace(ingredient)) {
            return false;
        }

        contents.Add(ingredient.Trim());
        RefreshVisualByContents();
        return true;
    }

    public void AttachToHand(Transform anchor) {
        if (anchor == null) return;

        handAnchor = anchor;
        isHeld = true;
        transform.SetParent(null);

        if (body != null) {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        GetComponent<Collider2D>().isTrigger = true;
    }

    public void DropToWorld(Vector3 worldPosition) {
        isHeld = false;
        handAnchor = null;
        transform.position = worldPosition;

        if (body != null) {
            body.simulated = true;
        }

        GetComponent<Collider2D>().isTrigger = false;
    }

    public void Clear() {
        contents.Clear();
        RefreshVisualByContents();
    }

    private void RefreshVisualByContents() {
        if (spriteRenderer == null) {
            return;
        }

        DrinkData matchedDrink;
        if (TryGetMatchedDrink(out matchedDrink) && matchedDrink.icon != null) {
            spriteRenderer.sprite = matchedDrink.icon;
            spriteRenderer.color = Color.white;
            return;
        }

        spriteRenderer.sprite = defaultCupSprite;
        spriteRenderer.color = defaultCupColor;
    }

    private bool TryGetMatchedDrink(out DrinkData matchedDrink) {
        matchedDrink = null;

        if (knownRecipes == null || knownRecipes.Count == 0 || contents.Count == 0) {
            return false;
        }

        for (int i = 0; i < knownRecipes.Count; i++) {
            DrinkData candidate = knownRecipes[i];
            if (candidate == null || candidate.ingredients == null) {
                continue;
            }

            if (candidate.ingredients.Count != contents.Count) {
                continue;
            }

            bool match = requireExactIngredientOrder
                ? IsExactOrderedMatch(contents, candidate.ingredients)
                : RecipeMatcher.IsExactMatch(contents, candidate.ingredients);

            if (!match) {
                continue;
            }

            matchedDrink = candidate;
            return true;
        }

        return false;
    }

    private bool IsExactOrderedMatch(IReadOnlyList<string> provided, IReadOnlyList<string> required) {
        if (provided == null || required == null || provided.Count != required.Count) {
            return false;
        }

        for (int i = 0; i < provided.Count; i++) {
            string left = Normalize(provided[i]);
            string right = Normalize(required[i]);
            if (!string.Equals(left, right, System.StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
        }

        return true;
    }

    private string Normalize(string value) {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
