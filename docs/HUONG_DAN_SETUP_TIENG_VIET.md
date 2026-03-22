# Hướng Dẫn Setup Unity (Tiếng Việt, Chi Tiết)

Tài liệu này dành cho người mới, hướng dẫn từng bước để chạy được dự án game cà phê.

## 1. Tổng Quan Hệ Thống

Dự án gồm các nhóm chính:

1. Điều khiển người chơi: nhặt/thả vật, click tương tác.
2. Vật phẩm và quầy: đặt vật vào slot, tự snap vào vị trí.
3. Workstation: pha chế, sự cố, sửa máy.
4. Khách hàng và đơn hàng: spawn, chờ, nhận đồ, rời đi.
5. UI dashboard: hiển thị danh sách đơn và thời gian còn lại.
6. Công thức data-driven: đọc công thức từ JSON.
7. VFX/SFX: âm thanh máy, ting hoàn thành đơn, khói nóng.
8. Game loop: điểm, game over, summary, nâng cấp.

## 2. Giải Thích Thuật Ngữ Nhanh

1. Prefab: mẫu object để tạo nhiều lần.
2. Component: thành phần gắn vào GameObject (script, collider, renderer...).
3. Inspector: nơi chỉnh tham số ở Unity.
4. LayerMask: bộ lọc layer khi raycast/overlap.
5. Collider2D: vùng va chạm trong game 2D.
6. Rigidbody2D: vật lý 2D.
7. World Space UI: UI nằm trong thế giới game, không phải HUD cố định.
8. ScriptableObject: asset dữ liệu (ví dụ DrinkData).

## 3. Setup Theo Từng Bước

## Bước 1: Tạo manager trong scene

Trong Hierarchy, tạo các object sau và gắn script tương ứng:

1. GameManager -> GameManager.cs
2. OrderManager -> OrderManager.cs
3. UpgradeManager -> UpgradeManager.cs
4. AudioFeedbackManager -> AudioFeedbackManager.cs

Với AudioFeedbackManager, gán 3 audio clip:

1. defaultGrinderLoop
2. orderCompleteChime
3. incidentAlert

## Bước 2: Setup người chơi

1. Chọn object Player, gắn PlayerController.cs.
2. Tạo child Empty tên HandAnchor tại vị trí tay.
3. Kéo HandAnchor vào field handAnchor.
4. Chỉnh:

1. interactionRadius: 1.2 đến 1.8 (đề xuất 1.5)
2. tapThresholdSeconds: 0.15 đến 0.25
3. interactionMask: phải chứa layer của item/counter/workstation/customer

## Bước 3: Setup prefab vật phẩm

Ví dụ prefab ly:

1. SpriteRenderer
2. Collider2D
3. Rigidbody2D
4. PickupItem

Nếu là ly chứa nguyên liệu:

1. CupContents

Nếu có trạng thái nấu/cháy:

1. IngredientStateMachine
2. (Tùy chọn) ParticleSystem kéo vào steamVfx
3. (Tùy chọn) WorldSpaceProgressBar

## Bước 4: Setup quầy đặt vật

Mỗi quầy:

1. Gắn CounterSlot.cs
2. Có Collider2D
3. Tạo child Empty tên SnapPoint
4. Kéo SnapPoint vào field snapPoint

## Bước 5: Setup Workstation

Mỗi máy pha/chế biến:

1. Gắn Workstation.cs
2. Điền ingredientOutput, processTime
3. Nếu muốn tạo item vật lý:

1. bật spawnPickupItem
2. gán outputPrefab
3. gán outputSpawnPoint

4. Nếu muốn xử lý state nguyên liệu đang cầm:

1. processHeldIngredient
2. startCookingHeldIngredient

5. VFX/SFX:

1. shakeVisual
2. grinderLoopSource (hoặc để script tự add)
3. grinderLoopClip

6. Incident + Repair:

1. incidentIconText (có thể để trống để auto tạo)
2. incidentIconOffset
3. repairHoldSeconds

## Bước 6: Setup khách hàng

Trên prefab Customer:

1. Gắn Customer.cs
2. Có Collider2D
3. Cấu hình order theo 1 trong 2 cách:

1. Cách A (dễ): dùng DrinkData trong field order
2. Cách B (data-driven): dùng recipeBook + recipeName

4. Chỉnh maxPatience, moveSpeed, stopDistance

## Bước 7: Setup sinh khách và hàng chờ

Tạo object HeatDirector:

1. Gắn HeatDirector.cs
2. Gán customerPrefab
3. Gán spawnPoint
4. Gán queuePositions theo thứ tự gần quầy đến xa
5. Gán exitPoint
6. Gán possibleDrinks

## Bước 8: Setup dashboard UI

1. Tạo Canvas (Screen Space Overlay)
2. Tạo TextMeshProUGUI ở góc màn hình
3. Tạo object gắn OrderDashboardUI.cs
4. Kéo TMP text vào orderListText
5. Chỉnh refreshInterval (đề xuất 0.2 đến 0.3)

## Bước 9: Setup công thức JSON

1. Tạo object RecipeBook trong scene, gắn RecipeBook.cs
2. Kéo file Assets/Settings/recipes.json vào recipeFile
3. Bảo đảm tên nguyên liệu trùng khớp giữa JSON và item thực tế

Ví dụ JSON:

```json
{
  "recipes": [
    {
      "drinkName": "CafeLatte",
      "ingredients": ["CaPhe", "SuaNong"]
    }
  ]
}
```

## Bước 10: Setup chaos layer

1. Tạo object ChaosIncidentDirector, gắn ChaosIncidentDirector.cs
2. Bật autoFindWorkstations hoặc kéo tay danh sách workstation
3. Chỉnh interval/duration sự cố

Vật cản động:

1. Gắn ChaosCrowdMover.cs
2. Gán waypoints

## Bước 11: Setup game over và nâng cấp

Trên GameManager, gán:

1. scoreText
2. reputationText
3. holdingsText
4. gameOverPanel
5. runSummaryText
6. upgradeResultText

Button nâng cấp (OnClick):

1. GameManager.BuyUpgradeBrew
2. GameManager.BuyUpgradePatience
3. GameManager.BuyUpgradeStability

## 4. Checklist Test Nhanh

1. Nhấn E gần item -> nhặt item.
2. Nhấn E gần quầy -> đặt item và snap đúng vị trí.
3. Click workstation -> máy chạy progress.
4. Có tiếng máy, rung máy khi pha.
5. Khách sinh ra và lên dashboard.
6. Giao đúng ly -> ting ting, tăng điểm.
7. Hết thời gian -> fail đơn, trừ điểm/reputation.
8. Sự cố xuất hiện -> icon hiện trên máy.
9. Giữ E gần máy bị lỗi -> sửa xong, máy hoạt động lại.

## 5. Lỗi Thường Gặp

1. Không nhặt được item:

1. thiếu Collider2D
2. thiếu PickupItem
3. sai interactionMask

2. Dashboard không hiển thị:

1. thiếu OrderManager trong scene
2. chưa gán orderListText

3. Công thức không match:

1. tên nguyên liệu không đồng nhất
2. CupContents chưa chứa đủ thành phần

4. Không sửa được máy:

1. đứng quá xa máy
2. máy chưa có sự cố active
3. giữ E chưa đủ repairHoldSeconds

## 6. Cấu Hình Gợi Ý

1. interactionRadius: 1.5
2. tapThresholdSeconds: 0.2
3. processTime: 3.0
4. repairHoldSeconds: 2.0
5. maxPatience: 20
6. refreshInterval dashboard: 0.25

---

Nếu bạn muốn, mình có thể tạo thêm một bản Quickstart 5 phút (rút gọn một trang) để bạn chỉ cần tick checklist là chạy được ngay.
