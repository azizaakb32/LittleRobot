# Kichik Robot — O'yin dizayni hujjati

## 1. Qisqacha g'oya
2D mini-platformer. Kichkina robot zavod, o'rmon, muz, cho'l va kosmik stansiya bo'ylab sayohat qiladi. Har bir daraja **30–60 soniya** davom etadi. Maqsad: kalitni topib, eshikka yetib borish. Yulduzlar bilan baholanadi.

**Nima uchun ishlaydi:** qisqa darajalar → "yana bitta daraja" effekti; yulduzlar → qayta o'ynash sababi; oddiy boshqaruv → mobil uchun qulay.

## 2. Boshqaruv (mobil)
- Chap pastda: ◀ ▶ tugmalari
- O'ng pastda: SAKRASH tugmasi (qancha uzoq bossa, shuncha baland sakraydi)
- Kompyuterda sinash uchun: A/D yoki strelkalar + Space

## 3. Asosiy sikl
1. Darajani boshlash
2. Tangalarni yig'ish (ixtiyoriy) va **kalit**ni olish (majburiy)
3. Tuzoqlardan (tikanlar, tushib ketish) qochish — o'lsang, boshiga qaytasan (jazo yo'q, tez respawn)
4. Eshikka yetish → yulduzlar beriladi → keyingi daraja

## 4. Yulduzlar tizimi
| Yulduz | Shart |
|---|---|
| ⭐ | Darajani tugatish |
| ⭐⭐ | Barcha tangalarni yig'ish |
| ⭐⭐⭐ | Belgilangan vaqtdan (par time) tez tugatish |

## 5. Darajalar tuzilishi (50 ta)
| Dunyo | Darajalar | Yangi element |
|---|---|---|
| 1. Zavod | 1–10 | Oddiy sakrash, tikanlar |
| 2. O'rmon | 11–20 | Harakatlanuvchi platformalar |
| 3. Muz | 21–30 | Sirpanchiq yer |
| 4. Cho'l | 31–40 | Yo'qolib boruvchi platformalar |
| 5. Kosmos | 41–50 | Aralash, qiyin kombinatsiyalar |

**Qiyinlik qoidasi:** har bir yangi elementni avval xavfsiz joyda ko'rsat, keyin qiyinlashtir. Dunyoning oxirgi darajasi "boss-daraja" (eng qiyin).

## 6. Monetizatsiya (Google Play)
- **Rewarded reklama:** "Reklamani ko'r → o'lgan joyingdan davom et" yoki "tangalarni 2 barobar qil". O'yinchi o'zi tanlaydi — eng yaxshi daromad/tajriba nisbati.
- **Interstitial:** har 3–4 darajadan keyin, daraja o'rtasida **hech qachon**.
- **Banner:** faqat menyuda (o'yin paytida emas).
- **"Reklamasiz" xaridi:** bir martalik (~$2–3). Interstitial va bannerni o'chiradi, rewarded qoladi.
- **Kosmetika:** tangalarga robot ranglari/skinlari sotib olish.
- SDK: Google AdMob (Unity uchun rasmiy plagin) + Unity IAP.

## 7. Rejа (taxminiy)
| Hafta | Ish |
|---|---|
| 1 | Prototip: harakat, sakrash, 3 ta test daraja |
| 2 | Barcha tuzoqlar, kalit, eshik, UI, 10 ta daraja |
| 3 | Qolgan darajalar, ovoz, menyu, daraja tanlash |
| 4 | Reklama, saqlash, sinov, Google Play'ga tayyorlash |

## 8. Unity'da sozlash (qadam-baqadam)
1. Unity Hub → **New project → 2D (Built-in RP yoki URP 2D)**. Unity 2022 LTS yoki Unity 6.
2. `Edit → Project Settings → Player → Other Settings → Active Input Handling = **Both**`
3. Layer'lar yarating: `Ground`. Tag: `Player`.
4. Skriptlarni `Assets/Scripts/` ichiga tashlang.
5. **Robot (Player):**
   - SpriteRenderer + `Rigidbody2D` (Gravity Scale 3, Freeze Rotation Z) + `CapsuleCollider2D`
   - Tag = `Player`
   - Bola ob'ekt `GroundCheck` (oyoq ostida), `PlayerController`'ga ulang, Ground Layer = `Ground`
6. **Yer/platformalar:** SpriteRenderer + `BoxCollider2D`, Layer = `Ground`. (Tilemap + `TilemapCollider2D` + `CompositeCollider2D` ham bo'ladi.)
7. **Tanga/Kalit:** Sprite + Collider2D (**Is Trigger** ✔) + `Pickup` (Type tanlang)
8. **Tikan:** Sprite + Collider2D (**Is Trigger** ✔) + `Hazard`
9. **Eshik:** Sprite + Collider2D (**Is Trigger** ✔) + `ExitDoor`
10. **Harakatlanuvchi platforma:** Layer = `Ground`, `Rigidbody2D` (Kinematic) + `MovingPlatform`, `PointB` ni belgilang
11. **Kamera:** `CameraFollow` ni Main Camera'ga qo'shing, Target = Robot
12. **Bo'sh ob'ekt `_Game`:** `GameManager` + `MobileInput` ni qo'shing
13. **UI (Canvas):** 3 ta tugma (◀ ▶ Sakrash). Har biriga `Event Trigger` qo'shing:
    - ◀: PointerDown → `MobileInput.LeftDown`, PointerUp → `MobileInput.LeftUp`
    - ▶: PointerDown → `RightDown`, PointerUp → `RightUp`
    - Sakrash: PointerDown → `JumpDown`, PointerUp → `JumpUp`
    - Matnlar: tanga, vaqt, g'alaba paneli (`GameManager`'dagi maydonlarga ulang)

## 9. Bepul resurslar
- Grafika va ovoz: **kenney.nl** (CC0, ruxsatsiz ishlatish mumkin), **opengameart.org** (litsenziyani tekshiring)
- Musiqa: **pixabay.com/music**, **incompetech.com** (shartlarini o'qing)

## 10. Google Play'dan oldin
- Google Play Developer akkaunt (bir martalik $25)
- Build formati: **AAB** (APK emas)
- Maxfiylik siyosati (reklama ishlatsangiz majburiy)
- Ikonka (512×512), "feature graphic" (1024×500), kamida 2 ta skrinshot
- Yangi shaxsiy akkauntlar uchun **yopiq test** talabi bor (odatda 12+ tester, 14 kun) — chiqarishdan oldin Play Console'da joriy shartlarni tekshiring.
