# Huong Dan Setup Unity (Chi Tiet Cho Nguoi Moi)

Tai lieu nay huong dan tung buoc de ban chay duoc du an va hieu cac he thong trong game cafe.

## 1) Ban Do He Thong (Nhin Tong Quan)

Game co 8 nhom chuc nang chinh:

1. Dieu khien nguoi choi: nhat/thả vat, click tuong tac.
2. Vat pham: ly/nguyen lieu co the cam, dat len quay.
3. Workstation: pha che, loi su co, sua may.
4. Customer + Order: khach vao, doi, nhan do, roi di.
5. Dashboard: hien danh sach don hang goc man hinh.
6. Cong thuc data-driven: doc JSON de match mon.
7. VFX/SFX: am thanh, khoi nong, icon canh bao.
8. Game loop + upgrade: game over, tong ket, mua nang cap.

Neu ban moi bat dau, hay setup theo dung thu tu trong tai lieu nay.

---

## 2) Giai Thich Nhanh Cac Khai Niem (Tu Dien)

1. `Prefab`: Mau doi tuong dung de spawn nhieu lan.
2. `Component`: Script/Renderer/Collider gan tren GameObject.
3. `Inspector`: O ben phai Unity, noi ban keo-tha reference va chinh field.
4. `SerializeField`: Bien private nhung van hien trong Inspector.
5. `LayerMask`: Bo loc layer khi raycast/overlap.
6. `Collider2D`: Vung va cham 2D de nhan click/trigger.
7. `Rigidbody2D`: Vat ly cho object 2D.
8. `World Space UI`: UI nam trong khong gian game (khong phai HUD canvas 2D thong thuong).
9. `ScriptableObject`: Tai san data trong Unity (vi du DrinkData).
10. `Singleton`: Mau script co 1 instance global (GameManager, OrderManager...).

---

## 3) Danh Sach Script Va Cong Dung

1. `Assets/Scripts/PlayerController.cs`
- Input chuot + phim E.
- Nhat/thả vat, dat vao CounterSlot.
- Giu E de sua Workstation khi dang bi su co.

2. `Assets/Scripts/PickupItem.cs`
- Dinh nghia vat pham co the cam.
- Ho tro item nang (do tre khi follow tay).

3. `Assets/Scripts/CounterSlot.cs`
- O dat vat tren quay.
- Tu dong snap vao diem giua (`snapPoint`).

4. `Assets/Scripts/Workstation.cs`
- Pha che theo thoi gian.
- Spawn output item.
- Quan ly su co (hong may, tran sua, het hat) + sua may.
- SFX may xay + rung + icon su co.

5. `Assets/Scripts/IngredientStateMachine.cs`
- State nguyen lieu: `Raw -> Processed -> Cooking -> Burnt`.
- Dieu khien khoi nong va progress nho.

6. `Assets/Scripts/Customer.cs`
- AI khach: vao, doi, roi di.
- Countdown patience.
- Nhan ly va kiem tra dung sai.

7. `Assets/Scripts/HeatDirector.cs`
- Spawn khach ngau nhien theo heat.
- Quan ly queue va vi tri xep hang.

8. `Assets/Scripts/OrderManager.cs`
- Luu danh sach don dang active.
- Thong ke complete/fail.

9. `Assets/Scripts/OrderDashboardUI.cs`
- HUD hien danh sach don + countdown.

10. `Assets/Scripts/RecipeBook.cs` + `Assets/Scripts/RecipeMatcher.cs`
- Doc cong thuc tu JSON.
- So khop thanh phan ly voi cong thuc.

11. `Assets/Scripts/AudioFeedbackManager.cs`
- Quan ly SFX global (ting ting, canh bao su co).

12. `Assets/Scripts/UpgradeManager.cs`
- Nang cap qua score, luu PlayerPrefs.

13. `Assets/Scripts/GameManager.cs`
- Score, reputation, game over panel, run summary.

14. `Assets/Scripts/ChaosIncidentDirector.cs` + `Assets/Scripts/ChaosCrowdMover.cs`
- Lop "hon loan": su co ngau nhien + vat can dong.

---

## 4) Setup Tung Buoc (Lam Theo Tu Tren Xuong)

## Buoc 0: Kiem tra package co ban

1. Mo Unity Package Manager.
2. Dam bao da co:
- Input System
- TextMeshPro
- 2D packages can thiet

Neu project hoi cai dat Input System, chon `Both` hoac `Input System Package (New)`.

## Buoc 1: Tao cac Manager trong Scene

Trong Hierarchy, tao cac GameObject rong:

1. `GameManager`
- Gan script `GameManager`.

2. `OrderManager`
- Gan script `OrderManager`.

3. `UpgradeManager`
- Gan script `UpgradeManager`.

4. `AudioFeedbackManager`
- Gan script `AudioFeedbackManager`.
- Gan clip:
  - `defaultGrinderLoop`
  - `orderCompleteChime`
  - `incidentAlert`

Luu y: moi manager chi nen co 1 object trong scene.

## Buoc 2: Setup Player

1. Chon object Player.
2. Gan script `PlayerController`.
3. Tao 1 child empty dat ten `HandAnchor` o vi tri tay.
4. Keo `HandAnchor` vao field `handAnchor`.
5. Chinh:
- `interactionRadius`: 1.2-1.8 (de test de su dung 1.5)
- `tapThresholdSeconds`: 0.15-0.25
- `interactionMask`: chua layer item/counter/workstation/customer

Neu khong nhat duoc item, 90% la sai layer mask hoac item thieu Collider2D.

## Buoc 3: Setup Item Prefab

Tao prefab `CupItem` (hoac item tuy ban):

1. Component bat buoc:
- `SpriteRenderer`
- `Collider2D`
- `Rigidbody2D`
- `PickupItem`

2. Neu item la ly co thanh phan:
- Gan `CupContents`

3. Neu item co qua trinh nong/chay:
- Gan `IngredientStateMachine`
- (Tuy chon) gan `ParticleSystem` vao `steamVfx`
- (Tuy chon) gan `WorldSpaceProgressBar`

4. Trong `PickupItem`:
- `ingredientName`: vi du `CaPhe` hoac `SuaNong`
- `heavyItem`: bat neu muon cam co do tre ro

## Buoc 4: Setup Counter (quay dat vat)

Moi quay dat vat:

1. Gan `CounterSlot`.
2. Dam bao co `Collider2D`.
3. Tao child empty ten `SnapPoint` dung giua mat quay.
4. Keo `SnapPoint` vao field `snapPoint`.

Ket qua: khi drop vao quay se auto can giua.

## Buoc 5: Setup Workstation

Moi may pha/che bien:

1. Gan script `Workstation`.
2. Field co ban:
- `ingredientOutput`: ten nguyen lieu output
- `processTime`: 2-5s

3. Output:
- Bat `spawnPickupItem`
- Gan `outputPrefab` = prefab item o Buoc 3
- Gan `outputSpawnPoint` (child empty o mieng may)

4. State action (neu dung):
- `processHeldIngredient`
- `startCookingHeldIngredient`

5. VFX/SFX:
- `shakeVisual`: child model may
- `grinderLoopSource`: AudioSource (co the de script tu tao)
- `grinderLoopClip`: clip may xay

6. Incident UI & Repair:
- `incidentIconText`: co the de trong (auto tao)
- `incidentIconOffset`: vi tri icon tren dau may
- `repairHoldSeconds`: 1.5-3.0

## Buoc 6: Setup Customer prefab

1. Gan script `Customer`.
2. Dam bao co `Collider2D`.
3. Chon 1 trong 2 cach order:

Cach A - ScriptableObject (de setup)
- Gan `order` = 1 `DrinkData` asset.

Cach B - JSON data-driven
- Gan `recipeBook` + `recipeName`.

4. Chinh AI:
- `moveSpeed` ~ 2.0
- `stopDistance` ~ 0.05
- `maxPatience` 15-30

## Buoc 7: Setup HeatDirector (spawn + queue)

1. Tao object `HeatDirector`, gan script cung ten.
2. Gan:
- `customerPrefab`
- `spawnPoint`
- `queuePositions` (array theo thu tu hang)
- `exitPoint`
- `possibleDrinks` (neu dung DrinkData)

3. Chinh nhiet do spawn:
- `firstOrderDelay`
- `minSpawnRate`
- `maxSpawnRate`

## Buoc 8: Setup dashboard UI

1. Tao Canvas (Screen Space Overlay).
2. Tao TMP text goc phai tren.
3. Tao object `OrderDashboardUI`, gan script.
4. Keo TMP text vao `orderListText`.
5. Chinh:
- `refreshInterval` 0.2-0.3
- `sortByRemainingTime` = true

## Buoc 9: Setup recipe JSON

1. Trong scene, tao object `RecipeBook`, gan script.
2. Keo file `Assets/Settings/recipes.json` vao field `recipeFile`.
3. Mau du lieu JSON:

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

Luu y: ten trong JSON phai trung voi ten ingredient trong item/cup.

## Buoc 10: Setup chaos layer

1. Tao object `ChaosIncidentDirector`, gan script.
2. Bat `autoFindWorkstations` hoac keo tay danh sach may.
3. Chinh interval + duration su co.

Vat can dong:
1. Tao object crowd obstacle.
2. Gan `ChaosCrowdMover`.
3. Gan array `waypoints`.

## Buoc 11: Setup game over + upgrade UI

1. Trong `GameManager` gan:
- `scoreText`, `reputationText`, `holdingsText`
- `gameOverPanel`
- `runSummaryText`
- `upgradeResultText`

2. Tao 3 button upgrade, bind OnClick:
- `GameManager.BuyUpgradeBrew()`
- `GameManager.BuyUpgradePatience()`
- `GameManager.BuyUpgradeStability()`

3. Neu muon hien level/cost upgrade, gan TMP vao `UpgradeManager.upgradeInfoText`.

---

## 5) Thu Tu Test De Nhat (10 Phut)

1. Vao Play Mode.
2. Nhac item bang phim E.
3. Dat item len quay bang phim E.
4. Click workstation de pha.
5. Kiem tra rung + am thanh + progress.
6. Spawn customer, dashboard co don.
7. Serve dung mon: nghe ting ting, score tang.
8. Cho timeout: fail don, tru diem/rep.
9. Cho su co xay ra: icon hien tren may.
10. Giu E sua may: icon tat, may dung lai binh thuong.

---

## 6) Loi Thuong Gap Va Cach Sua

1. Khong click duoc may/khach
- Object thieu Collider2D.
- Layer bi loai khoi `interactionMask`.

2. Khong nhat duoc item
- `handAnchor` chua gan.
- Item khong co `PickupItem` hoac Collider2D.

3. Dashboard khong hien don
- Scene thieu `OrderManager`.
- `OrderDashboardUI.orderListText` chua gan.

4. Cong thuc khong match du da dung
- Ten ingredient khong trung (phan biet dau cach, ky tu).
- `CupContents` chua add du thanh phan.
- `recipeName` khong trung `drinkName` trong JSON.

5. Su co khong xuat hien
- Chua co `ChaosIncidentDirector`.
- Khong tim thay `Workstation` (auto find fail).

6. Giu E khong sua duoc
- Chua dung gan may bi su co.
- Chua giu du `repairHoldSeconds`.

---

## 7) Cau Hinh Goi Y (De Choi On Dinh)

1. `PlayerController.interactionRadius = 1.5`
2. `PlayerController.tapThresholdSeconds = 0.2`
3. `Workstation.processTime = 3.0`
4. `Workstation.repairHoldSeconds = 2.0`
5. `Customer.maxPatience = 20`
6. `OrderDashboardUI.refreshInterval = 0.25`
7. `HeatDirector.minSpawnRate = 2.0`, `maxSpawnRate = 7.0`

---

## 8) Neu Ban Muon Don Gian Hoa Them

Minh co the tao them 2 tai lieu:

1. `SETUP_QUICKSTART_5PHUT_VI.md`
- Ban rut gon, chi 1 trang, setup de play ngay.

2. `CHEAT_SHEET_INSPECTOR_VI.md`
- Bang field nao can gan, field nao bo trong, cho tung script.

Chi can ban nhan: "tao quickstart + cheat sheet" la minh lam tiep.
