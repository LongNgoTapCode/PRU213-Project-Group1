# 💻 Code Examples & Detailed Explanation

---

## **EXAMPLE 1: Cup System - Từ Data đến Instance**

### **1.1 CupData.cs - Định Nghĩa Data**

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewCup", menuName = "DesolateCoffee/Cup")]
public class CupData : ScriptableObject {
    public string cupName;
    public Sprite cupSprite;
    public float capacity = 1f;
    public Color cupColor = Color.white;
}
```

**Giải Thích:**
```
[CreateAssetMenu(...)]
    ↓
Tạo menu option "Create → DesolateCoffee/Cup"
Khi click, tạo asset file .asset chứa CupData instance

Tại sao cần?
✓ Data tách biệt từ code
✓ Designer có thể tạo nhiều cup variants
✓ Easy to balance: change capacity, adjust color
✓ Reusable: same asset → nhiều prefab
```

**Ví dụ sử dụng:**
```csharp
// Trong editor: Create → DesolateCoffee/Cup → SetName "BasicCup"
// Later, trong Cup.cs:

[SerializeField] private CupData cupData;

void Start() {
    if (cupData != null && spriteRenderer != null) {
        spriteRenderer.sprite = cupData.cupSprite;  // ← Lấy sprite từ asset
        spriteRenderer.color = cupData.cupColor;    // ← Lấy color
        // capacity sẽ dùng trong TryAddIngredient()
    }
}
```

---

### **1.2 Cup.cs - Cup Object**

```csharp
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
    public IReadOnlyList<string> Contents => contents;  // ← Only read, not write
    public bool IsHeld => isHeld;
    public bool IsFull => contents.Count >= cupData.capacity;

    // ─── STARTUP ───
    void Awake() {
        body = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        // Auto-add collider if missing
        if (GetComponent<Collider2D>() == null) {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.8f);  // ← Cup size
        }
    }

    void Start() {
        // Apply cup data to visual
        if (cupData != null && spriteRenderer != null) {
            if (cupData.cupSprite != null) {
                spriteRenderer.sprite = cupData.cupSprite;
            }
            spriteRenderer.color = cupData.cupColor;  // ← Brown/white color
        }
    }

    // ─── EVERY FRAME ───
    void Update() {
        // If cup in hand, move with hand
        if (isHeld && handAnchor != null) {
            transform.position = handAnchor.position;
            transform.rotation = handAnchor.rotation;
        }
    }

    // ─── PUBLIC API ───
    
    /// <summary>
    /// Add ingredient to cup (respects capacity)
    /// </summary>
    public bool TryAddIngredient(string ingredient) {
        if (IsFull || string.IsNullOrWhiteSpace(ingredient)) {
            return false;  // ← Cup full or empty ingredient name
        }

        contents.Add(ingredient.Trim());  // ← "ESPRESSO" → "Espresso"
        return true;
    }

    /// <summary>
    /// Attach cup to player's hand
    /// </summary>
    public void AttachToHand(Transform anchor) {
        if (anchor == null) return;

        handAnchor = anchor;
        isHeld = true;
        transform.SetParent(null);  // ← Not child of anything (follow anchor)

        if (body != null) {
            body.linearVelocity = Vector2.zero;  // ← Stop moving
            body.angularVelocity = 0f;           // ← Stop rotating
            body.simulated = false;              // ← No physics while held
        }

        GetComponent<Collider2D>().isTrigger = true;  // ← Become trigger
    }

    /// <summary>
    /// Drop cup to world (no longer held)
    /// </summary>
    public void DropToWorld(Vector3 worldPosition) {
        isHeld = false;
        handAnchor = null;
        transform.position = worldPosition;

        if (body != null) {
            body.simulated = true;  // ← Re-enable physics
        }

        GetComponent<Collider2D>().isTrigger = false;  // ← Solid again
    }

    /// <summary>
    /// Clear all contents (empty the cup)
    /// </summary>
    public void Clear() {
        contents.Clear();
    }
}
```

**Giải Thích Chi Tiết:**

```csharp
// 1. Contents property
private readonly List<string> contents = new List<string>();
public IReadOnlyList<string> Contents => contents;

Vì sao IReadOnlyList?
✓ Prevent external code từ modify contents directly
✓ External can only read, not Add/Remove
✓ Cup controls thêm bao nhiêu ingredient
✓ Nếu public List<string> → bất cứ ai đều modify được

Example (BAD):
    cup.contents.Add("Extra"); // ← Bypass TryAddIngredient()

Example (GOOD):
    cup.TryAddIngredient("Extra"); // ← Check capacity first
```

```csharp
// 2. IsFull property
public bool IsFull => contents.Count >= cupData.capacity;

Vì sao?
✓ Capacity từ CupData (configurable)
✓ Not hardcoded = flexibility
✓ Different cup types có capacity khác
✓ Easy to balance: adjust capacity asset

Example:
    BasicCup capacity = 3
    ExpensiveCup capacity = 5
    QuickCup capacity = 1
```

```csharp
// 3. Collider trigger switch
GetComponent<Collider2D>().isTrigger = true;  // When held
GetComponent<Collider2D>().isTrigger = false; // When dropped

Vì sao?
✓ When held:
  - isHeld = true → Cup in player hand
  - isHeld = true AND isTrigger = true
    → Players can move through cup (it's in hand)
  - Prevent collision with other objects when held

✓ When dropped:
  - isHeld = false → Cup on ground
  - isTrigger = false
    → Cup become solid
    → Can collide with counter, ground
    → Physics apply normally
```

---

## **EXAMPLE 2: Pouring System - Ingredients to Cup**

### **2.1 PlayerController.TryPourAllIntoCup()**

```csharp
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
    public List<string> currentHoldings = new List<string>();
    private Cup heldCup;

    /// <summary>
    /// Pour all ingredients from hands into held cup
    /// </summary>
    public bool TryPourAllIntoCup() {
        // Step 1: Validate prerequisites
        if (heldCup == null) {
            Debug.Log("No cup in hand!");
            return false;
        }

        if (currentHoldings.Count == 0) {
            Debug.Log("No ingredients in hand!");
            return false;
        }

        // Step 2: Iterate and pour
        int poured = 0;
        // Reverse loop để avoid index shifting khi remove
        for (int i = currentHoldings.Count - 1; i >= 0; i--) {
            // Try add to cup
            if (heldCup.TryAddIngredient(currentHoldings[i])) {
                // Success! Remove from hand
                currentHoldings.RemoveAt(i);
                poured++;
            } else {
                // Cup full, stop trying
                Debug.Log("Cup is full!");
                break;
            }
        }

        // Step 3: Log result
        if (poured > 0) {
            Debug.Log($"Poured {poured} ingredients. Cup now has: " +
                     string.Join(", ", heldCup.Contents));
            return true;
        }

        return false;
    }
}
```

**Giải Thích:**

```
Vì sao reverse loop?
================================

❌ Forward loop (BAD):
    currentHoldings = ["Espresso", "Milk", "Sugar"]
    
    i=0: Remove "Espresso"
         holdings = ["Milk", "Sugar"] ← Size changed!
    i=1: Try remove index 1
         holdings[1] = "Sugar" (skipped Milk!) ❌

✅ Reverse loop (GOOD):
    currentHoldings = ["Espresso", "Milk", "Sugar"]
    
    i=2: Remove "Sugar"
         holdings = ["Espresso", "Milk"] ← Index 0 & 1 unchanged
    i=1: Remove "Milk"
         holdings = ["Espresso"] ← Index 0 unchanged
    i=0: Remove "Espresso"
         holdings = [] ✓
```

```
Vì sao check IsFull từ Cup?
================================

TryAddIngredient(ing) bên trong Cup:

protected bool IsFull => contents.Count >= cupData.capacity;

public bool TryAddIngredient(string ingredient) {
    if (IsFull) {
        return false;  // ← Reject, cup full
    }
    contents.Add(ingredient);
    return true;
}

Lợi ích:
✓ Cup là source of truth về capacity
✓ Nếu capacity change → tự apply everywhere
✓ No need check capacity in PlayerController
```

---

## **EXAMPLE 3: Serving & Validation**

### **3.1 CupServeValidator.TryServeCupToCustomer()**

```csharp
using UnityEngine;

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
    /// Serve cup to customer and validate order
    /// Returns: true if correct, false if wrong
    /// </summary>
    public bool TryServeCupToCustomer(Cup cup, Customer customer) {
        // ──── STEP 1: VALIDATE INPUTS ────
        if (cup == null || customer == null) {
            Debug.LogWarning("Cup or customer is null!");
            return false;
        }

        // ──── STEP 2: GET DATA ────
        var requiredIngredients = customer.GetRequiredIngredients();
        var cupContents = cup.Contents;

        Debug.Log($"Customer wants: {string.Join(", ", requiredIngredients)}");
        Debug.Log($"Cup has: {string.Join(", ", cupContents)}");

        // ──── STEP 3: VALIDATE MATCH ────
        // RecipeMatcher.IsExactMatch() checks:
        // ✓ Same ingredients
        // ✓ Same count
        // ✓ Unordered (order doesn't matter)
        bool isCorrect = RecipeMatcher.IsExactMatch(cupContents, requiredIngredients);

        Debug.Log($"Match result: {isCorrect}");

        // ──── STEP 4: HANDLE RESULT ────
        if (isCorrect) {
            return HandleCorrectServe(cup, customer);
        } else {
            return HandleWrongServe(cup, customer);
        }
    }

    private bool HandleCorrectServe(Cup cup, Customer customer) {
        // ✅ CORRECT ORDER PATH
        
        Debug.Log("✓ CORRECT ORDER!");

        // 1. Calculate reward
        int reward = GetReward(customer);
        Debug.Log($"Reward: {reward} coins");

        // 2. Award coins to player
        if (GameManager.Instance != null) {
            GameManager.Instance.AddCoins(reward);
            Debug.Log($"Coins awarded! Total: {GameManager.Instance.coins}");
        }

        // 3. Mark order as complete in system
        if (OrderManager.Instance != null) {
            OrderManager.Instance.CompleteOrder(customer);
            Debug.Log("Order marked complete");
        }

        // 4. Play celebratory sound
        if (AudioFeedbackManager.Instance != null) {
            AudioFeedbackManager.Instance.PlayOrderCompleteChime();  // ♫
        }

        // 5. Make customer leave happy
        customer.Satisfy();  // Will call StartLeaving() → FinalizeLeaving() → Destroy()

        return true;
    }

    private bool HandleWrongServe(Cup cup, Customer customer) {
        // ❌ WRONG ORDER PATH

        Debug.Log("✗ WRONG ORDER!");
        Debug.Log($"Customer wanted: {string.Join(", ", customer.GetRequiredIngredients())}");
        Debug.Log($"Got: {string.Join(", ", cup.Contents)}");

        // 1. Lose reputation
        if (GameManager.Instance != null) {
            GameManager.Instance.LoseReputation();
            Debug.Log($"Reputation lost! Now: {GameManager.Instance.reputation}");
            
            // Check if game over
            if (GameManager.Instance.reputation <= 0) {
                Debug.Log("GAME OVER - No reputation left!");
            }
        }

        // 2. Mark order as failed in system
        if (OrderManager.Instance != null) {
            OrderManager.Instance.FailOrder(customer);
            Debug.Log("Order marked failed");
        }

        // 3. Make customer leave angry
        customer.Leave();  // Will call LeaveAngry() → StartLeaving()

        return false;
    }

    private int GetReward(Customer customer) {
        // Base reward: fixed amount
        int reward = 10;

        // Bonus: served before customer got impatient
        // If customer still has > 50% patience remaining
        if (customer.RemainingTime > customer.MaxPatience * 0.5f) {
            reward += 5;  // Speed bonus!
        }

        return reward;  // Either 10 or 15 coins
    }
}
```

**Giải Thích:**

```csharp
// Vì sao cả này là centralized?

❌ BAD (Distributed validation):
    // Trong Customer.cs
    public bool ReceiveCup(Cup cup) {
        if (RecipeMatcher.IsExactMatch(...)) {
            AddCoins(...);  // ← Nơi 1
            ...
        }
    }
    
    // Trong PlayerController.cs
    if (served successfully) {
        GameManager.AddCoins(...);  // ← Nơi 2
    }

Problem: Coins added twice? Or miss cases?
         Different flow in 2 places
         Hard to maintain

✅ GOOD (Centralized validation):
    CupServeValidator.TryServeCupToCustomer()
    ├─ Validate
    ├─ Award coins (1 place)
    ├─ Mark complete (1 place)
    └─ Everything atomic
```

---

## **EXAMPLE 4: Coin System**

### **4.1 GameManager.cs - Coin Management**

```csharp
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    // ──── GAME STATE ────
    public int coins = 0;       // Current coins (upgrades spend from this)
    public int reputation = 5;  // Hearts (game over if 0)
    
    // ──── UI REFERENCES ────
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI reputationText;

    void Awake() {
        Instance = this;  // ← Singleton pattern (ensure one manager)
    }

    void Start() {
        UpdateUI();  // ← Show initial values
    }

    // ──── COIN OPERATIONS ────

    /// <summary>
    /// Add coins to player (when serve correct)
    /// </summary>
    public void AddCoins(int amount) {
        coins += amount;

        // Validate amount
        if (amount < 0) {
            Debug.LogWarning("AddCoins called with negative amount!");
        }

        Debug.Log($"Coins +{amount} → Total: {coins}");
        UpdateUI();
    }

    /// <summary>
    /// Spend coins (when buy upgrade)
    /// Returns true if success, false if insufficient coins
    /// </summary>
    public bool SpendCoins(int amount) {
        // Check if can afford
        if (coins < amount) {
            Debug.Log($"Not enough coins! Have {coins}, need {amount}");
            return false;  // ← FAIL
        }

        // Player can afford → deduct coins
        coins -= amount;
        Debug.Log($"Coins -{amount} → Total: {coins}");
        UpdateUI();
        return true;  // ← SUCCESS
    }

    // ──── REPUTATION ─────

    public void LoseReputation() {
        reputation--;
        Debug.Log($"Reputation lost → {reputation} / 5 remaining");
        UpdateUI();

        // Check game over condition
        if (reputation <= 0) {
            GameOver();
        }
    }

    // ──── UI ────

    void UpdateUI() {
        if (coinsText != null) {
            coinsText.text = "Coins: " + coins;  // ← Update display
        }

        if (reputationText != null) {
            reputationText.text = "Rep: " + reputation + " / 5";
        }
    }

    void GameOver() {
        Debug.Log("GAME OVER!");
        Time.timeScale = 0f;  // ← Pause game
        // (Show game over screen, etc)
    }
}
```

**Giải Thích:**

```csharp
// Vì sao coins separate từ score?

Old system:
├─ score = all points combined
├─ Hard to track: +5 (serve), +10 (bonus), -3 (penalty)
├─ Confusing: score for what? Ranking? Reward?
└─ Can't use for economy (score too volatile)

New system:
├─ coins = official currency
├─ Only increases from serve: +10, +15
├─ Clear source: serve correct order
├─ Easy balance: adjust coin_per_order
└─ professional: like real games
```

```csharp
// Vì sao bool return from SpendCoins()?

public bool SpendCoins(int amount) {
    if (coins < amount) {
        return false;  // ← Insufficient coins
    }
    coins -= amount;
    return true;   // ← Success
}

Vì sao?
✓ Caller know if operation succeed
✓ Can provide feedback: "Not enough coins"
✓ No side effects if fail (coins unchanged)
✓ Can chain checks:

    Example (UpgradeManager.cs):
    
    bool success = GameManager.Instance.SpendCoins(cost);
    if (!success) {
        // Show "Not enough coins" UI
        return;
    }
    // Continue upgrade logic
    BrewLevel++;
```

---

## **EXAMPLE 5: Coin-to-Upgrade Flow**

### **5.1 UpgradeManager.cs - Using Coins**

```csharp
using UnityEngine;

public class UpgradeManager : MonoBehaviour {
    public static UpgradeManager Instance { get; private set; }

    // ──── LEVELS ────
    public int BrewLevel { get; private set; }
    public int PatienceLevel { get; private set; }
    public int StabilityLevel { get; private set; }

    // ──── EFFECTS ────
    // Brew Lv.1 → Cook 8% faster
    // Brew Lv.2 → Cook 16% faster
    public float BrewTimeMultiplier => 
        Mathf.Clamp(1f - BrewLevel * 0.08f, 0.55f, 1f);
    // Min 0.55f (max 45% faster), Max 1.0f (no effect)

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // Load from persisted data (PlayerPrefs)
        BrewLevel = PlayerPrefs.GetInt("upgrade.brew", 0);
        PatienceLevel = PlayerPrefs.GetInt("upgrade.patience", 0);
        StabilityLevel = PlayerPrefs.GetInt("upgrade.stability", 0);

        Debug.Log($"Loaded upgrades: Brew={BrewLevel}, Patience={PatienceLevel}");
    }

    // ──── COST CALCULATION ────

    public int GetBrewUpgradeCost() {
        // Base 20, increase by 10 per level
        // Lv0: 20  |  Lv1: 30  |  Lv2: 40  |  Lv5: 70
        return 20 + BrewLevel * 10;
    }

    // ──── PURCHASE ────

    public bool BuyBrewUpgrade() {
        // Step 1: Get cost
        int cost = GetBrewUpgradeCost();
        Debug.Log($"Brew upgrade cost: {cost}");

        // Step 2: Try spend coins
        if (!Spend(cost)) {
            Debug.Log("Purchase failed - not enough coins");
            return false;  // ← FAIL
        }

        // Step 3: Upgrade level
        BrewLevel++;
        Debug.Log($"Brew upgrade purchased! Now Lv{BrewLevel}");

        // Step 4: Save to persistent storage
        PlayerPrefs.SetInt("upgrade.brew", BrewLevel);
        PlayerPrefs.Save();  // ← Ensure written to disk
        // Next game start, will load this value

        // Step 5: Update UI
        RefreshUI();
        return true;  // ← SUCCESS
    }

    // ──── INTERNAL ────

    private bool Spend(int amount) {
        // Call GameManager to deduct coins
        // GameManager handles validation + deduction
        if (GameManager.Instance == null) {
            return false;
        }

        return GameManager.Instance.SpendCoins(amount);
    }

    void RefreshUI() {
        // Show current levels and next costs
        Debug.Log(
            "UPGRADES\n" +
            $"Brew Lv{BrewLevel} (Cost {GetBrewUpgradeCost()})\n" +
            $"Patience Lv{PatienceLevel} (Cost {GetPatienceUpgradeCost()})"
        );
    }
}
```

**Giải Thích:**

```csharp
// Vì sao cost scales up?

GetBrewUpgradeCost() = 20 + BrewLevel * 10

Progression:
├─ Lv0 → Lv1:  cost 20  (easy to start)
├─ Lv1 → Lv2:  cost 30  (harder)
├─ Lv2 → Lv3:  cost 40  (very hard)
└─ Lv5 → Lv6:  cost 70  (expensive!)

Vì sao?
✓ Prevent snowball: early upgrade not too easy
✓ Force engagement: need to play multiple runs
✓ Economics: balance late-game power creep
✓ Natural progression: harder as you advance
```

```csharp
// Vì sao PlayerPrefs?

PlayerPrefs.SetInt("upgrade.brew", BrewLevel);

Vì sao?
✓ Persist between game sessions
✓ Player close game → reopen → upgrade still there
✓ Simple key-value storage (no database needed)
✓ Cross-platform (PC, mobile, etc)

Example:
├─ Session 1:
│  ├─ Earn 50 coins
│  ├─ Buy Brew Lv.1 (cost 20)
│  ├─ Game quit
│  └─ PlayerPrefs save: upgrade.brew = 1
│
├─ Session 2 (next day):
│  ├─ Game start → load upgrade.brew = 1
│  ├─ Brew already Lv.1! (persisted)
│  ├─ Earn 30 coins
│  ├─ Buy Brew Lv.2 (cost 30)
│  └─ Total: Brew Lv.2 after 2 sessions
```

---

## **EXAMPLE 6: Complete Flow - Real Scenario**

```
TIME 0s: GAME START
═════════════════════

GameManager.Start():
├─ coins = 0
├─ reputation = 5
└─ UpdateUI() → "Coins: 0", "Rep: 5/5"

UpgradeManager.Awake():
├─ Load from PlayerPrefs
└─ BrewLevel = 1 (from last session)
   ├─ BrewTimeMultiplier = 1.0 - (1 * 0.08) = 0.92
   └─ Workstations cook 8% faster!

HeatDirector.Update():
└─ Every 5-10s: spawn random customer


TIME 5s: CUSTOMER 1 ARRIVES
═════════════════════════════════

Customer spawned:
├─ order = Espresso (DrinkData)
├─ requiredIngredients = ["Espresso"]
├─ maxPatience = 20s (modified by UpgradeManager)
└─ patience bar shows green

OrderManager.RegisterOrder(customer):
└─ activeOrders.Add(customer)

OrderDashboardUI shows:
└─ "1. Espresso [20s]"


TIME 10s: PLAYER GETS INGREDIENT
═════════════════════════════════

Player click Espresso grinder:
├─ Workstation.HandleClick()
├─ startCooking = true
├─ processTime = 3s * BrewTimeMultiplier
│  └─ 3 * 0.92 = 2.76s ← 8% faster!
└─ ProgressBar show counting


TIME 12.76s: GRINDING DONE
═════════════════════════════

Workstation.Update():
├─ timer >= processTime → isReady = true
├─ Spawn PickupItem("Espresso")
└─ ProgressBar hide

Player press E (tap):
├─ FindNearestPickupItem()
├─ TryPickupItem(espresso)
└─ heldItem = espresso

UI updates:
└─ "Holding: Espresso"


TIME 13s: PLAYER PICKUP CUP
════════════════════════════

Player press E (tap):
├─ FindNearestCup()
├─ CupSpawner.cupTaken = true
│  └─ respawnTimer = 0.5s (will respawn at 13.5s)
├─ TryPickupCup(cup)
└─ heldCup = cup


TIME 14s: PLAYER POURS INGREDIENT
══════════════════════════════════

Player right-click:
├─ PlayerController.TryPourAllIntoCup()
├─ Loop through currentHoldings = ["Espresso"]
├─ cup.TryAddIngredient("Espresso")
│  └─ IsFull? false (contains 0, capacity 3)
│  └─ Add → contents.Add("Espresso")
├─ currentHoldings.Remove("Espresso")
└─ currentHoldings = [] (now empty)

Console show:
└─ "Poured 1 ingredients. Cup now has: Espresso"


TIME 15s: PLAYER SERVES CUP
═════════════════════════════

Player left-click customer:
├─ Physics2D.OverlapPoint(mousePos)
├─ Find Customer component
├─ PlayerController.heldCup = cup
│  └─ cup.Contents = ["Espresso"]
└─ CupServeValidator.TryServeCupToCustomer(cup, customer):
   ├─ requiredIngredients = ["Espresso"]
   ├─ cupContents = ["Espresso"]
   ├─ RecipeMatcher.IsExactMatch() = TRUE ✓
   │
   ├─ HandleCorrectServe():
   │  ├─ reward = GetReward(customer)
   │  │  └─ customer.RemainingTime = 20 - 10 = 10s
   │  │  └─ 10 > 20*0.5 = 10 → TRUE
   │  │  └─ reward = 10 + 5 = 15 coins
   │  │
   │  ├─ GameManager.AddCoins(15)
   │  │  └─ coins = 15 ✓
   │  │
   │  ├─ OrderManager.CompleteOrder(customer)
   │  │  └─ CompletedOrders++ = 1
   │  │
   │  ├─ AudioFeedbackManager.PlayOrderCompleteChime()
   │  │  └─ ♫ ding ding (celebration sound)
   │  │
   │  ├─ customer.Satisfy()
   │  │  └─ StartLeaving() → Walk to exit → Destroy()
   │  │
   │  └─ return true

UI updates immediately:
└─ CoinsText = "Coins: 15" ← CHANGED!

Console show:
├─ "✓ CORRECT ORDER!"
├─ "Reward: 15 coins"
├─ "Coins awarded! Total: 15"
└─ "Order marked complete"


TIME 13.5s: CUP RESPAWNS
═════════════════════════

CupSpawner.Update():
├─ respawnTimer <= 0 → SpawnCup()
├─ Instantiate(cupPrefab, spawnPoint, ...)
├─ New cup appear on counter
└─ cupTaken = false


TIME 16s: PLAYER HAS 15 COINS
══════════════════════════════

Player click "Buy Brew Upgrade" button:
├─ Button.onClick() → GameManager.BuyUpgradeBrew()
│  └─ Actually calls: UpgradeManager.BuyBrewUpgrade()
│
├─ UpgradeManager.BuyBrewUpgrade():
│  ├─ cost = GetBrewUpgradeCost()
│  │  └─ 20 + 1*10 = 30 coins
│  │
│  ├─ Spend(30):
│  │  └─ GameManager.SpendCoins(30)
│  │     ├─ coins >= 30? 15 >= 30? FALSE ❌
│  │     └─ return false
│  │
│  └─ return false ← UPGRADE FAILED
│
└─ Show feedback: "Not enough coins"

coins remains = 15 (unchanged)


TIME 20s: CUSTOMER 2 ARRIVES
════════════════════════════

Customer 2 spawned:
├─ order = Latte (DrinkData)
├─ requiredIngredients = ["Espresso", "Milk"]
└─ patience...

OrderDashboardUI.Refresh():
└─ "1. Latte [20s]"


...GAMEPLAY CONTINUES...
Player earn more coins from serving more customers...

TIME 45s: PLAYER NOW HAS 30 COINS
═════════════════════════════════

Player click "Buy Brew Upgrade" again:
├─ UpgradeManager.BuyBrewUpgrade():
│  ├─ cost = 30 (still same)
│  ├─ GameManager.SpendCoins(30)
│  │  ├─ 30 >= 30? YES ✓
│  │  ├─ coins -= 30 → coins = 0
│  │  └─ return true
│  │
│  ├─ BrewLevel++ = 2
│  ├─ BrewTimeMultiplier = 1.0 - 0.16 = 0.84 (16% faster!)
│  ├─ PlayerPrefs.SetInt("upgrade.brew", 2)
│  └─ return true ✓

UI updates:
│
├─ CoinsText = "Coins: 0"
└─ UpgradeInfoText = "Brew Lv.2 (Cost 40)"

Now workstations cook even faster (0.84x time)!

Next run:
├─ Game quit/restart
├─ UpgradeManager load: BrewLevel = 2
└─ Brew effect active immediately!
```

---

**Now you understand complete flow with all details! 🎓**
