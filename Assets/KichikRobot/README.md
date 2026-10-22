# Kichik Robot: Unity paketi

2D mini-platformer. Paketda tayyor skriptlar, sprite'lar va **sahnalarni bir bosishda yig'adigan** Editor vositasi bor. Ichida: bosh menyu (daraja tanlash), 1-daraja, HUD, sensorli tugmalar, g'alaba va pauza oynalari. Hammasi `GameDesign` dizaynidagidek.

## Talablar
- Unity 2021.3 LTS yoki yangiroq (2022 LTS va Unity 6 ham mos)
- Loyiha turi: **2D** (Built-in yoki URP 2D)
- Eski Input Manager ham, yangi Input System ham ishlaydi
- `Unity UI` (uGUI) paketi o'rnatilgan bo'lishi kerak (2D shablonda bor)

## Tez boshlash
1. Yangi 2D loyiha oching.
2. `Assets > Import Package > Custom Package...` orqali `KichikRobot.unitypackage` ni tanlang va **Import** qiling.
3. Import tugagach dialog chiqadi: **Yaratish** ni bosing. (Chiqmasa: yuqori menyu **Kichik Robot > 1. Hammasini yaratish**.)
4. `Menu` sahnasi ochiladi. **Play** bosing, 1-darajani tanlang.

Boshqaruv: **A/D** yoki strelkalar yuradi, **Space** (yoki W / yuqori strelka) sakraydi. Telefonda ekrandagi tugmalar ishlaydi. Sakrash tugmasini qancha uzoq bossangiz, shuncha baland sakraydi.

## Menyu bandlari (Kichik Robot)
| Band | Nima qiladi |
|---|---|
| 1. Hammasini yaratish | Sprite sozlamalari, `Ground` layer, `Menu` va `Level1` sahnalari, Build Settings |
| 2. Yangi daraja qo'shish | `Level1` nusxasidan `Level2`, `Level3`... yaratadi (raqamlar avtomatik) |
| 3. Android uchun sozlash | Gorizontal ekran va AAB formatini yoqadi |

## Yangi daraja yasash
1. **Kichik Robot > 2. Yangi daraja qo'shish** ni bosing: yangi sahna ochiladi.
2. `Level` obyekti ichidagi platformalar (`GroundA`, `Platform1`...), tikanlar (`Spikes`), tangalar (`Coin*`), kalit va eshikni siljiting yoki nusxalang.
   - Plitkali bloklarning o'lchamini `Sprite Renderer > Size` va `Box Collider 2D > Size` dan o'zgartiring (ikkalasi bir xil bo'lsin).
3. `_Game` obyektida `Par Time` (3-yulduz uchun vaqt) ni sozlang.
4. Tayyor. Daraja Build Settings'ga o'zi qo'shilgan, `Menu` da katakcha o'zi ochiladi.

Menyuda hozir 10 ta katakcha bor. Daraja tugagach keyingisi ochiladi (saqlash `PlayerPrefs` orqali).

## Papka tuzilishi
| Papka | Ichida |
|---|---|
| `Runtime/` | O'yin skriptlari: PlayerController, GameManager, MobileInput, TouchButton, Pickup, Hazard, ExitDoor, MovingPlatform, CameraFollow, LevelTile, LevelSelectMenu, Spin, UnityCompat |
| `Editor/` | Sahna quruvchi (faqat Editor'da ishlaydi, o'yin buildiga kirmaydi) |
| `Sprites/` | Robot, tanga, kalit, eshik, tikan, plitkalar, g'ildirak, UI ikonkalari |
| `Docs/` | O'yin dizayni hujjati (qoidalar, monetizatsiya, Google Play ro'yxati) |
| `Scenes/` | Builder yaratgan sahnalar (`Menu`, `Level1`...) |

## Google Play uchun
1. **Kichik Robot > 3. Android uchun sozlash**.
2. `Project Settings > Player > Android`: Package Name (masalan `com.ismingiz.kichikrobot`), Scripting Backend = **IL2CPP**, ARM64, Keystore.
3. `File > Build Settings > Android`, **Build App Bundle (Google Play)** yoqilgan holda build qiling (`.aab`).
4. Yangi shaxsiy Google Play akkauntlarida yopiq test talabi bo'lishi mumkin. Play Console'da joriy shartlarni tekshiring.

## Hali yo'q (keyingi qadamlar)
- Reklama (AdMob) va xaridlar (IAP). `GameManager.CompleteLevel()` va `OnRewardedDoubleCoins()` da joy belgilangan.
- Ovoz va zarracha effektlari (`Pickup.cs` da joyi belgilangan).
- 2–50 darajalar (dizaynda rejalashtirilgan: O'rmon, Muz, Cho'l, Kosmos dunyolari).

## Muammo bo'lsa
- **Robot sakramayapti:** `Robot` obyektida `Ground Layer` = `Ground` ekanini va zamin obyektlari `Ground` layerida ekanini tekshiring.
- **Tugmalar bosilmayapti:** sahnada `EventSystem` bo'lishi kerak (builder qo'shadi).
- **Matn ko'rinmayapti:** Console'da xato bormi qarang. Eski Unity'da `Arial.ttf` shrifti ishlatiladi.
