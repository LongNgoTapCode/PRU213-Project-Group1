using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour {
    [SerializeField] private string ingredientName = "Ingredient";
    [SerializeField] private bool heavyItem;
    [SerializeField] private float normalFollowSpeed = 18f;
    [SerializeField] private float heavyFollowSpeed = 8f;

    private Rigidbody2D body;
    private Collider2D[] allColliders;
    private Transform handAnchor;

    public string IngredientName => ingredientName;
    public bool IsHeld { get; private set; }
    public CounterSlot CurrentSlot { get; private set; }
    public bool IsHeavyItem => heavyItem;

    void Awake() {
        body = GetComponent<Rigidbody2D>();
        allColliders = GetComponents<Collider2D>();
    }

    public void SetIngredientName(string value) {
        ingredientName = value;
    }

    public void AttachToHand(Transform handAnchor) {
        if (handAnchor == null) {
            return;
        }

        DetachFromSlot();
        IsHeld = true;
        this.handAnchor = handAnchor;

        transform.SetParent(null);

        if (body != null) {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        SetCollidersAsTrigger(true);
    }

    public void DropToWorld(Vector3 worldPosition) {
        IsHeld = false;
        handAnchor = null;
        transform.SetParent(null);
        transform.position = worldPosition;

        if (body != null) {
            body.simulated = true;
        }

        SetCollidersAsTrigger(false);
    }

    public void SnapToSlot(CounterSlot slot, Transform snapPoint) {
        if (slot == null || snapPoint == null) {
            return;
        }

        IsHeld = false;
        handAnchor = null;
        CurrentSlot = slot;

        transform.SetParent(snapPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (body != null) {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        SetCollidersAsTrigger(true);
    }

    public void ClearSlotOwnership(CounterSlot slot) {
        if (CurrentSlot == slot) {
            CurrentSlot = null;
        }
    }

    private void DetachFromSlot() {
        if (CurrentSlot == null) {
            return;
        }

        CurrentSlot.ClearItem(this);
        CurrentSlot = null;
    }

    private void SetCollidersAsTrigger(bool isTrigger) {
        for (int i = 0; i < allColliders.Length; i++) {
            allColliders[i].isTrigger = isTrigger;
        }
    }

    void LateUpdate() {
        if (!IsHeld || handAnchor == null) {
            return;
        }

        float followSpeed = heavyItem ? heavyFollowSpeed : normalFollowSpeed;
        transform.position = Vector3.Lerp(transform.position, handAnchor.position, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, handAnchor.rotation, followSpeed * Time.deltaTime);
    }
}
