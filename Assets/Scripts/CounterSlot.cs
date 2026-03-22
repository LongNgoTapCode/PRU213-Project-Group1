using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CounterSlot : MonoBehaviour {
    [SerializeField] private Transform snapPoint;

    public PickupItem CurrentItem { get; private set; }

    public bool TryPlace(PickupItem item) {
        if (item == null || CurrentItem != null) {
            return false;
        }

        CurrentItem = item;
        Transform target = snapPoint != null ? snapPoint : transform;
        item.SnapToSlot(this, target);
        return true;
    }

    public PickupItem TryTake() {
        if (CurrentItem == null) {
            return null;
        }

        PickupItem item = CurrentItem;
        CurrentItem = null;
        item.ClearSlotOwnership(this);
        return item;
    }

    public void ClearItem(PickupItem item) {
        if (CurrentItem == item) {
            CurrentItem = null;
        }
    }

    void OnDrawGizmosSelected() {
        Transform target = snapPoint != null ? snapPoint : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(target.position, new Vector3(0.45f, 0.45f, 0.05f));
    }
}
