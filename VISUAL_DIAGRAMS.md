# 🎯 Quick Reference & Visual Diagrams

---

## **DIAGRAM 1: SCENE HIERARCHY**

```
🎮 Scene - DemoLevel
├─ 🧑 Player
│  ├─ Body (SpriteRenderer)
│  ├─ HandAnchor (Transform)
│  │  └─ 📌 Cup/Item attach here when held
│  ├─ PlayerController.cs
│  │  ├─ Hand Anchor → point to HandAnchor
│  │  ├─ Interaction Radius = 1.25f
│  │  └─ On Update:
│  │     ├─ Read WASD input
│  │     ├─ Right-click → TryPourAllIntoCup()
│  │     └─ Left-click → Serve cup to customer
│  │
│  └─ Collider2D (BoxCollider)
│
├─ 🏪 CupCounter (Counter top)
│  ├─ SpriteRenderer (counter visual)
│  ├─ CupSpawner.cs ← MANAGES CUP RESPAWN
│  │  ├─ Cup Prefab → BasicCup prefab
│  │  ├─ Spawn Point → this GameObject
│  │  └─ Respawn Delay = 0.5s
│  │
│  └─ BoxCollider2D
│     ├─ Is Trigger = ON ← Detect when cup taken
│     └─ Size = (0.5, 0.5)
│
├─ ⚙️ Workstations (Grinder, Heater, etc.)
│  ├─ Espresso Grinder
│  │  ├─ Workstation.cs
│  │  │  ├─ Ingredient Output = "Espresso"
│  │  │  ├─ Process Time = 3s
│  │  │  └─ Spawn PickupItem when ready
│  │  │
│  │  └─ PickupItem prefab (spawned)
│  │     ├─ Ingredient Name = "Espresso"
│  │     └─ Can pickup/drop/snap to counter
│  │
│  ├─ Milk Station
│  │  └─ (similar to Grinder)
│  │
│  └─ Sugar Station
│     └─ (similar)
│
├─ 👥 Customers (spawned by HeatDirector)
│  ├─ Customer.cs
│  │  ├─ Order → Latte (DrinkData)
│  │  │  └─ Required: ["Espresso", "Milk"]
│  │  ├─ Max Patience = 20s
│  │  ├─ Patience Bar (visual)
│  │  └─ Order Label (TextMesh "Latte")
│  │
│  └─ Collider2D
│     └─ Left-click → serve cup
│
├─ 🎲 HeatDirector
│  ├─ HeatDirector.cs
│  │  ├─ Customer Prefab
│  │  ├─ Spawn Point
│  │  ├─ Possible Drinks → array of DrinkData
│  │  │  ├─ Espresso
│  │  │  ├─ Latte
│  │  │  └─ Cappuccino
│  │  │
│  │  ├─ Queue Positions → transform array
│  │  │  └─ Customers walk to queue[0], [1], etc
│  │  │
│  │  └─ Every 5-10s: Spawn random customer
│  │
│  └─ Exit Point → où customers leave
│
├─ 🎪 GameManager (Singleton)
│  ├─ GameManager.cs
│  │  ├─ coins = 0 ← MAIN CURRENCY
│  │  ├─ reputation = 5 ← HEALTH
│  │  ├─ AddCoins(amount) ← Called when serve correct
│  │  ├─ SpendCoins(amount) ← Called when buy upgrade
│  │  └─ LoseReputation() ← Called when wrong serve
│  │
│  ├─ Score Text → "Score: 0" (legacy, can remove)
│  ├─ Coins Text → "Coins: 0" ← UPDATE THIS
│  ├─ Reputation Text → "Rep: 5/5"
│  └─ Holdings Text → "Holding: Espresso + Milk"
│
├─ 📋 OrderManager (Singleton)
│  ├─ OrderManager.cs
│  │  ├─ activeOrders dict ← track current orders
│  │  ├─ CompletedOrders = 0 ← stat
│  │  ├─ FailedOrders = 0 ← stat
│  │  │
│  │  ├─ RegisterOrder(customer) ← when arrive
│  │  ├─ CompleteOrder(customer) ← when serve correct
│  │  └─ FailOrder(customer) ← when wrong serve
│  │
│  └─ OrdersChanged event → notify UI
│
├─ 🔧 UpgradeManager (Singleton)
│  ├─ UpgradeManager.cs
│  │  ├─ BrewLevel = 0 (Lv0→5)
│  │  │  └─ BrewTimeMultiplier = 1f - (BrewLevel * 0.08f)
│  │  │     = 1.0 (Lv0), 0.92 (Lv1), 0.84 (Lv2)
│  │  │     ← Cook 8% faster per level
│  │  │
│  │  ├─ PatienceLevel = 0
│  │  │  └─ CustomerPatienceMultiplier = 1f + (Patience * 0.12f)
│  │  │     ← Customers 12% patient per level
│  │  │
│  │  ├─ StabilityLevel = 0
│  │  │  └─ ChaosIntervalMultiplier = 1f + (Stability * 0.15f)
│  │  │     ← Incidents 15% less frequent per level
│  │  │
│  │  ├─ GetBrewUpgradeCost() → 20 + (BrewLevel * 10)
│  │  ├─ BuyBrewUpgrade() → check coins, decrease, level++
│  │  └─ Save to PlayerPrefs ← PERSIST between runs
│  │
│  └─ Upgrade Info Text → "Brew Lv.1 (Cost 30)"
│
├─ ⚡ ChaosIncidentDirector
│  ├─ ChaosIncidentDirector.cs
│  │  ├─ Every 15-30s: trigger random incident
│  │  ├─ MachineBroken → need hold E for 2s repair
│  │  ├─ MilkSpill → increase brew time 1.6x
│  │  └─ OutOfBeans → can't use this workstation
│  │
│  └─ UpgradeManager.StabilityIntervalMultiplier effect
│     └─ ← Stability upgrade reduce incident frequency
│
├─ 🎨 UI Canvas
│  ├─ ScoreText (legacy)
│  ├─ CoinsText ← MAIN DISPLAY
│  ├─ ReputationText
│  ├─ HoldingsText → "Holding: Espresso + Milk"
│  └─ UpgradeButtons
│     ├─ BuyBrewButton → onClick = GameManager.BuyUpgradeBrew()
│     ├─ BuyPatienceButton → GameManager.BuyUpgradePatience()
│     └─ BuyStabilityButton → GameManager.BuyUpgradeStability()
│
└─ 🔊 AudioFeedbackManager (Singleton)
   ├─ AudioFeedbackManager.cs
   │  ├─ PlayOrderCompleteChime() ← ding ding
   │  └─ PlayIncidentAlert() ← warning beep
   │
   └─ Audio Sources (for loop grinder sounds)
```

---

## **DIAGRAM 2: COMPONENT DEPENDENCY**

```
                    PlayerController
                          │
            ┌─────────────┼─────────────┐
            │             │             │
         Input       Pickup/Drop    Click Serve
            │             │             │
            │      ┌──────┘             │
            │      │                    │
            ▼      ▼                    ▼
         Rigidbody2D     Cup.cs    CupServeValidator
            │             │             │
            └──────┬───────┘             │
                   │                     │
            ▼      ▼                     ▼
         GameManager ◄─────────────────────┘
         (add coins)
            │
            ├─ UpdateUI()
            │  ├─ ScoreText (old)
            │  ├─ CoinsText ← MAIN
            │  └─ ReputationText
            │
            └─ LoseReputation()
               └─ Check if rep <= 0 → GameOver()


    OrderManager ◄────────────┬─────────────┐
    (track orders) │          │             │
       │        CompleteOrder  │         FailOrder
       │           │          │             │
       ├─ OrderDashboardUI    │             │
       │   (show pending)  Server          │
       │                 Success        Failure
       │                   │               │
       └─ OrdersChanged     │          GameManager
           event ►OrderDash │          LoseReputation()
                            │
                     Customer.Satisfy()
                     (leave happy)


    UpgradeManager ◄────── GameManager.SpendCoins()
    (manage levels)        │
       │                   │
       ├─ BrewLevel    button click → BuyBrewUpgrade()
       │  └─ Workstation.processTime *= BrewTimeMultiplier
       │
       ├─ PatienceLevel
       │  └─ Customer.maxPatience *= CustomerPatienceMultiplier
       │
       └─ StabilityLevel
          └─ ChaosIncidentDirector ×ChaosIntervalMultiplier
             (less frequent incidents)
```

---

## **DIAGRAM 3: CUP LIFECYCLE**

```
┌─ CUP LIFECYCLE ─────────────────────────────────┐
│                                                 │
│  [Start]                                        │
│    │                                            │
│    ├─ CupSpawner.SpawnCup()                    │
│    │  └─ Instantiate(cupPrefab, spawnPoint)   │
│    │                                            │
│    ▼                                            │
│  [Respawned on Counter]                        │
│    │                                            │
│    ├─ Cup Object exist in world                │
│    ├─ position = spawnPoint                    │
│    ├─ isHeld = false                           │
│    ├─ Contents = [] (empty)                    │
│    └─ Collider2D trigger ON ← can detect       │
│       │                                         │
│       ├─ Player E (tap) near cup               │
│       │  └─ PlayerController.FindNearestCup() │
│       │     (raycast/overlap circle)           │
│       │     └─ return cup                      │
│       │                                         │
│       ▼                                         │
│  [Cup Picked Up]                               │
│    │                                            │
│    ├─ Cup.AttachToHand(handAnchor)             │
│    ├─ isHeld = true                            │
│    ├─ position = handAnchor.position (update) │
│    ├─ Collider isTrigger = true (no physics) │
│    └─ Player has heldCup reference             │
│       │                                         │
│       ├─ If Player right-click (RMB)           │
│       │  └─ PlayerController.TryPourAllIntoCup()│
│       │     ├─ for each in currentHoldings:    │
│       │     │  └─ cup.TryAddIngredient(ing)   │
│       │     │     └─ if (IsFull) break         │
│       │     └─ Remove from holdings            │
│       │        │                                │
│       │        ▼                                │
│       │  [Cup with Contents]                   │
│       │     ├─ cup.Contents = ["Espresso", ...]│
│       │     └─ IsFull = (count >= capacity)   │
│       │                                         │
│       ├─ If Player left-click Customer         │
│       │  └─ CupServeValidator.TryServeCup...() │
│       │     ├─ Validate contents vs order      │
│       │     ├─ if CORRECT:                     │
│       │     │  ├─ GameManager.AddCoins(reward)│
│       │     │  └─ Destroy(cup.gameObject)     │
│       │     │                                  │
│       │     │     ▼                            │
│       │     │  [Cup Destroyed] ✓              │
│       │     └─ if WRONG:                       │
│       │        └─ Destroy(cup.gameObject)     │
│       │           ├─ GameManager.LoseReputation│
│       │           │                            │
│       │           ▼                            │
│       │        [Cup Destroyed] ✗              │
│       │                                         │
│       └─ CupSpawner detect cup missing         │
│          └─ respawnTimer = 0.5s ← start wait  │
│             │                                  │
│             └─ After 0.5s                      │
│                └─ SpawnCup() again ◄───┐      │
│                   └─ return to top ────┘      │
│                                                 │
└─────────────────────────────────────────────────┘
```

---

## **DIAGRAM 4: SERVE VALIDATION FLOW**

```
Left-Click Customer
        │
        ▼
PlayerController.Update()
├─ Physics2D.OverlapPoint(mousePos)
├─ Find Customer component
│        │
│        ▼
│    if (heldCup != null)
│        │
│        ▼
│    CupServeValidator.TryServeCupToCustomer()
│        │
│        ├─ Get cup.Contents
│        │  └─ e.g., ["Espresso", "Milk"]
│        │
│        ├─ Get customer.GetRequiredIngredients()
│        │  └─ e.g., ["Milk", "Espresso"]
│        │
│        ├─ RecipeMatcher.IsExactMatch()
│        │  └─ same items? (unordered)
│        │     └─ TRUE ✓ (order doesn't matter)
│        │
│        ▼
│    [VALIDATION BRANCH]
│
│    if (isCorrect) ──────────────────────────┐
│                                             │
│    ✅ CORRECT PATH:                        │
│    ├─ int reward = GetReward()             │
│    │  └─ base 10 + speed bonus = 15        │
│    │                                       │
│    ├─ GameManager.AddCoins(15)             │
│    │  └─ coins = 15 ✓                      │
│    │                                       │
│    ├─ OrderManager.CompleteOrder()         │
│    │  └─ CompletedOrders++ = 1 ✓           │
│    │                                       │
│    ├─ AudioFeedbackManager.Play Chime()    │
│    │  └─ ♫ ding ding                       │
│    │                                       │
│    ├─ customer.Satisfy()                   │
│    │  └─ ordersResolved = true             │
│    │  └─ StartLeaving() → exit            │
│    │     └─ FinalizeLeaving() → Destroy()│
│    │                                       │
│    └─ return true                          │
│                                             │
│ else ────────────────────────────────────┐ │
│                                          │ │
│ ❌ WRONG PATH:                           │ │
│ ├─ GameManager.LoseReputation()          │ │
│ │  └─ reputation-- = 4 ✗                 │ │
│ │  └─ Check if rep <= 0 → GameOver()     │ │
│ │                                        │ │
│ ├─ OrderManager.FailOrder()              │ │
│ │  └─ FailedOrders++ = 1 ✗               │ │
│ │                                        │ │
│ ├─ customer.Leave()                      │ │
│ │  └─ LeaveAngry() → exit               │ │
│ │     └─ Destroy() unhappy              │ │
│ │                                        │ │
│ └─ return false                          │ │
│                                          │ │
└──────────────────────────────────────────┘ │
                                              │
Back to main update:
├─ if (success)
│  └─ Destroy(heldCup.gameObject)
│  └─ heldCup = null
│
└─ CupSpawner detect cup gone
   └─ respawn new cup after 0.5s
```

---

## **QUICK SETUP CHECKLIST**

```
Scene Setup Checklist:
─────────────────────

☐ Player GameObject
  ☐ PlayerController.cs attached
  ☐ HandAnchor child created
  ☐ Hand Anchor field → point to HandAnchor
  ☐ Interaction Radius = 1.25
  ☐ Rigidbody2D (Physics)
  ☐ BoxCollider2D

☐ CupCounter GameObject
  ☐ CupSpawner.cs attached
  ☐ Cup Prefab field → BasicCup prefab
  ☐ Spawn Point field → this GameObject
  ☐ BoxCollider2D (trigger = ON)
  ☐ Respawn Delay = 0.5s

☐ Workstations (Grinder, etc)
  ☐ Workstation.cs attached
  ☐ Ingredient Output = "Espresso" (etc)
  ☐ Process Time = 3s
  ☐ Output Prefab = PickupItem
  ☐ Collider2D

☐ HeatDirector
  ☐ HeatDirector.cs attached
  ☐ Customer Prefab = Customer prefab
  ☐ Spawn Point = transform
  ☐ Possible Drinks = [Espresso, Latte, ...]
  ☐ Queue Positions = array of 3-5 transforms
  ☐ Exit Point = transform

☐ GameManager
  ☐ GameManager.cs attached
  ☐ Coins Text field → CoinsText UI
  ☐ Score Text field → ScoreText UI
  ☐ Reputation Text → ReputationText UI
  ☐ Game Over Panel → panel GameObject

☐ OrderManager
  ☐ OrderManager.cs attached
  ☐ (no fields to assign)

☐ UpgradeManager
  ☐ UpgradeManager.cs attached
  ☐ Upgrade Info Text → UpgradeInfoText UI

☐ ChaosIncidentDirector
  ☐ ChaosIncidentDirector.cs attached
  ☐ Target Workstations = array of workstations
  ☐ OR Auto Find = true

☐ AudioFeedbackManager
  ☐ AudioFeedbackManager.cs attached
  ☐ Grinder Loop Clip → audio asset
  ☐ Order Complete Chime → audio asset
  ☐ Incident Alert → audio asset

☐ UI Canvas
  ☐ Canvas created
  ☐ ScoreText → TextMeshProUGUI
  ☐ CoinsText → TextMeshProUGUI (NEW)
  ☐ ReputationText → TextMeshProUGUI
  ☐ Upgrade Buttons created
  ☐ Button onClick → GameManager methods

Assets Created:
───────────────

☐ CupData/BasicCup.asset
  ☐ Cup Name = "Basic Cup"
  ☐ Capacity = 3
  ☐ Color = white/brown

☐ Prefabs/Cups/CupPrefab.prefab
  ☐ Cup.cs component
  ☐ SpriteRenderer
  ☐ Rigidbody2D
  ☐ BoxCollider2D

☐ DrinkData assets
  ☐ Espresso.asset (ingredients: [Espresso])
  ☐ Latte.asset (ingredients: [Espresso, Milk])
  ☐ Cappuccino.asset ([Espresso, Milk, Foam])

Test Checklist:
───────────────

☐ Play → Cup spawns on counter
☐ E key → pickup cup (follows hand)
☐ Gather ingredient from workstation
☐ RMB → pour into cup (Console show contents)
☐ Left-click customer with cup
  ☐ If correct → coins +X, customer leave
  ☐ If wrong → rep -1, customer leave
☐ Check coins display update
☐ Buy upgrade → coins decrease
☐ Run multiple times, verify coins accumulate
```

---

## **TROUBLESHOOTING FLOWCHART**

```
Problem: Cup doesn't spawn
├─ Check 1: CupSpawner script running?
│  └─ Inspector → CupSpawner → enabled checkbox
├─ Check 2: Cup Prefab assigned?
│  └─ Inspector → cupPrefab field → null?
├─ Check 3: Script Errors?
│  └─ Console → red errors?
└─ Solution: 
   ├─ Add debug: Debug.Log("CupSpawner.Start");
   └─ Check console output

Problem: Cup doesn't follow hand
├─ Check 1: HandAnchor assigned?
│  └─ Inspector → PlayerController → Hand Anchor field
├─ Check 2: handAnchor is null in Cup?
│  └─ Add: Debug.Log("handAnchor = " + handAnchor);
├─ Check 3: Cup.Update() running?
│  └─ Verify Cup enabled, not destroyed
└─ Solution:
   ├─ Re-assign HandAnchor in inspector
   └─ Verify Cup Update() has follow code

Problem: Serve doesn't validate correctly
├─ Check 1: Cup contents wrong?
│  └─ Log: Debug.Log("Cup: " + cup.Contents);
├─ Check 2: Customer order wrong?
│  └─ Log: Debug.Log("Order: " + customer.GetRequired...());
├─ Check 3: RecipeMatcher logic?
│  └─ Output: ["Espresso"] vs ["Milk"] = FALSE (correct)
└─ Solution:
   ├─ Print both lists to console
   └─ Check order of ingredients (should not matter)

Problem: Coins don't display
├─ Check 1: CoinsText assigned?
│  └─ Inspector → GameManager → Coins Text field
├─ Check 2: UpdateUI() called?
│  └─ Add: Debug.Log("UpdateUI, coins = " + coins);
├─ Check 3: AddCoins() working?
│  └─ Log: Debug.Log("AddCoins " + amount);
└─ Solution:
   ├─ Drag CoinsText UI into field
   └─ Verify UpdateUI() runs after AddCoins()

Problem: Upgrade cost wrong
├─ Check: GetBrewUpgradeCost formula
│  └─ 20 + BrewLevel * 10 = expected cost
└─ Debug: Log cost before SpendCoins()
```

---

**Now você have complete visual reference! Use this with UNITY_SETUP_GUIDE.md** 🎯
