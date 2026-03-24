using System.Collections.Generic;
using UnityEngine;
using System;

public class Customer : MonoBehaviour {
    private static int nextOrderId = 1;
    private static Sprite sharedSquareSprite;

    private enum CustomerAiState {
        Entering,
        Waiting,
        Leaving
    }

    public DrinkData order;

    public float maxPatience = 20f;
    private float currentPatience;
    private List<string> requiredIngredients = new List<string>();
    private string displayOrderName = "Unknown";
    private Sprite displayOrderIcon;
    private int rewardCoins;

    private SpriteRenderer barBackground;
    private SpriteRenderer barFill;
    private TextMesh orderLabel;
    private SpriteRenderer orderIcon;
    private float barWidth = 1.2f;
    private float barHeight = 0.15f;
    private bool orderResolved;
    private bool isLeavingCompleted;
    private CustomerAiState aiState = CustomerAiState.Waiting;
    private Transform queueTarget;
    private Transform exitTarget;

    [Header("Customer AI")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float stopDistance = 0.05f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingParam = "isWalking";
    [SerializeField] private string inputXParam = "InputX";
    [SerializeField] private string inputYParam = "InputY";
    [SerializeField] private string lastInputXParam = "LastInputX";
    [SerializeField] private string lastInputYParam = "LastInputY";

    public int OrderId { get; private set; }
    public float RemainingTime => Mathf.Max(0f, currentPatience);
    public float RemainingRatio => maxPatience <= 0f ? 0f : Mathf.Clamp01(currentPatience / maxPatience);
    public string DisplayOrderName => displayOrderName;
    public event Action<Customer> LeftCafe;
    private Vector2 lastMoveDirection = Vector2.down;

    void Awake() {
        OrderId = nextOrderId++;

        // Tự động thêm Collider nếu chưa có
        if (GetComponent<Collider2D>() == null) {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
            Debug.Log("Customer: Auto-added BoxCollider2D");
        }
    }

    void Start() {
        if (animator == null) {
            animator = GetComponent<Animator>();
        }

        if (UpgradeManager.Instance != null) {
            maxPatience *= UpgradeManager.Instance.CustomerPatienceMultiplier;
        }

        currentPatience = maxPatience;
        ResolveOrderData();
        CreatePatienceBar();
        CreateOrderIcon();
        CreateOrderLabel();
        OrderManager.Instance?.RegisterOrder(this);

        if (queueTarget != null) {
            aiState = CustomerAiState.Entering;
        }
    }

    void ResolveOrderData() {
        requiredIngredients.Clear();

        if (order != null) {
            displayOrderName = order.drinkName;
            displayOrderIcon = order.icon;
            rewardCoins = Mathf.RoundToInt(order.basePrice);
            if (order.ingredients != null) {
                requiredIngredients.AddRange(order.ingredients);
            }
            return;
        }

        displayOrderName = "Unknown";
        displayOrderIcon = null;
        rewardCoins = 0;
    }

    void CreateOrderIcon() {
        if (displayOrderIcon == null) {
            return;
        }

        GameObject iconObj = new GameObject("OrderIcon");
        iconObj.transform.SetParent(transform);
        iconObj.transform.localPosition = new Vector3(0, 1.45f, 0);

        orderIcon = iconObj.AddComponent<SpriteRenderer>();
        orderIcon.sprite = displayOrderIcon;
        orderIcon.color = Color.white;
        orderIcon.sortingOrder = 7;
        iconObj.transform.localScale = Vector3.one * 0.5f;
    }

    void CreatePatienceBar() {
        // Background (xám đen)
        GameObject bgObj = new GameObject("BarBG");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        barBackground = bgObj.AddComponent<SpriteRenderer>();
        barBackground.sprite = GetSharedSquareSprite();
        barBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        barBackground.sortingOrder = 5;
        bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        // Fill (xanh lá)
        GameObject fillObj = new GameObject("BarFill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        barFill = fillObj.AddComponent<SpriteRenderer>();
        barFill.sprite = GetSharedSquareSprite();
        barFill.color = Color.green;
        barFill.sortingOrder = 6;
        fillObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);
    }

    void CreateOrderLabel() {
        GameObject textObj = new GameObject("OrderLabel");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 1.1f, 0);
        orderLabel = textObj.AddComponent<TextMesh>();
        orderLabel.alignment = TextAlignment.Center;
        orderLabel.anchor = TextAnchor.MiddleCenter;
        orderLabel.characterSize = 0.12f;
        orderLabel.fontSize = 48;
        orderLabel.color = Color.white;
        orderLabel.GetComponent<MeshRenderer>().sortingOrder = 7;

        orderLabel.text = displayOrderName;
    }

    private static Sprite GetSharedSquareSprite() {
        if (sharedSquareSprite != null) {
            return sharedSquareSprite;
        }

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        sharedSquareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return sharedSquareSprite;
    }

    void Update() {
        UpdateMovement();

        if (aiState == CustomerAiState.Waiting) {
            currentPatience -= Time.deltaTime;
        }

        if (barFill != null) {
            float ratio = Mathf.Clamp01(currentPatience / maxPatience);
            barFill.transform.localScale = new Vector3(barWidth * ratio, barHeight, 1f);
            // Giữ thanh fill căn trái
            float offset = (barWidth - barWidth * ratio) * 0.5f;
            barFill.transform.localPosition = new Vector3(-offset, 0.8f, 0);

            // Đổi màu
            if (ratio > 0.5f) barFill.color = Color.green;
            else if (ratio > 0.25f) barFill.color = Color.yellow;
            else barFill.color = Color.red;
        }

        if (currentPatience <= 0) {
            LeaveAngry();
        }
    }

    public bool ReceiveDrink(List<string> servedIngredients) {
        if (requiredIngredients.Count == 0) return false;
        if (IsMatch(servedIngredients)) {
            orderResolved = true;
            OrderManager.Instance?.CompleteOrder(this);
            AudioFeedbackManager.Instance?.PlayOrderCompleteChime();
            Debug.Log("Served " + displayOrderName + " Successfully! +" + rewardCoins + " coins");
            GameManager.Instance?.AddScore(10);
            GameManager.Instance?.AddCoins(rewardCoins);
            StartLeaving();
            return true;
        } else {
            Debug.Log("Wrong order! Customer wants: " + string.Join(", ", requiredIngredients));
            return false;
        }
    }

    public bool ReceiveCup(PickupItem cupItem) {
        if (cupItem == null) {
            return false;
        }

        CupContents cup = cupItem.GetComponent<CupContents>();
        if (cup != null && cup.Ingredients != null && cup.Ingredients.Count > 0) {
            return ReceiveDrink(new List<string>(cup.Ingredients));
        }

        return ReceiveDrink(new List<string> { cupItem.IngredientName });
    }

    void LeaveAngry() {
        if (orderResolved) {
            return;
        }

        orderResolved = true;
        OrderManager.Instance?.FailOrder(this);
        Debug.Log("Customer left... Reputation down!");
        GameManager.Instance?.LoseReputation();
        StartLeaving();
    }

    void OnDestroy() {
        if (!orderResolved && !isLeavingCompleted) {
            OrderManager.Instance?.FailOrder(this);
            orderResolved = true;
        }
    }

    bool IsMatch(List<string> served) {
        return RecipeMatcher.IsExactMatch(served, requiredIngredients);
    }

    public void ConfigureAiTargets(Transform waitingSpot, Transform leaveSpot) {
        queueTarget = waitingSpot;
        exitTarget = leaveSpot;
        aiState = queueTarget != null ? CustomerAiState.Entering : CustomerAiState.Waiting;
    }

    public void SetQueueTarget(Transform waitingSpot) {
        queueTarget = waitingSpot;
        if (aiState != CustomerAiState.Leaving) {
            aiState = queueTarget != null ? CustomerAiState.Entering : CustomerAiState.Waiting;
        }
    }

    private void UpdateMovement() {
        Transform target = null;
        if (aiState == CustomerAiState.Entering) {
            target = queueTarget;
        } else if (aiState == CustomerAiState.Leaving) {
            target = exitTarget;
        }

        if (target == null) {
            if (aiState == CustomerAiState.Entering) {
                aiState = CustomerAiState.Waiting;
            }
            UpdateAnimatorState(Vector2.zero, false);
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 toTarget = targetPos - currentPos;

        if (toTarget.sqrMagnitude > 0.0001f) {
            lastMoveDirection = toTarget.normalized;
        }

        Vector3 nextPosition = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        transform.position = nextPosition;

        bool isWalking = Vector3.Distance(transform.position, target.position) > stopDistance;
        UpdateAnimatorState(lastMoveDirection, isWalking);

        if (!isWalking) {
            if (aiState == CustomerAiState.Entering) {
                aiState = CustomerAiState.Waiting;
            } else if (aiState == CustomerAiState.Leaving) {
                FinalizeLeaving();
            }
        }
    }

    private void UpdateAnimatorState(Vector2 direction, bool isWalking) {
        if (animator == null) {
            return;
        }

        animator.SetBool(walkingParam, isWalking);

        if (!isWalking) {
            animator.SetFloat(lastInputXParam, lastMoveDirection.x);
            animator.SetFloat(lastInputYParam, lastMoveDirection.y);
            animator.SetFloat(inputXParam, 0f);
            animator.SetFloat(inputYParam, 0f);
            return;
        }

        animator.SetFloat(inputXParam, direction.x);
        animator.SetFloat(inputYParam, direction.y);
    }

    private void StartLeaving() {
        aiState = CustomerAiState.Leaving;
        if (barBackground != null) {
            barBackground.gameObject.SetActive(false);
        }
        if (barFill != null) {
            barFill.gameObject.SetActive(false);
        }

        if (orderLabel != null) {
            orderLabel.gameObject.SetActive(false);
        }

        if (orderIcon != null) {
            orderIcon.gameObject.SetActive(false);
        }

        if (exitTarget == null) {
            FinalizeLeaving();
        }
    }

    private void FinalizeLeaving() {
        if (isLeavingCompleted) {
            return;
        }

        isLeavingCompleted = true;
        LeftCafe?.Invoke(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Returns the required ingredients for this customer's order
    /// </summary>
    public List<string> GetRequiredIngredients() {
        return requiredIngredients;
    }

    /// <summary>
    /// Mark customer as satisfied and make them leave happy
    /// </summary>
    public void Satisfy() {
        if (orderResolved) {
            return;
        }

        orderResolved = true;
        AudioFeedbackManager.Instance?.PlayOrderCompleteChime();
        StartLeaving();
    }

    /// <summary>
    /// Make customer leave (used when wrong order is served)
    /// </summary>
    public void Leave() {
        LeaveAngry();
    }

    /// <summary>
    /// Expose max patience as a public property
    /// </summary>
    public float MaxPatience => maxPatience;
}