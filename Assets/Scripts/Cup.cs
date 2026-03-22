using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Cup : MonoBehaviour {
    [SerializeField] private CupData cupData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private readonly List<string> contents = new List<string>();
    private Rigidbody2D body;
    private bool isHeld;
    private Transform handAnchor;

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
    }
}
