using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class PlayerController : MonoBehaviour {
    public static PlayerController Instance;
    public List<string> currentHoldings = new List<string>();

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private bool usePhysicsMovement = true;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";

    [Header("Pickup & Drop")]
    [SerializeField] private Transform handAnchor;
    [SerializeField] private float interactionRadius = 1.25f;
    [SerializeField] private LayerMask interactionMask = ~0;
    [SerializeField] private float tapThresholdSeconds = 0.2f;
    [SerializeField] private bool verboseLogs;
    [Header("Interaction UI")]
    [SerializeField] private TextMeshProUGUI workstationHintText;

    private PickupItem heldItem;
    private Cup heldCup; // Cập nhật: thêm cup support
    private float eHoldTimer;
    private bool isHoldingE;
    private bool attemptedRepair;
    private Workstation repairTarget;
    private Workstation nearbyWorkstation;
    private string lastWorkstationHint = string.Empty;
    private readonly Collider2D[] nearbyHitsBuffer = new Collider2D[48];
    private readonly Collider2D[] clickHitsBuffer = new Collider2D[32];
    private Camera cachedMainCamera;
    private Rigidbody2D body;
    private Vector2 moveInput;
    private ContactFilter2D interactionFilter;
    private Animator animator;
    private InputAction moveAction;


    void Awake() {
        Instance = this;
        ConfigureInteractionFilter();
        body = GetComponent<Rigidbody2D>();
    }

    void Start() {
        cachedMainCamera = Camera.main;
        animator = GetComponent<Animator>();
    }

    void OnEnable() {
        BindMoveAction();
    }

    void OnDisable() {
        UnbindMoveAction();
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

        if (heldCup != null) {
            heldCup.Clear();
            Vector3 dropPosition = transform.position + transform.right * 0.5f;
            heldCup.DropToWorld(dropPosition);
            heldCup = null;
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

        if (Keyboard.current != null) {
            HandleEInteraction();
        }

        TryAutoPickupNearbyCup();

        UpdateNearbyWorkstationHint();

        // Click trái: tương tác với Workstation hoặc Customer
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            AudioManager.instance?.OnClickSound();

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

                // Thử tương tác Customer
                Customer customer = col.GetComponent<Customer>();
                if (customer != null) {
                    if (verboseLogs) {
                        Debug.Log("  -> Customer clicked! Holdings: " + string.Join(", ", currentHoldings));
                    }
                    bool success = false;

                    // Thử serve cup trước
                    if (heldCup != null) {
                        if (CupServeValidator.Instance != null) {
                            success = CupServeValidator.Instance.TryServeCupToCustomer(heldCup, customer);
                        } else {
                            success = customer.ReceiveDrink(new List<string>(heldCup.Contents));
                        }

                        if (success) {
                            Destroy(heldCup.gameObject);
                            heldCup = null;
                        }
                        return;
                    }

                    // Rồi thử serve từ heldItem (cup cũ)
                    if (heldItem != null) {
                        success = customer.ReceiveCup(heldItem);
                        if (success) {
                            Destroy(heldItem.gameObject);
                            heldItem = null;
                        }
                    } else {
                        // Hoặc serve từ holdings (cách cũ)
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

    private void BindMoveAction() {
        if (moveAction != null) {
            return;
        }

        if (inputActionsAsset == null) {
            Debug.LogWarning("PlayerController: Missing InputActionAsset reference.", this);
            return;
        }

        InputActionMap actionMap = inputActionsAsset.FindActionMap(actionMapName, false);
        if (actionMap == null) {
            Debug.LogWarning($"PlayerController: Action map '{actionMapName}' not found.", this);
            return;
        }

        moveAction = actionMap.FindAction(moveActionName, false);
        if (moveAction == null) {
            Debug.LogWarning($"PlayerController: Move action '{moveActionName}' not found.", this);
            return;
        }

        moveAction.performed += Move;
        moveAction.canceled += Move;
        actionMap.Enable();
    }

    private void UnbindMoveAction() {
        if (moveAction == null) {
            return;
        }

        InputActionMap actionMap = moveAction.actionMap;
        moveAction.performed -= Move;
        moveAction.canceled -= Move;
        moveAction = null;

        if (actionMap != null) {
            actionMap.Disable();
        }

        moveInput = Vector2.zero;
        if (animator != null) {
            animator.SetBool("isWalking", false);
            animator.SetFloat("InputX", 0f);
            animator.SetFloat("InputY", 0f);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        bool isWalking = input.sqrMagnitude > 0.0001f;

        if (!isWalking && animator != null)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        moveInput = input;

        if (animator == null) {
            return;
        }

        animator.SetBool("isWalking", isWalking);
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    private void ApplyMovement() {
        Vector2 velocity = moveInput * moveSpeed;

        if (usePhysicsMovement && body != null) {
            body.linearVelocity = velocity;
            return;
        }

        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
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
                if (TryInteractNearestWorkstation()) {
                    isHoldingE = false;
                    attemptedRepair = false;
                    eHoldTimer = 0f;
                    repairTarget = null;
                    return;
                }

                HandlePickupDrop();
            }

            isHoldingE = false;
            attemptedRepair = false;
            eHoldTimer = 0f;
            repairTarget = null;
        }
    }

    private void HandlePickupDrop() {
        // Try to pick up cup first
        Cup nearestCup = FindNearestCup();
        if (nearestCup != null && heldCup == null && heldItem == null) {
            TryPickupCup(nearestCup);
            return;
        }

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

    private Cup FindNearestCup() {
        int hitCount = CollectNearbyHits();

        float closestDistance = float.MaxValue;
        Cup nearest = null;

        for (int i = 0; i < hitCount; i++) {
            Cup cup = nearbyHitsBuffer[i].GetComponentInParent<Cup>();
            if (cup == null || cup.IsHeld) {
                continue;
            }

            float distance = Vector2.Distance(transform.position, cup.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                nearest = cup;
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

    private Workstation FindNearestWorkstation() {
        int hitCount = CollectNearbyHits();

        float closestDistance = float.MaxValue;
        Workstation nearest = null;

        for (int i = 0; i < hitCount; i++) {
            Workstation ws = nearbyHitsBuffer[i].GetComponentInParent<Workstation>();
            if (ws == null) {
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

    private void TryAutoPickupNearbyCup() {
        if (heldCup != null || heldItem != null || handAnchor == null) {
            return;
        }

        Cup nearestCup = FindNearestCup();
        if (nearestCup != null) {
            TryPickupCup(nearestCup);
        }
    }

    private bool TryInteractNearestWorkstation() {
        Workstation target = FindNearestWorkstation();
        if (target == null) {
            return false;
        }

        if (verboseLogs) {
            Debug.Log("E interaction with workstation: " + target.gameObject.name);
        }

        target.HandleClick();
        return true;
    }

    private void UpdateNearbyWorkstationHint() {
        nearbyWorkstation = FindNearestWorkstation();

        if (workstationHintText == null) {
            return;
        }

        string nextHint = nearbyWorkstation != null
            ? "E: " + nearbyWorkstation.gameObject.name
            : string.Empty;

        if (nextHint == lastWorkstationHint) {
            return;
        }

        lastWorkstationHint = nextHint;
        workstationHintText.text = nextHint;
        workstationHintText.gameObject.SetActive(!string.IsNullOrEmpty(nextHint));
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



    void OnValidate() {

        ConfigureInteractionFilter();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    /// <summary>
    /// Try to pick up a cup and attach it to hand
    /// </summary>
    public bool TryPickupCup(Cup cup) {
        if (cup == null || heldCup != null || heldItem != null || handAnchor == null) {
            return false;
        }

        heldCup = cup;
        heldCup.AttachToHand(handAnchor);
        return true;
    }

    public bool HasHeldCup() {
        return heldCup != null;
    }

    /// <summary>
    /// Release the held cup
    /// </summary>
    public Cup ReleaseHeldCup() {
        Cup cup = heldCup;
        heldCup = null;
        return cup;
    }

    /// <summary>
    /// Get the currently held cup
    /// </summary>
    public Cup GetHeldCup() {
        return heldCup;
    }

    public bool TryAddIngredientToHeldCup(string ingredient) {
        if (heldCup == null) {
            return false;
        }

        return heldCup.TryAddIngredient(ingredient);
    }

    /// <summary>
    /// Pour an ingredient from holdings into the held cup
    /// </summary>
    public bool TryPourIntoCup(int ingredientIndex) {
        if (heldCup == null || ingredientIndex < 0 || ingredientIndex >= currentHoldings.Count) {
            return false;
        }

        string ingredient = currentHoldings[ingredientIndex];
        if (heldCup.TryAddIngredient(ingredient)) {
            currentHoldings.RemoveAt(ingredientIndex);
            Debug.Log($"Poured {ingredient} into cup. Cup contents: {string.Join(", ", heldCup.Contents)}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Pour all holdings into the held cup
    /// </summary>
    public bool TryPourAllIntoCup() {
        if (heldCup == null || currentHoldings.Count == 0) {
            return false;
        }

        int poured = 0;
        for (int i = currentHoldings.Count - 1; i >= 0; i--) {
            if (heldCup.TryAddIngredient(currentHoldings[i])) {
                currentHoldings.RemoveAt(i);
                poured++;
            } else {
                break; // Cup is full
            }
        }

        Debug.Log($"Poured {poured} ingredients into cup. Cup contents: {string.Join(", ", heldCup.Contents)}");
        return poured > 0;
    }
}