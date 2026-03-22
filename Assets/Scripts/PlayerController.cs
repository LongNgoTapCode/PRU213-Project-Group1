using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance;
    public List<string> currentHoldings = new List<string>();

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private bool usePhysicsMovement = true;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string lastMoveXParam = "LastMoveX";
    [SerializeField] private string lastMoveYParam = "LastMoveY";
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string isMovingParam = "IsMoving";

    [Header("Pickup & Drop")]
    [SerializeField] private Transform handAnchor;
    [SerializeField] private float interactionRadius = 1.25f;
    [SerializeField] private LayerMask interactionMask = ~0;
    [SerializeField] private float tapThresholdSeconds = 0.2f;
    [SerializeField] private bool verboseLogs;

    private PickupItem heldItem;
    private float eHoldTimer;
    private bool isHoldingE;
    private bool attemptedRepair;
    private Workstation repairTarget;
    private readonly Collider2D[] nearbyHitsBuffer = new Collider2D[48];
    private readonly Collider2D[] clickHitsBuffer = new Collider2D[32];
    private Camera cachedMainCamera;
    private Rigidbody2D body;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private ContactFilter2D interactionFilter;
    private int moveXHash;
    private int moveYHash;
    private int lastMoveXHash;
    private int lastMoveYHash;
    private int speedHash;
    private int isMovingHash;

    void Awake() {
        Instance = this;
        body = GetComponent<Rigidbody2D>();
        if (characterAnimator == null) {
            characterAnimator = GetComponentInChildren<Animator>();
        }

        CacheAnimatorHashes();
        ConfigureInteractionFilter();
    }

    void Start() {
        cachedMainCamera = Camera.main;
    }

    public void AddIngredient(string ing) {
        if (currentHoldings.Count < 5) {
            currentHoldings.Add(ing);
            Debug.Log("Holding: " + string.Join(", ", currentHoldings));
        } else {
            Debug.Log("Hands full! Max 5 ingredients.");
        }
    }

    public void TrashFlip() {
        if (heldItem != null) {
            Destroy(heldItem.gameObject);
            heldItem = null;
        }

        currentHoldings.Clear();
        Debug.Log("Cleared hands!");
    }

    public bool HasHeldItem() {
        return heldItem != null;
    }

    public PickupItem GetHeldItem() {
        return heldItem;
    }

    public bool TryPickupItem(PickupItem item) {
        if (item == null || heldItem != null || handAnchor == null) {
            return false;
        }

        heldItem = item;
        heldItem.AttachToHand(handAnchor);
        return true;
    }

    public PickupItem ReleaseHeldItem() {
        PickupItem item = heldItem;
        heldItem = null;
        return item;
    }

    void Update() {
        if (Mouse.current == null) return;

        ReadMovementInput();
        UpdateAnimationParameters();

        if (Keyboard.current != null) {
            HandleEInteraction();
        }

        // Click trái: tương tác với Workstation hoặc Customer
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (cachedMainCamera == null) {
                cachedMainCamera = Camera.main;
                if (cachedMainCamera == null) {
                    return;
                }
            }

            Vector2 worldPos = cachedMainCamera.ScreenToWorldPoint(mousePos);

            // Dùng OverlapPointAll để tìm tất cả collider tại vị trí click
            int clickHitCount = Physics2D.OverlapPoint(worldPos, interactionFilter, clickHitsBuffer);

            if (verboseLogs) {
                Debug.Log("Click at " + worldPos + " - Found " + clickHitCount + " colliders");
            }

            for (int i = 0; i < clickHitCount; i++) {
                Collider2D col = clickHitsBuffer[i];
                if (verboseLogs) {
                    Debug.Log("  Hit: " + col.gameObject.name);
                }

                // Thử tương tác Workstation
                Workstation ws = col.GetComponent<Workstation>();
                if (ws != null) {
                    if (verboseLogs) {
                        Debug.Log("  -> Workstation clicked: " + ws.ingredientOutput);
                    }
                    ws.HandleClick();
                    return;
                }

                // Thử tương tác Customer
                Customer customer = col.GetComponent<Customer>();
                if (customer != null) {
                    if (verboseLogs) {
                        Debug.Log("  -> Customer clicked! Holdings: " + string.Join(", ", currentHoldings));
                    }
                    bool success = false;

                    if (heldItem != null) {
                        success = customer.ReceiveCup(heldItem);
                        if (success) {
                            Destroy(heldItem.gameObject);
                            heldItem = null;
                        }
                    } else {
                        success = customer.ReceiveDrink(currentHoldings);
                        if (success) currentHoldings.Clear();
                    }

                    return;
                }
            }
        }

        // Click phải để bỏ hết nguyên liệu (TrashFlip)
        if (Mouse.current.rightButton.wasPressedThisFrame) {
            TrashFlip();
        }
    }

    void FixedUpdate() {
        ApplyMovement();
    }

    private void ReadMovementInput() {
        if (Keyboard.current == null) {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;

        moveInput = new Vector2(horizontal, vertical);
        if (moveInput.sqrMagnitude > 1f) {
            moveInput.Normalize();
        }
    }

    private void ApplyMovement() {
        Vector2 velocity = moveInput * moveSpeed;

        if (usePhysicsMovement && body != null) {
            body.linearVelocity = velocity;
            return;
        }

        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimationParameters() {
        if (characterAnimator == null) {
            return;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.0001f;
        if (isMoving) {
            lastMoveDirection = moveInput;
        }

        characterAnimator.SetFloat(moveXHash, moveInput.x);
        characterAnimator.SetFloat(moveYHash, moveInput.y);
        characterAnimator.SetFloat(lastMoveXHash, lastMoveDirection.x);
        characterAnimator.SetFloat(lastMoveYHash, lastMoveDirection.y);
        characterAnimator.SetFloat(speedHash, moveInput.sqrMagnitude);
        characterAnimator.SetBool(isMovingHash, isMoving);
    }

    private void HandleEInteraction() {
        if (Keyboard.current == null) {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame) {
            isHoldingE = true;
            attemptedRepair = false;
            eHoldTimer = 0f;
            repairTarget = FindNearestRepairableWorkstation();
            if (repairTarget != null) {
                repairTarget.BeginRepair();
                attemptedRepair = true;
            }
        }

        if (isHoldingE && Keyboard.current.eKey.isPressed) {
            eHoldTimer += Time.deltaTime;
            if (repairTarget != null && repairTarget.CanRepairIncident()) {
                attemptedRepair = true;
                repairTarget.UpdateRepair(Time.deltaTime);
            }
        }

        if (Keyboard.current.eKey.wasReleasedThisFrame) {
            bool useTapInteraction = eHoldTimer <= tapThresholdSeconds && !attemptedRepair;

            if (repairTarget != null) {
                repairTarget.CancelRepair();
            }

            if (useTapInteraction) {
                HandlePickupDrop();
            }

            isHoldingE = false;
            attemptedRepair = false;
            eHoldTimer = 0f;
            repairTarget = null;
        }
    }

    private void HandlePickupDrop() {
        if (heldItem == null) {
            PickupItem nearestItem = FindNearestPickupItem();
            if (nearestItem != null) {
                TryPickupItem(nearestItem);
            }
            return;
        }

        CounterSlot nearestCounter = FindNearestCounterSlot();
        if (nearestCounter != null && nearestCounter.TryPlace(heldItem)) {
            heldItem = null;
            return;
        }

        Vector3 dropPosition = transform.position + transform.right * 0.5f;
        PickupItem item = ReleaseHeldItem();
        if (item != null) {
            item.DropToWorld(dropPosition);
        }
    }

    private PickupItem FindNearestPickupItem() {
        int hitCount = CollectNearbyHits();

        float closestDistance = float.MaxValue;
        PickupItem nearest = null;

        for (int i = 0; i < hitCount; i++) {
            PickupItem item = nearbyHitsBuffer[i].GetComponentInParent<PickupItem>();
            if (item == null || item.IsHeld || item.CurrentSlot != null) {
                continue;
            }

            float distance = Vector2.Distance(transform.position, item.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                nearest = item;
            }
        }

        return nearest;
    }

    private CounterSlot FindNearestCounterSlot() {
        int hitCount = CollectNearbyHits();

        float closestDistance = float.MaxValue;
        CounterSlot nearest = null;

        for (int i = 0; i < hitCount; i++) {
            CounterSlot slot = nearbyHitsBuffer[i].GetComponentInParent<CounterSlot>();
            if (slot == null) {
                continue;
            }

            float distance = Vector2.Distance(transform.position, slot.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                nearest = slot;
            }
        }

        return nearest;
    }

    private Workstation FindNearestRepairableWorkstation() {
        int hitCount = CollectNearbyHits();

        float closestDistance = float.MaxValue;
        Workstation nearest = null;

        for (int i = 0; i < hitCount; i++) {
            Workstation ws = nearbyHitsBuffer[i].GetComponentInParent<Workstation>();
            if (ws == null || !ws.CanRepairIncident()) {
                continue;
            }

            float distance = Vector2.Distance(transform.position, ws.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                nearest = ws;
            }
        }

        return nearest;
    }

    private int CollectNearbyHits() {
        return Physics2D.OverlapCircle(transform.position, interactionRadius, interactionFilter, nearbyHitsBuffer);
    }

    private void ConfigureInteractionFilter() {
        interactionFilter = new ContactFilter2D();
        interactionFilter.useLayerMask = true;
        interactionFilter.layerMask = interactionMask;
        interactionFilter.useTriggers = true;
    }

    private void CacheAnimatorHashes() {
        moveXHash = Animator.StringToHash(moveXParam);
        moveYHash = Animator.StringToHash(moveYParam);
        lastMoveXHash = Animator.StringToHash(lastMoveXParam);
        lastMoveYHash = Animator.StringToHash(lastMoveYParam);
        speedHash = Animator.StringToHash(speedParam);
        isMovingHash = Animator.StringToHash(isMovingParam);
    }

    void OnValidate() {
        CacheAnimatorHashes();
        ConfigureInteractionFilter();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}