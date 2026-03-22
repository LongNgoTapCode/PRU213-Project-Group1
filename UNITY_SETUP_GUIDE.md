# 🎮 Hướng Dẫn Setup & Sử Dụng Chi Tiết - Desolate Coffee

---

## **PHẦN 1: TẠO CÁC ASSET & PREFAB**

### **1.1 Tạo CupData ScriptableObject**

#### **Vì sao cần?**
- CupData lưu trữ cấu hình cup (dung tích, màu sắc, sprite)
- Cho phép tạo nhiều loại cup khác nhau mà không cần code
- Giống như DrinkData cho drink - quản lý data tách biệt khỏi code

#### **Các bước:**

1. **Tạo thư mục & Asset**
   ```
   Assets/
   └─ Prefabs/
      ├─ Cups/
      │  └─ (sẽ tạo ở đây)
   ```
   - Chuột phải `Assets/Prefabs` → New Folder → đặt tên `Cups`
   - **Vì sao?** Tổ chức prefab theo chủ đề giúp dễ tìm kiếm

2. **Tạo CupData asset**
   - Chuột phải trong thư mục → `Create` → `DesolateCoffee/Cup`
   - Đặt tên: `BasicCup`
   - **Vì sao "DesolateCoffee"?** Đó là menu prefix từ `[CreateAssetMenu]` trong script

3. **Cấu hình CupData trong Inspector**
   ```
   BasicCup Asset Settings:
   ├─ Cup Name: "Basic Cup"
   ├─ Cup Sprite: (tìm sprite cup hoặc để trống)
   ├─ Capacity: 3
   │  └─📝 Vì sao 3? = 3 ingredient tối đa trong cup
   │         Ví dụ: [Espresso, Milk, Sugar]
   ├─ Cup Color: White / Light Brown
   │  └─📝 Dùng để tô màu cup theo loại
   ```

---

### **1.2 Tạo Cup Prefab**

#### **Vì sao cần prefab?**
- Prefab = Template để instantiate multiple copies
- Thay vì tạo từng cup bằng tay, instantiate từ prefab
- Dễ thay đổi thuộc tính chung (sprite, size) cho tất cả cups

#### **Các bước:**

1. **Tạo prefab từ Object hiện tại**
   - Tạo empty GameObject tên `CupPrefab`
   - **Vì sao empty?** Sạch, không có component vô cần

2. **Thêm Components**
   ```
   CupPrefab
   ├─ Transform ✓ (sẵn)
   ├─ Cup.cs (NEW) ← Click Add Component
   ├─ SpriteRenderer (NEW)
   ├─ Rigidbody2D (NEW)
   │  └─ Settings:
   │     ├─ Body Type: Dynamic
   │     ├─ Gravity Scale: 0 (cup không rơi)
   │     └─ Constraints: Freeze Rotation Z
   ├─ BoxCollider2D (auto-add từ Cup.cs)
   │  └─ Is Trigger: unchecked (phải vật lý)
   └─ (Optional) Animator nếu cần animation cup
   ```

3. **Cấu hình Cup.cs component**
   ```
   Cup.cs:
   ├─ Cup Data: BasicCup (drag vào)
   │  └─📝 Vì sao? Để cup biết capacity & color
   ├─ Sprite Renderer: (auto-find hoặc manual)
   │  └─📝 Script sẽ render sprite từ CupData
   ```

4. **Cấu hình Sprite Renderer**
   ```
   SpriteRenderer Settings:
   ├─ Sprite: (tìm hoặc giữ trống)
   ├─ Color: White (Cup.cs sẽ đặt từ CupData)
   └─ Sorting Order: 2 (ở trên ingredient)
   ```

5. **Cấu hình Rigidbody2D**
   ```
   ├─ Mass: 0.5
   │  └─📝 Nhẹ để player kéo dễ
   ├─ Gravity Scale: 0
   │  └─📝 Không rơi khi trên mặt đất
   ├─ Collision Detection: Continuous
   └─ Constraints:
      └─ Freeze Rotation Z: ON
         └─📝 Cup không xoay khi đặt
   ```

6. **Tạo thành Prefab**
   - Drag GameObject từ Hierarchy vào `Assets/Prefabs/Cups/`
   - Hệ thống sẽ tự động tạo prefab
   - Xóa GameObject từ scene (giữ prefab)
   - **Vì sao xóa?** Prefab là template, instance tạo từ code

---

### **1.3 Tạo Cup Spawner (Counter)**

#### **Vì sao cần Cup Spawner?**
- Cup không vô hạn - phải respawn
- Người chơi có thể destroy/drop cup
- Spawner tự động tạo cup mới

#### **Các bước:**

1. **Tạo Spawner GameObject**
   ```
   CupCounter (GameObject)
   ├─ Position: (1, -2, 0)  ← Gần counter
   ├─ Scale: (1, 1, 1)
   └─ Components:
      └─ CupSpawner.cs (NEW)
   ```

2. **Cấu hình CupSpawner**
   ```
   CupSpawner.cs Settings:
   ├─ Cup Prefab: BasicCup prefab (drag vào)
   │  └─📝 Prefab này sẽ được instantiate
   ├─ Spawn Point: (drag CupCounter vào)
   │  └─📝 Vị trí spawn cup
   └─ Respawn Delay: 0.5s
      └─📝 Đợi 0.5s sau khi cup bị lấy mới respawn
         (không respawn quá nhanh)
   ```

3. **Thêm Trigger cho Spawner**
   - Thêm `BoxCollider2D` vào CupCounter
   - Settings:
     ```
     ├─ Is Trigger: ON
     │  └─📝 Trigger để detect cup pickup
     ├─ Size: (0.5, 0.5)
     └─ Offset: (0, 0)
     ```

4. **Test: Xem cup spawn lên**
   - Play → Cửa sổ Game sẽ thấy cup xuất hiện
   - Quay lại Scene, chuột phải vào CupCounter → Raycast hits để verify collider

---

## **PHẦN 2: CẤU HÌNH GAME MANAGER**

### **2.1 Setup Coins UI**

#### **Vì sao cần?**
- Player cần thấy coins hiện tại
- Coins là currency chính (thay score cũ)
- Mỗi action (serve cup, buy upgrade) sẽ thay đổi coins

#### **Các bước:**

1. **Tìm GameManager trong scene**
   - Hoặc tạo mới: `GameObject → Create Empty → GameManager`
   - **Vì sao?** Game lifecycle quản lý globally

2. **Thêm UI Text cho Coins**
   - Tạo Canvas: `GameObject → UI → Canvas`
   - Tạo TextMeshProUGUI:
     ```
     Canvas
     ├─ ScoreText (TextMeshProUGUI)
     │  └─ Text: "Score: 0" (old)
     ├─ CoinsText (TextMeshProUGUI) ← NEW
     │  └─ Text: "Coins: 0"
     │     Position: (0, 80, 0)
     │     Font Size: 24
     └─ ReputationText (TextMeshProUGUI)
     ```

3. **Liên kết CoinsText vào GameManager**
   - Select GameManager trong Hierarchy
   - Inspector → GameManager script
   - Drag CoinsText từ Hierarchy vào field `Coins Text`
   ```
   GameManager.cs Inspector:
   ├─ Score Text: ScoreText
   ├─ Coins Text: CoinsText ← NEW (drag vào)
   ├─ Reputation Text: ReputationText
   └─ (các field khác)
   ```

---

### **2.2 Khởi tạo Coins**

#### **Vì sao cần code này?**
```csharp
public int coins = 0;  // ← Coins bắt đầu từ 0
```
- Mỗi game mới, player bắt đầu 0 coins
- Coins tích lũy từ các đơn hàng hoàn thành

#### **Cách hoạt động:**

```
Frame 1 (Start):
├─ coins = 0
├─ UpdateUI() → "Coins: 0"

Frame 2 (Player serve cup đúng):
├─ Customer.Satisfy() 
├─ CupServeValidator.TryServeCupToCustomer()
└─ GameManager.AddCoins(15)
   ├─ coins += 15 → coins = 15
   └─ UpdateUI() → "Coins: 15"

Frame 3 (Player buy upgrade):
├─ UpgradeManager.BuyBrewUpgrade()
├─ GameManager.SpendCoins(20)
│  └─ coins -= 20 → coins = -5 ❌ FAILED
│     (không có coins, nên không subtract)
└─ UpdateUI() → "Coins: 15" (unchanged)
```

---

## **PHẦN 3: CẤU HÌNH PLAYER CONTROLLER**

### **3.1 Cup Pickup System**

#### **Vì sao cần Cup Pickup?**
- Player cần lấy cup từ counter
- Giống như pickup item, nhưng cup là container
- Cup sẽ theo tay player khi held

#### **Cách hoạt động:**

```
E Key Press (Tap):
├─ HandleEInteraction() từ PlayerController
│  └─ eHoldTimer = 0 (start count)
│
E Key Release < 0.2 seconds:
├─ useTapInteraction = true
├─ HandlePickupDrop()
├─ FindNearestCup() ← FindNearestCup() là hàm mới
│  └─ Tìm cup trong radius
│  └─ Return cup gần nhất
├─ TryPickupCup(cup)
│  ├─ cup.AttachToHand(handAnchor)
│  │  └─ Cup vị trí = hand vị trí
│  ├─ heldCup = cup ← Assign to player
│  └─ cup.isHeld = true
└─ Update() mỗi frame:
   ├─ if (isHeld && handAnchor):
   │  └─ transform.position = handAnchor.position
   │     ← Cup luôn ở vị trí tay
```

#### **Cấu hình trong Unity:**

1. **Hand Anchor Setup**
   ```
   Player Character
   ├─ Body (Sprite)
   └─ HandAnchor (Empty GameObject)
      └─ Position: (0.3, 0, 0) ← Offset tay phải
      ```

2. **PlayerController Settings**
   ```
   PlayerController.cs Inspector:
   ├─ Hand Anchor: (drag HandAnchor vào)
   │  └─📝 Nơi cup/item sẽ attach vào
   ├─ Interaction Radius: 1.25
   │  └─📝 Khoảng cách quét để tìm cup
   ├─ Use Physics Movement: true
   └─ (các field khác)
   ```

---

### **3.2 Pouring System - Đổ Nguyên Liệu vào Cup**

#### **Vì sao cần pouring?**
- Cup trống lúc đầu
- Player phải đổ ingredient từ holdings vào cup
- Mỗi cup có capacity (dung tích)

#### **Cách hoạt động:**

```
Game Flow:
1. Player pickup ingredient từ workstation
   ├─ currentHoldings = ["Espresso"]
   └─ (giữ ingredient trong tay)

2. Player pickup cup
   ├─ heldCup = cup (có sẵn 3 capacity)
   └─ cup.Contents = [] (rỗng)

3. Right-click (RMB) trên cup:
   ├─ TryPourAllIntoCup()
   ├─ FOR each ingredient in currentHoldings:
   │  ├─ cup.TryAddIngredient(ing) ← Thêm vào cup
   │  │  └─ if (IsFull) return false → stop
   │  └─ currentHoldings.Remove(ing) ← Xóa từ holdings
   └─ Kết quả: cup.Contents = ["Espresso"]

4. Player click customer:
   ├─ CupServeValidator.TryServeCupToCustomer(cup, customer)
   │  ├─ cup.Contents = ["Espresso"]
   │  ├─ customer.GetRequiredIngredients() = ["Espresso", "Milk"]
   │  └─ RecipeMatcher.IsExactMatch() → FALSE
   │     (thiếu Milk)
   └─ Serve FAIL → -1 reputation
```

#### **Code Chi Tiết:**

```csharp
// Trong PlayerController.cs

public bool TryPourAllIntoCup() {
    if (heldCup == null || currentHoldings.Count == 0) {
        return false;  // ← Không có cup hoặc holdings rỗng
    }

    int poured = 0;
    for (int i = currentHoldings.Count - 1; i >= 0; i--) {
        if (heldCup.TryAddIngredient(currentHoldings[i])) {
            // ← TryAddIngredient() kiểm tra capacity
            // ← Nếu full, return false và dừng vòng lặp
            currentHoldings.RemoveAt(i);
            poured++;
        } else {
            break;  // Cup full, stop pouring
        }
    }
    return poured > 0;
}
```

#### **Cấu hình UI (Right-click để pour):**

Tạo Input Action (nếu chưa có):
```
File → Input System Actions (hoặc trong code):

nput.rightClick:
├─ Binding: Right Mouse Button
│  └─📝 Right-click để đổ all ingredients
└─ Action Type: Button Press
```

---

## **PHẦN 4: CẤU HÌNH CUSTOMER & SERVING**

### **4.1 Customer Setup**

#### **Vì sao cần cấu hình này?**
- Mỗi customer có 1 order (yêu cầu drink cụ thể)
- Order = list ingredients + patience timer
- Validation sẽ match cup contents với order

#### **Cấu hình trong Unity:**

1. **Customer Prefab Inspector:**
   ```
   Customer.cs Settings:
   ├─ Order: (drag DrinkData asset vào)
   │  └─ Ví dụ: "Espresso" DrinkData
   │     ├─ Drink Name: "Espresso"
   │     ├─ Ingredients: ["Espresso"]
   │     └─ Base Price: 10 coins
   │        └─📝 Reward player nhận nếu phục vụ đúng
   │
   ├─ Max Patience: 20
   │  └─📝 Seconds trước khi customer bực tức
   │     (UpgradeManager sẽ nhân với multiplier)
   │
   ├─ Recipe Book: (nếu muốn data-driven)
   │  └─📝 Alternative để DrinkData
   └─ (các field khác)
   ```

2. **Create DrinkData cho mỗi drink:**
   ```
   Assets/
   └─ Espresso.asset
      ├─ Drink Name: "Espresso"
      ├─ Ingredients: ["Espresso"]
      ├─ Base Price: 10
      └─ Icon: (sprite nếu có)

   Latte.asset
   ├─ Drink Name: "Latte"
   ├─ Ingredients: ["Espresso", "Milk"]
   ├─ Base Price: 15
   └─ Icon: (sprite)

   Cappuccino.asset
   ├─ Drink Name: "Cappuccino"
   ├─ Ingredients: ["Espresso", "Milk", "Foam"]
   ├─ Base Price: 18
   └─ Icon: (sprite)
   ```

---

### **4.2 Serving & Validation Flow**

#### **Vì sao cần CupServeValidator?**
- Centralized validation logic
- Tránh lỗi (validate 1 lần, không 2 lần)
- Tính toán reward với bonus
- Quản lý reputation & coin atomically

#### **Cách hoạt động chi tiết:**

```csharp
// Trong CupServeValidator.cs

public bool TryServeCupToCustomer(Cup cup, Customer customer) {
    // Step 1: Lấy dữ liệu
    var requiredIngredients = customer.GetRequiredIngredients();
    // ← Ví dụ: ["Espresso", "Milk"]
    
    var cupContents = cup.Contents;
    // ← Ví dụ: ["Milk", "Espresso"]
    
    // Step 2: Validate (unordered match)
    bool isCorrect = RecipeMatcher.IsExactMatch(cupContents, requiredIngredients);
    // ← IsExactMatch([Milk, Espresso], [Espresso, Milk]) = TRUE
    //   Vì same items, không quan tâm thứ tự
    
    if (isCorrect) {
        // ✅ CORRECT PATH
        
        // 2a. Tính reward
        int reward = GetReward(customer);
        // ← Base 10 + speed bonus 5 = 15 coins
        
        // 2b. Award coins
        GameManager.Instance?.AddCoins(reward);
        
        // 2c. Mark order complete
        OrderManager.Instance?.CompleteOrder(customer);
        
        // 2d. Play happy sound
        AudioFeedbackManager.Instance?.PlayOrderCompleteChime();
        
        // 2e. Customer leaves happy
        customer.Satisfy();
        // ← Gọi StartLeaving() → FinalizeLeaving() → Destroy()
        
        return true;
    } else {
        // ❌ WRONG PATH
        
        // 3a. Lose reputation
        GameManager.Instance?.LoseReputation();
        // ← reputation-- → Check if <= 0 → GameOver()
        
        // 3b. Mark order failed
        OrderManager.Instance?.FailOrder(customer);
        
        // 3c. Customer leaves angry
        customer.Leave();
        // ← Gọi LeaveAngry() → StartLeaving()
        
        return false;
    }
}

private int GetReward(Customer customer) {
    int reward = 10;  // Base reward
    
    // Bonus nếu customer còn kiên nhẫn
    if (customer.RemainingTime > customer.MaxPatience * 0.5f) {
        reward += 5;  // Speed bonus
    }
    
    return reward;  // Ví dụ: 10 hoặc 15
}
```

---

## **PHẦN 5: CLICK TO SERVE MECHANIC**

### **Vì sao cần click?**
- Serve cup là action chính
- Left-click customer để gửi cup
- System tự validate & reward

### **Cấu hình trong PlayerController:**

```csharp
// Trong Update() method

if (Mouse.current.leftButton.wasPressedThisFrame) {
    // Step 1: Raycast từ mouse position
    Vector2 mousePos = Mouse.current.position.ReadValue();
    Vector2 worldPos = cachedMainCamera.ScreenToWorldPoint(mousePos);
    
    // Step 2: Tìm collider tại position
    int clickHitCount = Physics2D.OverlapPoint(worldPos, ..., clickHitsBuffer);
    
    // Step 3: Loop qua tất cả hits
    for (int i = 0; i < clickHitCount; i++) {
        Collider2D col = clickHitsBuffer[i];
        
        // Step 4a: Tìm Customer component
        Customer customer = col.GetComponent<Customer>();
        if (customer != null) {
            // ✅ Found Customer!
            
            // Step 5a: Try serve cup (nếu holding cup)
            if (heldCup != null) {
                bool success = CupServeValidator.Instance
                    ?.TryServeCupToCustomer(heldCup, customer) ?? false;
                
                if (success) {
                    // Destroy cup sau khi serve
                    Destroy(heldCup.gameObject);
                    heldCup = null;
                }
                return;  // Stop checking
            }
            
            // Step 5b: Fallback - serve từ holdings (cách cũ)
            bool success = customer.ReceiveDrink(currentHoldings);
            if (success) currentHoldings.Clear();
            return;
        }
    }
}
```

---

## **PHẦN 6: UPGRADE SYSTEM - COINS TO UPGRADES**

### **6.1 Vì sao coins không phải score?**

```
❌ Cách cũ (Score-based):
├─ Score = tất cả điểm từ mọi source
├─ Hard to separate: serve cup, bonus, cheat code
├─ Confusing: score hay prize?
└─ Không phù hợp với economy game

✅ Cách mới (Coin-based):
├─ Coins = official currency
├─ Clear: coins từ serve cup chính xác
├─ Easy to balance: adjust coin per order
└─ Professional: giống real game economy
```

### **6.2 Cấu hình Upgrade**

#### **Trong Unity:**
```
GameManager Scene:
├─ Upgrade Buttons (Canvas)
│  ├─ BuyBrewButton
│  │  └─ Button.onClick → GameManager.BuyUpgradeBrew()
│  ├─ BuyPatienceButton
│  │  └─ Button.onClick → GameManager.BuyUpgradePatience()
│  └─ BuyStabilityButton
│     └─ Button.onClick → GameManager.BuyUpgradeStability()
│
└─ UpgradeInfoText
   └─ Shows current levels & costs
```

#### **Cấu hình UpgradeManager:**
```
UpgradeManager.cs Inspector:
├─ Upgrade Info Text: (drag UpgradeInfoText vào)
│  └─ Text sẽ update: "Brew Lv.1 (Cost 30)"
└─ (Settings tự động load từ PlayerPrefs)
```

### **6.3 Coin Cost Formula:**

```csharp
// costs scale up with level

public int GetBrewUpgradeCost() {
    return 20 + BrewLevel * 10;
}
// Lv.0: 20 coins
// Lv.1: 30 coins
// Lv.2: 40 coins
// ...

public int GetPatienceUpgradeCost() {
    return 20 + PatienceLevel * 10;
}

public int GetStabilityUpgradeCost() {
    return 25 + StabilityLevel * 12;
}
// Stability đắt hơn vì effect lớn
```

#### **Vì sao cost tăng?**
- Prevent snowballing (lv1 quá rẻ)
- Encourage progression (phải chơi nhiều để lv cao)
- Economic balance (không spam upgrade)

---

## **PHẦN 7: RUN SUM & PROGRESSION**

### **Vì sao track coins earned?**

```
Game Over Screen:
├─ Run Result
├─ Coins Earned: 45
│  └─ 3 correct orders × 15 coins = 45
├─ Completed: 3
├─ Failed: 2
└─ Accuracy: 60%

Progression:
├─ Run 1 → Earn 45 coins → Buy Brew Lv.1 (cost 20)
├─ Run 2 → Earn 30 coins (harder) → Buy Patience Lv.1 (cost 20)
└─ Run 3 → Earn 60 coins (các upgrade giúp) → Buy Stability Lv.1 (cost 25)
```

#### **Setup Run Summary:**

```
GameManager.cs:
├─ BuildRunSummary()
│  └─ Lấy:
│     ├─ coins (từ GameManager)
│     ├─ CompletedOrders (từ OrderManager)
│     ├─ FailedOrders
│     └─ runTimer (thời gian chơi)
│  └─ Display:
│     "Run Result\n"
│     "Coins: 45\n"
│     "Completed: 3\n"
│     "Failed: 2\n"
│     "Accuracy: 60%\n"
│     "Time: 120s"
```

---

## **PHẦN 8: COMPLETE TEST FLOW**

### **Step-by-Step Test Scenario:**

#### **Setup Scene:**
```
Scene Layout:
├─ Player (PlayerController)
├─ HeatDirector (spawn customers)
├─ CupCounter (CupSpawner)
├─ Workstations (provide ingredients)
├─ GameManager (track coins)
└─ Canvas/UI (show coins & reputation)
```

#### **Test Play:**

```
Time: 0s
├─ Game Start
├─ coins = 0
├─ reputation = 5
└─ Coins Text: "Coins: 0"

Time: 5s
├─ HeatDirector spawn Customer
├─ Customer order: "Espresso"
│  └─ required: ["Espresso"]
│  └─ reward: 10 coins
├─ OrderManager.RegisterOrder()
├─ OrderDashboardUI show: "1. Espresso [20s]"

Time: 10s
├─ Player pickup ingredient from grinder
├─ currentHoldings = ["Espresso"]
├─ Holdings UI: "Holding: Espresso"

Time: 15s
├─ Player press E (tap)
├─ PlayerController.HandlePickupDrop()
├─ FindNearestCup() → CupPrefab instantiated by CupSpawner
├─ TryPickupCup(cup)
├─ heldCup = cup
├─ Cup appear in hand

Time: 17s
├─ Player right-click (RMB)
├─ TryPourAllIntoCup()
├─ cup.TryAddIngredient("Espresso") → true
├─ currentHoldings.Remove("Espresso")
├─ cup.Contents = ["Espresso"]
├─ Validation: cup has ["Espresso"], customer needs ["Espresso"]

Time: 18s
├─ Player left-click customer
├─ CupServeValidator.TryServeCupToCustomer(cup, customer)
├─ RecipeMatcher.IsExactMatch(["Espresso"], ["Espresso"]) = TRUE ✓
├─ int reward = GetReward(customer) = 15 (base 10 + speed 5)
├─ GameManager.AddCoins(15)
│  └─ coins = 15 ✓
├─ Coins Text: "Coins: 15"
├─ OrderManager.CompleteOrder()
├─ customer.Satisfy()
├─ customer.StartLeaving() → Destroy()
└─ AudioFeedbackManager.PlayOrderCompleteChime() ♫

Time: 25s
├─ HeatDirector spawn Customer 2
├─ Customer 2 order: "Latte"
│  └─ required: ["Espresso", "Milk"]
│  └─ reward: 15 coins (2 ingredients)

Time: 30s
├─ Player gather ingredients
├─ currentHoldings = ["Espresso", "Milk"]
├─ E → pickup cup (new one respawned)
├─ RMB → pour both into cup
├─ cup.Contents = ["Espresso", "Milk"]

Time: 32s
├─ Left-click Customer 2
├─ Validate ["Espresso", "Milk"] vs ["Espresso", "Milk"] = TRUE ✓
├─ coins += 15 → coins = 30
├─ Customer 2 leaves happy

Time: 35s
├─ HeatDirector spawn Customer 3
├─ Customer 3 order: "Cappuccino"
│  └─ required: ["Espresso", "Milk", "Foam"]

Time: 40s
├─ Player gather only ["Espresso", "Milk"]
├─ Forget "Foam"!
├─ Pour into cup
├─ cup.Contents = ["Espresso", "Milk"] (missing Foam)

Time: 42s
├─ Left-click Customer 3
├─ Validate ["Espresso", "Milk"] vs ["Espresso", "Milk", "Foam"] = FALSE ❌
├─ GameManager.LoseReputation()
│  └─ reputation = 4
├─ Coins Text stay: "Coins: 30" (no change)
├─ Reputation Text: "Rep: 4 / 5"
├─ Customer 3 LeaveAngry() → Destroy()

Time: 50s
├─ Player gathered enough coins (30)
├─ Click "Buy Brew Upgrade" button
├─ UpgradeManager.BuyBrewUpgrade()
├─ int cost = GetBrewUpgradeCost() = 20
├─ GameManager.SpendCoins(20)
│  └─ coins = 10 ✓
├─ BrewLevel = 1
├─ Coins Text: "Coins: 10"
├─ Workstations cook faster now!
└─ UpgradeInfoText: "Brew Lv.1 (Cost 30)"

Time: 60s
├─ Game continue...
└─ (and so on)

Time: 120s
├─ HeatDirector.timer reach timeout
├─ OR reputation drop to 0
├─ Game Over!
├─ BuildRunSummary() show:
│  ├─ Coins: 10 (tổng coins cuối cùng)
│  ├─ Completed: 2
│  ├─ Failed: 1
│  ├─ Accuracy: 66%
│  └─ Time: 120s
└─ Player can restart
```

---

## **PHẦN 9: DEBUGGING CHECKLIST**

### **Nếu Cup không spawn:**
```
❌ Problem: CupSpawner.Update() không gọi SpawnCup()
✓ Solution:
  1. Check CupSpawner.cupPrefab có assign không
     → Inspector: CupSpawner → Cup Prefab field
  2. Check CupSpawner attached vào GameObject?
  3. Check script enabled (checkbox ✓)
  4. Add debug log:
     
     void Start() {
         Debug.Log("CupSpawner started, cupPrefab = " + cupPrefab);
     }
     
     → Console sẽ show "cupPrefab = null" nếu lỗi
```

### **Nếu Cup không move với tay:**
```
❌ Problem: Cup.Update() không update position
✓ Solution:
  1. Check isHeld = true?
     → Add Log: Debug.Log("Cup.isHeld = " + isHeld);
  2. Check handAnchor assigned?
     → Inspector: PlayerController → Hand Anchor field
  3. Check Cup.Update() running?
     → Verify Cup script enabled
     → Verify Cup GameObject active (not grayed out)
```

### **Nếu Serve không validate:**
```
❌ Problem: TryServeCupToCustomer() return false khi should true
✓ Solution:
  1. Add detailed log:
     
     // Trong CupServeValidator.cs
     Debug.Log($"Cup contents: {string.Join(", ", cup.Contents)}");
     Debug.Log($"Required: {string.Join(", ", requiredIngredients)}");
     Debug.Log($"Match result: {isCorrect}");
     
     → Console will show exact mismatch
  2. Check Cup contents correct order?
     → RecipeMatcher.IsExactMatch() không care thứ tự
     → ["Milk", "Espresso"] = ["Espresso", "Milk"] ✓
  3. Check Customer order assigned?
     → Hierarchy → Customer prefab → Inspector
     → Customer.order field có assign DrinkData?
```

### **Nếu Coins không tăng:**
```
❌ Problem: AddCoins() run nhưng Coins Text không update
✓ Solution:
  1. Check GameManager.Instance != null?
     → Verify GameManager singleton setup:
        void Awake() { Instance = this; }
  2. Check Coins Text assigned?
     → Inspector: GameManager → Coins Text field
  3. Check UpdateUI() running?
     → Add log: Debug.Log("Coins: " + coins);
  4. Check AddCoins() called?
     → Add log tại:
        public void AddCoins(int amount) {
            Debug.Log("AddCoins called with " + amount);
            coins += ...;
        }
```

### **Nếu Upgrade cost sai:**
```
❌ Problem: Buy upgrade show "Not enough coins" when should work
✓ Solution:
  1. Check cost calculation:
     int cost = UpgradeManager.GetBrewUpgradeCost();
     Debug.Log($"Brew cost: {cost}, Current coins: {GameManager.Instance.coins}");
  2. Check SpendCoins logic:
     bool success = GameManager.Instance.SpendCoins(cost);
     Debug.Log($"Spend coins success: {success}");
```

---

## **PHẦN 10: BEST PRACTICES**

### **1. Always assign vào Inspector nếu là public**
```csharp
// ❌ Bad: Hardcoded
Cup cupPrefab = Resources.Load<Cup>("Cups/BasicCup");

// ✅ Good: Assign via inspector
[SerializeField] private Cup cupPrefab;
```
**Vì sao?** Inspector flexible, dễ test, dễ swap assets

### **2. Use components unity cung cấp**
```csharp
// ❌ Bad: Manual position update
transform.position = new Vector3(x, y, z);

// ✅ Good: Rigidbody physics
rigidbody.velocity = moveDirection * speed;
```
**Vì sao?** Physics engine tối ưu, collision xử lý tự động

### **3. Centralize validation logic**
```csharp
// ❌ Bad: Validate ở 2 chỗ
// PlayerController.cs
bool correct = RecipeMatcher.IsExactMatch(...);
// Customer.cs
bool correct = RecipeMatcher.IsExactMatch(...);

// ✅ Good: Validate ở 1 chỗ
CupServeValidator.TryServeCupToCustomer(cup, customer);
```
**Vì sao?** Tránh duplicate logic, dễ maintain, bug fix once

### **4. Use ScriptableObject cho data**
```csharp
// ❌ Bad: Hardcode trong script
if (drinkName == "Espresso") { ... }

// ✅ Good: Data asset
[SerializeField] private DrinkData drinkData;
string name = drinkData.drinkName;
```
**Vì sao?** Reusable, designer friendly, easy balance

### **5. Prefabs > Manual instantiation**
```csharp
// ❌ Bad: Tạo object từ code
GameObject cup = new GameObject("Cup");
cup.AddComponent<SpriteRenderer>();

// ✅ Good: Instantiate từ prefab
Cup cup = Instantiate(cupPrefab, position, Quaternion.identity);
```
**Vì sao?** Consistent setup, ít error, dễ iterate

---

## **SUMMARY - GAME LOOP HOÀN CHỈNH:**

```
┌─────────────────────────────────────────────┐
│         DESOLATE COFFEE - GAME LOOP         │
└─────────────────────────────────────────────┘

    Player Start Game
         │
         ├─ GameManager init: coins = 0, reputation = 5
         ├─ HeatDirector spawn customers randomly
         └─ UI show: "Coins: 0" "Rep: 5/5"
         │
    ┌────┴────────────────────────────────────┐
    │     MAIN GAME LOOP (each customer)       │
    │                                          │
    │  1. 👥 Customer arrive with order       │
    │     (e.g., "Latte" = need "Espresso" + "Milk")
    │                                          │
    │  2. ⏱️ Patience countdown               │
    │     (20s → 0s, color green→red)        │
    │                                          │
    │  3. 🧑 Player action loop:              │
    │     a) Get ingredients from workstation │
    │     b) Pick up cup with E key           │
    │     c) Pour ingredients with RMB        │
    │     d) Left-click customer to serve     │
    │                                          │
    │  4. ✓ Validate cup contents             │
    │     ✅ CORRECT:                         │
    │        - Award coins +15                │
    │        - Mark order complete            │
    │        - Customer leave happy           │
    │     ❌ WRONG:                           │
    │        - Lose reputation -1             │
    │        - Mark order failed              │
    │        - Customer leave angry           │
    │                                          │
    │  5. 💰 Spend coins on upgrades         │
    │     (optional, between orders)         │
    │     - Brew: faster cook                │
    │     - Patience: customers wait longer  │
    │     - Stability: less incidents        │
    │                                          │
    └─────────────────────────────────────────┘
         │
         ├─ Loop qua customers
         │
    Game Over Condition:
         ├─ reputation reach 0 → GAME OVER ❌
         └─ OR all customers served successfully
         │
    🏁 Show Run Summary
       ├─ Total coins earned
       ├─ Completed orders
       ├─ Failure count
       └─ Play time
       │
    🔄 Restart option
       └─ Keep upgrades (saved in PlayerPrefs)
          Next run: upgrades active!
```

---

**Hết phần hướng dẫn. Mở Unity, follow step-by-step, sẽ thành công! 🚀**
