# 📚 Documentation Index - Desolate Coffee

---

## **Hướng Dẫn Sử Dụng Tài Liệu**

Có **4 tài liệu** để giúp bạn setup & hiểu code:

### **1. 📖 [UNITY_SETUP_GUIDE.md](UNITY_SETUP_GUIDE.md) - BẮT ĐẦU TỪ ĐÂY!**

**Sử dụng khi:** Bạn muốn setup game từ đầu trong Unity

**Nội dung 10 phần:**
- **Phần 1**: Tạo CupData asset & Cup Prefab
- **Phần 2**: Setup GameManager + Coins UI
- **Phần 3**: Cấu hình PlayerController (pickup cup)
- **Phần 4**: Customer & Order setup
- **Phần 5**: Click-to-serve mechanic
- **Phần 6**: Upgrade system (coins to upgrades)
- **Phần 7**: Run summary & progression
- **Phần 8**: Complete test flow (thời gian cụ thể)
- **Phần 9**: Debugging checklist
- **Phần 10**: Best practices

**Cách đọc:**
```
1. Mở Unity
2. Follow step-by-step từ Phần 1
3. Sau mỗi bước, test trong Play mode
4. Nếu lỗi, jump to Phần 9 (Debugging)
```

---

### **2. 🎨 [VISUAL_DIAGRAMS.md](VISUAL_DIAGRAMS.md) - THAM KHẢO NHANH**

**Sử dụng khi:** Bạn muốn có cái nhìn tổng quát hoặc cần reference nhanh

**Nội dung:**
- **Scene Hierarchy**: Toàn bộ GameObject & component cần có
- **Component Dependency**: Mối quan hệ giữa scripts
- **Cup Lifecycle**: Từ spawn → pickup → serve → destroy
- **Serve Validation Flowchart**: Decision tree khi serve cup
- **Setup Checklist**: ✓ boxes để check từng bước
- **Troubleshooting Flowchart**: Quick fixes cho common issues

**Cách đọc:**
```
┌─ Khi stuck?
├─ Check Scene Hierarchy diagram
├─ Verify component relationships
└─ Follow troubleshooting flowchart

┌─ Không hiểu flow?
├─ Xem Cup Lifecycle diagram
└─ Xem Serve Validation flowchart
```

---

### **3. 💻 [CODE_EXAMPLES.md](CODE_EXAMPLES.md) - HIỂU CODE CHI TIẾT**

**Sử dụng khi:** Bạn muốn hiểu tại sao code lại viết như vậy

**Nội dung:**
- **Example 1**: CupData + Cup.cs (from data to instance)
- **Example 2**: Pouring system (ingredients → cup)
- **Example 3**: Serving & validation
- **Example 4**: Coin system (earning & spending)
- **Example 5**: Upgrade flow (coin → level)
- **Example 6**: Complete scenario (time progression 0s → 45s)

**Mỗi example có:**
```
Code → Giải Thích Chi Tiết → Vì sao?
```

**Cách đọc:**
```
1. Đọc code
2. Hiểu từng method bằng comments
3. Xem "Vì sao?" section
4. Trace complete flow qua Example 6
```

---

### **4. 📄 Implementation Files**

Các file này đã được implemented sẵn:

```
Assets/Scripts/
├─ Cup.cs ← Read this + CODE_EXAMPLES.md
├─ CupData.cs
├─ CupSpawner.cs
├─ CupServeValidator.cs
├─ GameManager.cs (modified)
├─ UpgradeManager.cs (modified)
├─ PlayerController.cs (modified)
└─ Customer.cs (modified)
```

---

## **HOW TO USE GUIDES - QUICK FLOWCHART**

```
┌─ START: "Tôi muốn setup game"
│
├─ First time?
│  └─ Read: UNITY_SETUP_GUIDE.md (Phần 1-10)
│
├─ Stuck on setup?
│  ├─ Check: VISUAL_DIAGRAMS.md (Scene Hierarchy)
│  └─ Then: VISUAL_DIAGRAMS.md (Setup Checklist)
│
├─ "Tại sao code lại như vậy?"
│  └─ Read: CODE_EXAMPLES.md (relevant example)
│
├─ "Cái này không work!"
│  ├─ Check: CODE_EXAMPLES.md (Example 1-3)
│  └─ Follow: VISUAL_DIAGRAMS.md (Troubleshooting)
│
└─ "Muốn hiểu complete flow"
   └─ Read: CODE_EXAMPLES.md (Example 6)
```

---

## **KEY TERMS & CONCEPTS**

### **Cup System**
```
CupData
  └─ Asset chứa: name, sprite, capacity, color
     (Giống DrinkData, nhưng cho cup)

Cup
  └─ MonoBehaviour chứa:
     - TryAddIngredient() (add with capacity check)
     - AttachToHand() (attach to player)
     - DropToWorld() (drop back to ground)
     - Contents[] (read-only list)
```

### **Serving System**
```
CupServeValidator
  └─ Centralized validation + reward
     ├─ TryServeCupToCustomer()
     ├─ Validate contents vs order
     ├─ Award coins (correct) or lose rep (wrong)
     └─ One place = easy to maintain

RecipeMatcher.IsExactMatch()
  └─ Check if ["Espresso", "Milk"] == ["Milk", "Espresso"]
     (Unordered match = order doesn't matter)
```

### **Coin Economy**
```
coins
  └─ Main currency (replaces score)
     ├─ Earned from: serving correct orders
     ├─ Spent on: upgrades (Brew, Patience, Stability)
     └─ Persisted in: PlayerPrefs (survive restarts)

Upgrade Costs
  └─ Cost increases per level
     └─ Brew Lv0→1: 20, Lv1→2: 30, Lv2→3: 40, ...
        (Prevent snowballing, encourage progression)
```

---

## **SETUP FLOW - TL;DR**

```
Step 1: Create Assets (5 min)
├─ CupData/BasicCup.asset
└─ Prefabs/Cups/CupPrefab.prefab

Step 2: Scene Objects (10 min)
├─ GameManager (coins UI)
├─ CupCounter (CupSpawner)
└─ Other systems (already exist)

Step 3: Assign References (5 min)
├─ CupSpawner.cupPrefab = CupPrefab
├─ GameManager.coinsText = CoinsText UI
└─ PlayerController.handAnchor = HandAnchor

Step 4: Test (5 min)
├─ Play → Cup spawn
├─ E → Pickup cup
├─ RMB → Pour ingredients
├─ Click customer → Serve
└─ Check coins +15 on correct order

Total: ~25 minutes for complete setup
```

---

## **READING ORDER RECOMMENDATIONS**

### **Option A: "New to all this"**
```
1. VISUAL_DIAGRAMS.md (Scene Hierarchy)
   └─ Understand structure
2. UNITY_SETUP_GUIDE.md (Phần 1-4)
   └─ Create assets & basic setup
3. CODE_EXAMPLES.md (Example 1-2)
   └─ Understand code
4. Back to UNITY_SETUP_GUIDE.md (Phần 5-8)
   └─ Complete setup
5. Test & use VISUAL_DIAGRAMS.md Troubleshooting
   └─ Fix issues
```

### **Option B: "I just want to setup quickly"**
```
1. VISUAL_DIAGRAMS.md (Setup Checklist)
   └─ Follow checkbox by checkbox
2. Test in Play mode
3. If issue → VISUAL_DIAGRAMS.md (Troubleshooting)
```

### **Option C: "I want to understand everything"**
```
1. CODE_EXAMPLES.md (Example 1-3)
   └─ Understand cup & serving
2. CODE_EXAMPLES.md (Example 4-5)
   └─ Understand coins & upgrades
3. CODE_EXAMPLES.md (Example 6)
   └─ See complete flow
4. UNITY_SETUP_GUIDE.md (Phần yang relevant)
   └─ Implement what you understand
5. VISUAL_DIAGRAMS.md (all)
   └─ Reference & verify
```

---

## **COMMON QUESTIONS & WHERE TO FIND ANSWERS**

| Question | Answer Location |
|----------|------------------|
| "How do I create cup?" | UNITY_SETUP_GUIDE.md 1.2-1.3 |
| "Why is CupData separate?" | CODE_EXAMPLES.md Example 1.1 |
| "How does pouring work?" | CODE_EXAMPLES.md Example 2.1 |
| "Why validation centralized?" | CODE_EXAMPLES.md Example 3.1 |
| "How coin system work?" | CODE_EXAMPLES.md Example 4-5 |
| "What is this dependency?" | VISUAL_DIAGRAMS.md Component Dependency |
| "Scene setup is confusing" | VISUAL_DIAGRAMS.md Scene Hierarchy |
| "Cup keeps disappearing!" | VISUAL_DIAGRAMS.md Troubleshooting |
| "Complete flow walkthrough" | CODE_EXAMPLES.md Example 6 |
| "Best practices?" | UNITY_SETUP_GUIDE.md Phần 10 |

---

## **DEBUGGING WORKFLOW**

Nếu gặp lỗi:

```
1. Check Console (Ctrl+Shift+C)
   └─ Gì errors?

2. Read error message carefully
   └─ NullReferenceException? → field not assigned
   └─ Missing component? → check inspector

3. Use appropriate guide:
   ├─ If "Cup not spawn"
   │  └─ VISUAL_DIAGRAMS.md Troubleshooting
   │  └─ UNITY_SETUP_GUIDE.md 1.3 (CupSpawner)
   │
   ├─ If "Validation wrong"
   │  └─ CODE_EXAMPLES.md Example 3.1
   │  └─ UNITY_SETUP_GUIDE.md Phần 4 (Customer setup)
   │
   └─ If "Coins not showing"
      └─ UNITY_SETUP_GUIDE.md 2.1 (Coins UI)
      └─ CODE_EXAMPLES.md Example 4.1

4. Add debug logs
   └─ CODE_EXAMPLES.md show examples with logs

5. Re-read relevant section
   └─ Usually the answer là ở đó
```

---

## **SUMMARY**

```
📖 UNITY_SETUP_GUIDE.md
   ↓
   Step-by-step implementation
   (Use when setting up in editor)

🎨 VISUAL_DIAGRAMS.md
   ↓
   Visual reference & quick lookup
   (Use when need overview or debugging)

💻 CODE_EXAMPLES.md
   ↓
   Deep understanding of each part
   (Use when want to understand why)

📄 Implementation Files (Assets/Scripts/*)
   ↓
   Real code to study & reference
   (Use alongside CODE_EXAMPLES.md)
```

---

**Start with UNITY_SETUP_GUIDE.md Phần 1.1 - Good luck! 🚀**
