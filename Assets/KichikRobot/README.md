# Kichik Robot: Unity paketi

2D mini-platformer. Paketda tayyor skriptlar, sprite'lar va **sahnalarni bir bosishda yig'adigan** Editor vositasi bor. Ichida: bosh menyu (daraja tanlash, 2 ta dunyo), **20 ta daraja**, HUD, sensorli tugmalar, g'alaba va pauza oynalari. Hammasi `GameDesign` dizaynidagidek.

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

## Robot animatsiyasi
Robot kod orqali animatsiya qilinadi (`PlayerAnimator`, Animator yoki .anim fayllari kerak emas):
- **Turish:** nafas olish va har 3–5 soniyada ko'z qisish
- **Yugurish:** 6 kadrli sikl (oyoq va qo'l harakati, tezlikka qarab tezlashadi)
- **Sakrash / tushish:** alohida kadrlar, havoda cho'zilish, yerga tushganda yassilanish (squash and stretch)
- **O'lganda:** qayta paydo bo'lgach 0.6 soniya miltillaydi
- **Daraja tugaganda:** xursand kadr va sakrab turish

Sozlamalar `Robot > Visual` obyektidagi `PlayerAnimator` da (yugurish tezligi, ko'z qisish oralig'i, yassilanish tezligi). Eski sahnalarda robot animatsiyasi bo'lmaydi: **Kichik Robot > 4. Darajalarni JSON'dan qayta yaratish** ni bosing.

## Menyu bandlari (Kichik Robot)
| Band | Nima qiladi |
|---|---|
| 1. Hammasini yaratish | Sprite sozlamalari, `Ground` layer, `Menu` va barcha `LevelN` sahnalari (`Levels/*.json` dan), Build Settings |
| 2. Yangi daraja qo'shish | `Level1` nusxasidan `Level2`, `Level3`... yaratadi (raqamlar avtomatik) |
| 3. Android uchun sozlash | Gorizontal ekran va AAB formatini yoqadi |
| 4. Darajalarni JSON'dan qayta yaratish | `Levels/level_NN.json` ni tahrirlagandan keyin sahnalarni yangilaydi |

## Darajalar (20 ta)
| Dunyo | Darajalar | Yangi narsalar |
|---|---|---|
| Zavod | 1–10 | tikanlar, chuqurlar, suzuvchi platformalar, zinapoyalar, lift, qo'sh tikan to'siqlari |
| O'rmon | 11–20 | gorizontal harakatlanuvchi platformalar, uzunroq va qiyinroq yo'llar |

Har bir daraja `Levels/level_NN.json` faylida (bloklar, tikanlar, tangalar, kalit, eshik, liftlar, rang mavzusi). Faylni tahrirlab, **Kichik Robot > 4** ni bosing. Yangi dunyolar uchun (Cho'l, Muz, Kosmos) JSON'lar qo'shilsa, menyuda yangi sahifa o'zi paydo bo'ladi (har sahifada 10 daraja).

Daraja o'tishi tekshirilgan: sakrash masofalari robotning fizikasi (tezlik 6, sakrash 13, gravitatsiya x3) bo'yicha hisoblab chiqilgan.

## Yangi daraja yasash (qo'lda)
1. **Kichik Robot > 2. Yangi daraja qo'shish** ni bosing: yangi sahna ochiladi.
2. `Level` obyekti ichidagi platformalar (`Ground`, `Platform`...), tikanlar (`Spikes`), tangalar (`Coin*`), kalit va eshikni siljiting yoki nusxalang.
   - Plitkali bloklarning o'lchamini `Sprite Renderer > Size` va `Box Collider 2D > Size` dan o'zgartiring (ikkalasi bir xil bo'lsin).
3. `_Game` obyektida `Par Time` (3-yulduz uchun vaqt) ni sozlang.
4. Tayyor. Daraja Build Settings'ga o'zi qo'shilgan, `Menu` da katakcha o'zi ochiladi.

Menyuda 20 ta katakcha bor (2 sahifa). Daraja tugagach keyingisi ochiladi (saqlash `PlayerPrefs` orqali).

## Papka tuzilishi
| Papka | Ichida |
|---|---|
| `Runtime/` | O'yin skriptlari: PlayerController, GameManager, MobileInput, TouchButton, Pickup, Hazard, ExitDoor, MovingPlatform, CameraFollow, PlayerAnimator, LevelTile, LevelSelectMenu, Spin, UnityCompat |
| `Editor/` | Sahna quruvchi va daraja ma'lumotlari (faqat Editor'da ishlaydi, o'yin buildiga kirmaydi) |
| `Levels/` | 20 ta daraja JSON fayli |
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
- 21–50 darajalar (dizaynda rejalashtirilgan: Cho'l, Muz, Kosmos dunyolari). Sirpanchiq yer va yo'qoluvchi platformalar hali kodda yo'q.

## Muammo bo'lsa
- **Robot sakramayapti:** `Robot` obyektida `Ground Layer` = `Ground` ekanini va zamin obyektlari `Ground` layerida ekanini tekshiring.
- **Tugmalar bosilmayapti:** sahnada `EventSystem` bo'lishi kerak (builder qo'shadi).
- **Matn ko'rinmayapti:** Console'da xato bormi qarang. Eski Unity'da `Arial.ttf` shrifti ishlatiladi.
