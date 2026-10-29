using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// "Sakrovchilar" demo sahnalarini (Menu + Level1) kod orqali yig'adi.
/// Menyu: Sakrovchilar &gt; ...
/// </summary>
public static class SakrovchilarBuilder
{
    const string Root = "Assets/Sakrovchilar";
    const string SpriteDir = Root + "/Sprites/";
    const string SceneDir = Root + "/Scenes/";
    const string PhysicsDir = Root + "/Physics";
    const string CharDir = Root + "/Resources/Characters/";
    public const string CharacterSelectScenePath = SceneDir + "CharacterSelect.unity";
    public const string MenuScenePath = SceneDir + "Menu.unity";
    public const string Level1ScenePath = SceneDir + "Level1.unity";

    // ---------- Ranglar (dizayndan) ----------
    static readonly Color BgColor = Hex("1B2540");
    static readonly Color BodyColor = Hex("2C3A5C");
    static readonly Color TealColor = Hex("6FD3C0");
    static readonly Color DialogColor = Hex("2C3A5C");
    static readonly Color OrangeColor = Hex("FF8A3D");
    static readonly Color YellowColor = Hex("FFC93C");
    static readonly Color LightText = Hex("EAF0FF");
    static readonly Color MutedText = Hex("9FB1DA");
    static readonly Color DarkText = Hex("14203D");
    static readonly Color PanelColor = new Color(10f / 255f, 16f / 255f, 34f / 255f, 0.55f);
    static readonly Color ButtonDark = new Color(10f / 255f, 16f / 255f, 34f / 255f, 0.45f);

    // ---------- Sprite sozlamalari ----------
    struct SpriteSpec
    {
        public string name;
        public float ppu;
        public int border;
        public SpriteSpec(string n, float p, int b)
        {
            name = n;
            ppu = p;
            border = b;
        }
    }

    static readonly SpriteSpec[] Specs =
    {
        new SpriteSpec("coin", 64, 0),
        new SpriteSpec("key", 64, 0),
        new SpriteSpec("door", 64, 0),
        new SpriteSpec("spikes", 64, 0),
        new SpriteSpec("shooter_a", 64, 0),
        new SpriteSpec("shooter_fire", 64, 0),
        new SpriteSpec("shooter_dead", 64, 0),
        new SpriteSpec("bullet", 64, 0),
        new SpriteSpec("enemy_a", 64, 0),
        new SpriteSpec("enemy_b", 64, 0),
        new SpriteSpec("enemy_dead", 64, 0),
        new SpriteSpec("tile_body", 64, 0),
        new SpriteSpec("tile_top", 64, 0),
        new SpriteSpec("gear", 64, 0),
        new SpriteSpec("ui_round", 100, 24),
        new SpriteSpec("ui_circle", 100, 0),
        new SpriteSpec("icon_chevron", 100, 0),
        new SpriteSpec("icon_up", 100, 0),
        new SpriteSpec("icon_pause", 100, 0),
        new SpriteSpec("icon_star", 100, 0),
        new SpriteSpec("icon_home", 100, 0),
        new SpriteSpec("icon_restart", 100, 0),
        new SpriteSpec("icon_lock", 100, 0),
        new SpriteSpec("icon_play", 100, 0),
    };

    static readonly Vector2 TL = new Vector2(0f, 1f);
    static readonly Vector2 TR = new Vector2(1f, 1f);
    static readonly Vector2 TC = new Vector2(0.5f, 1f);
    static readonly Vector2 BL = new Vector2(0f, 0f);
    static readonly Vector2 BR = new Vector2(1f, 0f);
    static readonly Vector2 BC = new Vector2(0.5f, 0f);
    static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);

    // =====================================================================
    //  MENYU BANDLARI
    // =====================================================================

    [MenuItem("Sakrovchilar/1. Hammasini yaratish (Menu + barcha darajalar)")]
    public static void BuildAll()
    {
        if (File.Exists(Level1ScenePath) &&
            !EditorUtility.DisplayDialog("Sakrovchilar",
                "Menu va Level sahnalari qaytadan yaratiladi. Ularga qo'lda kiritgan o'zgarishlaringiz o'chib ketadi.\n\nDavom etamizmi?",
                "Davom etish", "Bekor qilish"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        AssetDatabase.Refresh();
        if (!ConfigureSprites()) return;
        EnsureFolder(SceneDir.TrimEnd('/'));

        int count = BuildAllLevels();
        if (count == 0)
        {
            EditorUtility.DisplayDialog("Sakrovchilar", "Levels/level_01.json topilmadi. Paketni qayta import qiling.", "OK");
            return;
        }
        BuildMenuScene(count);
        BuildCharacterSelectScene();
        PlayerSettings.productName = GameInfo.Title;
        ApplyLandscape();
        ApplyIcon();
        RefreshBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(CharacterSelectScenePath);
        EditorUtility.DisplayDialog("Sakrovchilar",
            "Tayyor! " + count + " ta daraja yaratildi. Personaj tanlash sahnasi ochildi, Play bosing.\n\n" +
            "Klaviatura: A/D yoki strelkalar, Space. Ekrandagi tugmalar telefon uchun.", "OK");
    }

    [MenuItem("Sakrovchilar/4. Darajalarni JSON'dan qayta yaratish")]
    public static void RebuildLevelsFromJson()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        AssetDatabase.Refresh();
        if (!ConfigureSprites()) return;
        EnsureFolder(SceneDir.TrimEnd('/'));

        int count = BuildAllLevels();
        BuildMenuScene(count);
        BuildCharacterSelectScene();
        PlayerSettings.productName = GameInfo.Title;
        ApplyLandscape();
        ApplyIcon();
        RefreshBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(CharacterSelectScenePath);
        EditorUtility.DisplayDialog("Sakrovchilar", count + " ta daraja Levels/*.json dan qayta yaratildi.", "OK");
    }

    [MenuItem("Sakrovchilar/2. Yangi daraja qo'shish (Level1 nusxasi)")]
    public static void AddLevel()
    {
        if (!File.Exists(Level1ScenePath))
        {
            EditorUtility.DisplayDialog("Sakrovchilar", "Avval 'Sakrovchilar > 1. Hammasini yaratish' ni bajaring.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        int n = 2;
        while (File.Exists(LevelPath(n))) n++;

        AssetDatabase.CopyAsset(Level1ScenePath, LevelPath(n));
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.OpenScene(LevelPath(n));
        var gm = UnityCompat.FindFirst<GameManager>();
        if (gm != null)
        {
            gm.levelNumber = n;
            gm.nextSceneName = "Level" + (n + 1);
            EditorUtility.SetDirty(gm);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        RefreshBuildSettings();
        EditorUtility.DisplayDialog("Sakrovchilar",
            "Level" + n + " yaratildi va Build Settings'ga qo'shildi.\n" +
            "Platformalar, tikanlar va tangalarni siljitib yangi daraja yasang.", "OK");
    }

    [MenuItem("Sakrovchilar/3. Android uchun sozlash (gorizontal ekran + AAB)")]
    public static void AndroidSetup()
    {
        ApplyLandscape();
        ApplyIcon();
        EditorUserBuildSettings.buildAppBundle = true;
        EditorUtility.DisplayDialog("Sakrovchilar",
            "Gorizontal ekran va AAB (Android App Bundle) yoqildi.\n\n" +
            "Qolganini Project Settings > Player > Android da sozlang: Package Name, Keystore, versiya.", "OK");
    }

    /// <summary>Faqat gorizontal (landscape) ekran: telefon aylantirilsa ham portret bo'lmaydi.</summary>
    static void ApplyLandscape()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
    }

    /// <summary>Ilova ikonkasini (Icons/app_icon.png) Player Settings'ga o'rnatadi.</summary>
    static void ApplyIcon()
    {
        string path = Root + "/Icons/app_icon.png";
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType = TextureImporterType.Default;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.mipmapEnabled = false;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { tex }, IconKind.Application);
    }

    static string LevelPath(int n) { return SceneDir + "Level" + n + ".unity"; }

    // =====================================================================
    //  SPRITE VA PAPKALAR
    // =====================================================================

    static bool ConfigureSprites()
    {
        foreach (var spec in Specs)
            if (!ConfigureOne(SpriteDir + spec.name + ".png", spec.ppu, spec.border)) return false;

        string[] frames = { "idle", "blink", "run0", "run1", "run2", "run3", "run4", "run5", "jump", "fall", "happy" };
        foreach (var character in CharacterCatalog.All)
            foreach (var frame in frames)
                if (!ConfigureOne(CharDir + character.id + "_" + frame + ".png", 80f, 0)) return false;
        return true;
    }

    static bool ConfigureOne(string path, float ppu, int border)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null)
        {
            EditorUtility.DisplayDialog("Sakrovchilar", "Sprite topilmadi: " + path + "\nPaketni qayta import qiling.", "OK");
            return false;
        }

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = ppu;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.filterMode = FilterMode.Bilinear;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.spriteBorder = new Vector4(border, border, border, border);

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(settings);

        ti.SaveAndReimport();
        return true;
    }

    /// <summary>Personaj kadri (Resources/Characters ichidan), masalan "robot_idle".</summary>
    static Sprite SC(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(CharDir + name + ".png");
    }

    static Sprite S(string name)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + name + ".png");
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        string leaf = Path.GetFileName(folder);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    static int EnsureLayer(string layerName)
    {
        int existing = LayerMask.NameToLayer(layerName);
        if (existing >= 0) return existing;

        var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return 0;

        var tagManager = new SerializedObject(assets[0]);
        var layers = tagManager.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            var sp = layers.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }
        Debug.LogWarning("[Sakrovchilar] Bo'sh layer topilmadi. 'Ground' layerini qo'lda yarating.");
        return 0;
    }

    static void RefreshBuildSettings()
    {
        var ours = new List<EditorBuildSettingsScene>();
        var ourPaths = new HashSet<string>();

        if (File.Exists(CharacterSelectScenePath))
        {
            ours.Add(new EditorBuildSettingsScene(CharacterSelectScenePath, true));
            ourPaths.Add(CharacterSelectScenePath);
        }
        if (File.Exists(MenuScenePath))
        {
            ours.Add(new EditorBuildSettingsScene(MenuScenePath, true));
            ourPaths.Add(MenuScenePath);
        }
        for (int n = 1; File.Exists(LevelPath(n)); n++)
        {
            ours.Add(new EditorBuildSettingsScene(LevelPath(n), true));
            ourPaths.Add(LevelPath(n));
        }
        foreach (var existing in EditorBuildSettings.scenes)
        {
            // Mavjud bo'lmagan (masalan, o'chirilgan eski KichikRobot) sahnalar build'ni buzadi: tashlab yuboramiz
            if (string.IsNullOrEmpty(existing.path) || !File.Exists(existing.path)) continue;
            if (!ourPaths.Contains(existing.path)) ours.Add(existing);
        }

        EditorBuildSettings.scenes = ours.ToArray();
    }

    // =====================================================================
    //  1-DARAJA SAHNASI
    // =====================================================================

    static void BuildLevelScene(LevelData d, string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int ground = EnsureLayer("Ground");
        var root = new GameObject("Level").transform;

        Color bg = Hex(d.bg), body = Hex(d.body), topC = Hex(d.top), gearC = Hex(d.gear);
        Color moverBody = Hex(d.moverBody), moverTop = Hex(d.moverTop);

        // ----- Kamera -----
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bg;
        cam.nearClipPlane = -20f;
        cam.farClipPlane = 50f;
        camGo.transform.position = new Vector3(d.spawn.x, 1f, -10f);
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow>();
        follow.useBounds = true;
        follow.minPos = new Vector2(d.boundsMin.x, d.boundsMin.y);
        follow.maxPos = new Vector2(d.boundsMax.x, d.boundsMax.y);

        // ----- Orqa fon -----
        if (d.gears != null)
            foreach (var g in d.gears)
                MakeGear(root, new Vector3(g.x, g.y, 0f), g.scale, g.speed, gearC);

        // ----- Zamin, platformalar, devorlar -----
        foreach (var b in d.blocks)
        {
            Color topColor = b.kind == "wall" ? body : topC;
            Block(string.IsNullOrEmpty(b.n) ? "Block" : b.n, root, b.x0, b.y0, b.x1, b.y1, body, topColor, ground);
        }

        // ----- Liftlar va harakatlanuvchi platformalar -----
        if (d.movers != null)
        {
            foreach (var m in d.movers)
            {
                float hw = m.w * 0.5f;
                var lift = Block("Mover", root, m.ax - hw, m.ay - 0.25f, m.ax + hw, m.ay + 0.25f, moverBody, moverTop, ground);
                var rb = lift.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                var mover = lift.AddComponent<MovingPlatform>();
                var pointB = new GameObject("MoverPointB");
                pointB.transform.SetParent(root, false);
                pointB.transform.position = new Vector3(m.bx, m.by, 0f);
                mover.pointB = pointB.transform;
                mover.speed = m.speed > 0f ? m.speed : 2f;
            }
        }

        // ----- Tikanlar -----
        if (d.spikes != null)
            foreach (var s in d.spikes) MakeSpike(root, new Vector3(s.x, s.y, 0f));
        if (d.enemies != null)
            foreach (var e in d.enemies) MakeEnemy(root, e);
        if (d.shooters != null)
            foreach (var sh in d.shooters) MakeShooter(root, sh);

        // ----- Tangalar va kalit -----
        if (d.coins != null)
        {
            for (int i = 0; i < d.coins.Count; i++)
                MakePickup(root, "Coin" + (i + 1), S("coin"), new Vector3(d.coins[i].x, d.coins[i].y, 0f), Pickup.PickupType.Coin, 0.35f);
        }
        MakePickup(root, "Key", S("key"), new Vector3(d.key.x, d.key.y, 0f), Pickup.PickupType.Key, 0.4f);

        // ----- Eshik -----
        var doorGo = new GameObject("ExitDoor");
        doorGo.transform.SetParent(root, false);
        doorGo.transform.position = new Vector3(d.door.x, d.door.y, 0f);
        var doorSr = doorGo.AddComponent<SpriteRenderer>();
        doorSr.sprite = S("door");
        doorSr.sortingOrder = 2;
        var doorCol = doorGo.AddComponent<BoxCollider2D>();
        doorCol.isTrigger = true;
        doorCol.size = new Vector2(1.1f, 1.9f);
        var exit = doorGo.AddComponent<ExitDoor>();
        exit.doorSprite = doorSr;

        // ----- Robot -----
        var player = MakePlayer(ground, new Vector3(d.spawn.x, d.spawn.y, 0f));
        follow.target = player.transform;

        // ----- Boshqaruvchi obyekt -----
        var gmGo = new GameObject("_Game");
        var gm = gmGo.AddComponent<GameManager>();
        gm.levelNumber = d.number;
        gm.worldName = d.world;
        gm.worldFirstLevel = d.worldFirst;
        gm.parTime = d.par;
        gm.nextSceneName = "Level" + (d.number + 1);
        gm.menuSceneName = "Menu";

        BuildGameUI(gm);

        EditorSceneManager.SaveScene(scene, path);
    }

    static string LevelJsonPath(int n)
    {
        return Root + "/Levels/level_" + n.ToString("00") + ".json";
    }

    static LevelData LoadLevel(int n)
    {
        string path = LevelJsonPath(n);
        if (!File.Exists(path)) return null;
        var data = JsonUtility.FromJson<LevelData>(File.ReadAllText(path));
        if (data != null && data.number <= 0) data.number = n;
        return data;
    }

    /// <summary>Levels/ papkasidagi barcha JSON'dan sahnalarni yaratadi. Yaratilgan sonni qaytaradi.</summary>
    static int BuildAllLevels()
    {
        int built = 0;
        for (int n = 1; ; n++)
        {
            var data = LoadLevel(n);
            if (data == null) break;
            BuildLevelScene(data, LevelPath(n));
            built++;
        }
        return built;
    }

    static void MakeEnemy(Transform parent, LevelEnemy e)
    {
        var go = new GameObject("Enemy");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(e.x, e.y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = S("enemy_a");
        sr.sortingOrder = 6;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.55f);
        col.offset = new Vector2(0f, -0.03f);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var en = go.AddComponent<Enemy>();
        en.sr = sr;
        en.frameA = S("enemy_a");
        en.frameB = S("enemy_b");
        en.deadFrame = S("enemy_dead");
        en.minX = e.minX;
        en.maxX = e.maxX;
        en.speed = e.speed;
    }

    static void MakeShooter(Transform parent, LevelShooter e)
    {
        var go = new GameObject("Shooter");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(e.x, e.y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = S("shooter_a");
        sr.sortingOrder = 6;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.6f);
        col.offset = new Vector2(0.05f, -0.02f);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var sh = go.AddComponent<ShooterEnemy>();
        sh.sr = sr;
        sh.idleFrame = S("shooter_a");
        sh.fireFrame = S("shooter_fire");
        sh.deadFrame = S("shooter_dead");
        sh.bulletSprite = S("bullet");
        sh.interval = e.interval;
        sh.bulletSpeed = e.speed;
    }

    static void MakeGear(Transform parent, Vector3 pos, float scale, float speed, Color color)
    {
        var go = new GameObject("Gear");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = S("gear");
        sr.color = color;
        sr.sortingOrder = -20;
        var spin = go.AddComponent<Spin>();
        spin.degreesPerSecond = speed;
    }

    static GameObject Block(string name, Transform parent, float x0, float y0, float x1, float y1, Color body, Color top, int layer)
    {
        float w = x1 - x0;
        float h = y1 - y0;

        var go = new GameObject(name);
        go.layer = layer;
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3((x0 + x1) * 0.5f, (y0 + y1) * 0.5f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = S("tile_body");
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(w, h);
        sr.color = body;
        sr.sortingOrder = 0;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(w, h);

        var strip = new GameObject("Top");
        strip.layer = layer;
        strip.transform.SetParent(go.transform, false);
        strip.transform.localPosition = new Vector3(0f, h * 0.5f - 0.125f, 0f);
        var ssr = strip.AddComponent<SpriteRenderer>();
        ssr.sprite = S("tile_top");
        ssr.drawMode = SpriteDrawMode.Tiled;
        ssr.tileMode = SpriteTileMode.Continuous;
        ssr.size = new Vector2(w, 0.25f);
        ssr.color = top;
        ssr.sortingOrder = 1;

        return go;
    }

    static void MakeSpike(Transform parent, Vector3 pos)
    {
        var go = new GameObject("Spikes");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = S("spikes");
        sr.sortingOrder = 3;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.85f, 0.35f);
        col.offset = new Vector2(0f, -0.05f);
        go.AddComponent<Hazard>();
    }

    static void MakePickup(Transform parent, string name, Sprite sprite, Vector3 pos, Pickup.PickupType type, float radius)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 5;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;
        var pickup = go.AddComponent<Pickup>();
        pickup.type = type;
    }

    static GameObject MakePlayer(int groundLayerIndex, Vector3 spawn)
    {
        EnsureFolder(PhysicsDir);
        string matPath = PhysicsDir + "/NoFriction.asset";
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("NoFriction");
            mat.friction = 0f;
            mat.bounciness = 0f;
            AssetDatabase.CreateAsset(mat, matPath);
        }

        var go = new GameObject("Player");
        go.tag = "Player";
        go.transform.position = spawn;

        // Ko'rinish: Visual (oyoq nuqtasida, squash/stretch shu yerda) > Sprite
        var visual = new GameObject("Visual");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localPosition = new Vector3(0f, -0.65f, 0f);
        var spriteGo = new GameObject("Sprite");
        spriteGo.transform.SetParent(visual.transform, false);
        spriteGo.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        var sr = spriteGo.AddComponent<SpriteRenderer>();
        sr.sprite = SC("robot_idle");
        sr.sortingOrder = 10;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var cap = go.AddComponent<CapsuleCollider2D>();
        cap.direction = CapsuleDirection2D.Vertical;
        cap.size = new Vector2(0.8f, 1.2f);
        cap.offset = new Vector2(0f, -0.05f);
        cap.sharedMaterial = mat;

        var check = new GameObject("GroundCheck");
        check.transform.SetParent(go.transform, false);
        check.transform.localPosition = new Vector3(0f, -0.65f, 0f);

        var pc = go.AddComponent<PlayerController>();
        pc.groundCheck = check.transform;
        pc.groundLayer = 1 << groundLayerIndex;
        pc.killY = -9f;

        var anim = visual.AddComponent<PlayerAnimator>();
        anim.sr = sr;
        anim.controller = pc;
        anim.body = rb;
        anim.useSelectedCharacter = true;   // kadrlar o'yin vaqtida tanlangan personajdan olinadi

        return go;
    }

    // =====================================================================
    //  O'YIN INTERFEYSI (HUD, tugmalar, g'alaba va pauza oynalari)
    // =====================================================================

    static void BuildGameUI(GameManager gm)
    {
        var canvas = NewCanvas();
        NewEventSystem();
        Transform hud = canvas.transform;

        // ----- Yuqori panel -----
        var pauseBtn = NewButton(hud, "PauseButton", S("ui_round"), PanelColor, S("icon_pause"), LightText, 24f);
        Place(pauseBtn, TL, TL, new Vector2(16f, -14f), new Vector2(44f, 44f));
        UnityEventTools.AddPersistentListener(pauseBtn.onClick, gm.TogglePause);

        var labelPanel = NewImage(hud, "LevelPanel", S("ui_round"), PanelColor, false);
        Place(labelPanel, TL, TL, new Vector2(68f, -14f), new Vector2(120f, 44f));
        var levelLabel = NewText(labelPanel.transform, "Text", "Zavod · 1", 17, LightText, TextAnchor.MiddleCenter);
        Stretch(levelLabel);

        var timerPanel = NewImage(hud, "TimerPanel", S("ui_round"), PanelColor, false);
        Place(timerPanel, TC, TC, new Vector2(0f, -14f), new Vector2(100f, 44f));
        var timerText = NewText(timerPanel.transform, "Text", "0.0 s", 20, LightText, TextAnchor.MiddleCenter);
        Stretch(timerText);

        var coinPanel = NewImage(hud, "CoinPanel", S("ui_round"), PanelColor, false);
        Place(coinPanel, TR, TR, new Vector2(-72f, -14f), new Vector2(104f, 44f));
        var coinIcon = NewImage(coinPanel.transform, "Icon", S("coin"), Color.white, false);
        coinIcon.preserveAspect = true;
        Place(coinIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(12f, 0f), new Vector2(24f, 24f));
        var coinText = NewText(coinPanel.transform, "Text", "0 / 0", 20, LightText, TextAnchor.MiddleRight);
        Place(coinText, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(60f, 44f));

        var keyPanel = NewImage(hud, "KeyPanel", S("ui_round"), PanelColor, false);
        Place(keyPanel, TR, TR, new Vector2(-16f, -14f), new Vector2(44f, 44f));
        var keyIcon = NewImage(keyPanel.transform, "Icon", S("key"), Color.white, false);
        keyIcon.preserveAspect = true;
        Place(keyIcon, Mid, Mid, Vector2.zero, new Vector2(26f, 26f));

        // ----- Sensorli tugmalar -----
        var left = NewTouchButton(hud, "LeftButton", S("ui_round"), ButtonDark, S("icon_chevron"), LightText, 40f, TouchButton.Kind.Left);
        Place(left, BL, BL, new Vector2(20f, 20f), new Vector2(64f, 64f));
        left.transform.Find("Icon").localScale = new Vector3(-1f, 1f, 1f);
        var right = NewTouchButton(hud, "RightButton", S("ui_round"), ButtonDark, S("icon_chevron"), LightText, 40f, TouchButton.Kind.Right);
        Place(right, BL, BL, new Vector2(96f, 20f), new Vector2(64f, 64f));
        var jump = NewTouchButton(hud, "JumpButton", S("ui_circle"), OrangeColor, S("icon_up"), DarkText, 44f, TouchButton.Kind.Jump);
        Place(jump, BR, BR, new Vector2(-16f, 16f), new Vector2(84f, 84f));
        jump.gameObject.AddComponent<CharacterAccent>();

        // ----- G'alaba oynasi -----
        var winRoot = NewImage(hud, "WinOverlay", null, new Color(0f, 0f, 0f, 0.5f), true);
        Stretch(winRoot);

        var happy = NewImage(winRoot.transform, "CharacterHappy", SC("robot_happy"), Color.white, false);
        happy.preserveAspect = true;
        Place(happy, Mid, Mid, new Vector2(-330f, -20f), new Vector2(100f, 118f));

        var panel = NewImage(winRoot.transform, "Panel", S("ui_round"), DialogColor, true);
        Place(panel, Mid, Mid, Vector2.zero, new Vector2(488f, 322f));

        var worldTxt = NewText(panel.transform, "World", "Daraja tugadi", 15, MutedText, TextAnchor.MiddleCenter);
        PlaceTop(worldTxt, 0f, -22f, 300f, 24f);
        var title = NewText(panel.transform, "Title", "Daraja tugadi!", 34, LightText, TextAnchor.MiddleCenter);
        PlaceTop(title, 0f, -56f, 400f, 44f);

        float[] starSize = { 52f, 68f, 52f };
        float[] starX = { -64f, 0f, 64f };
        float[] starY = { -118f, -114f, -118f };
        var starImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            starImages[i] = NewImage(panel.transform, "Star" + (i + 1), S("icon_star"), new Color(1f, 1f, 1f, 0.18f), false);
            PlaceTop(starImages[i], starX[i], starY[i], starSize[i], starSize[i]);
        }

        var info = NewText(panel.transform, "Info", "Tangalar: 0/0\nVaqt: 0.0 s", 18, LightText, TextAnchor.MiddleCenter);
        PlaceTop(info, 0f, -178f, 300f, 52f);

        var homeBtn = NewButton(panel.transform, "HomeButton", S("ui_round"), ButtonDark, S("icon_home"), LightText, 24f);
        PlaceTop(homeBtn, -120f, -234f, 56f, 56f);
        UnityEventTools.AddPersistentListener(homeBtn.onClick, gm.GoToMenu);

        var restartBtn = NewButton(panel.transform, "RestartButton", S("ui_round"), ButtonDark, S("icon_restart"), LightText, 24f);
        PlaceTop(restartBtn, -52f, -234f, 56f, 56f);
        UnityEventTools.AddPersistentListener(restartBtn.onClick, gm.RestartLevel);

        var nextBtn = NewButton(panel.transform, "NextButton", S("ui_round"), OrangeColor, null, Color.white, 0f);
        PlaceTop(nextBtn, 68f, -234f, 160f, 56f);
        nextBtn.gameObject.AddComponent<CharacterAccent>();
        var nextLabel = NewText(nextBtn.transform, "Text", "Keyingi", 22, DarkText, TextAnchor.MiddleCenter);
        Place(nextLabel, Mid, Mid, new Vector2(-12f, 0f), new Vector2(100f, 56f));
        var nextIcon = NewImage(nextBtn.transform, "Icon", S("icon_chevron"), DarkText, false);
        Place(nextIcon, Mid, Mid, new Vector2(50f, 0f), new Vector2(22f, 22f));
        UnityEventTools.AddPersistentListener(nextBtn.onClick, gm.NextLevel);

        var rewardBtn = NewButton(panel.transform, "RewardButton", S("ui_round"), new Color(TealColor.r, TealColor.g, TealColor.b, 0.18f), null, Color.white, 0f);
        PlaceTop(rewardBtn, 0f, -290f, 330f, 44f);
        var rewardIcon = NewImage(rewardBtn.transform, "Icon", S("icon_play"), TealColor, false);
        Place(rewardIcon, Mid, Mid, new Vector2(-148f, 0f), new Vector2(16f, 16f));
        var rewardText = NewText(rewardBtn.transform, "Text", "Tangalarni 2 barobar qilish (reklama)", 16, TealColor, TextAnchor.MiddleCenter);
        Place(rewardText, Mid, Mid, new Vector2(8f, 0f), new Vector2(290f, 44f));
        UnityEventTools.AddPersistentListener(rewardBtn.onClick, gm.OnRewardedDoubleCoins);

        // ----- Pauza oynasi -----
        var pauseRoot = NewImage(hud, "PauseOverlay", null, new Color(0f, 0f, 0f, 0.55f), true);
        Stretch(pauseRoot);
        var pausePanel = NewImage(pauseRoot.transform, "Panel", S("ui_round"), DialogColor, true);
        Place(pausePanel, Mid, Mid, Vector2.zero, new Vector2(300f, 220f));
        var pauseTitle = NewText(pausePanel.transform, "Title", "Pauza", 30, LightText, TextAnchor.MiddleCenter);
        PlaceTop(pauseTitle, 0f, -38f, 240f, 40f);

        var resumeBtn = NewButton(pausePanel.transform, "ResumeButton", S("ui_round"), OrangeColor, null, Color.white, 0f);
        PlaceTop(resumeBtn, 0f, -104f, 220f, 52f);
        var resumeText = NewText(resumeBtn.transform, "Text", "Davom etish", 22, DarkText, TextAnchor.MiddleCenter);
        Stretch(resumeText);
        UnityEventTools.AddPersistentListener(resumeBtn.onClick, gm.TogglePause);

        var again = NewButton(pausePanel.transform, "RestartButton", S("ui_round"), ButtonDark, null, Color.white, 0f);
        PlaceTop(again, 0f, -166f, 220f, 52f);
        var againText = NewText(again.transform, "Text", "Qayta boshlash", 20, LightText, TextAnchor.MiddleCenter);
        Stretch(againText);
        UnityEventTools.AddPersistentListener(again.onClick, gm.RestartLevel);

        // ----- GameManager'ga ulash -----
        gm.levelLabel = levelLabel;
        gm.coinText = coinText;
        gm.timerText = timerText;
        gm.keyImage = keyIcon;
        gm.winPanel = winRoot.gameObject;
        gm.winStars = starImages;
        gm.winInfoText = info;
        gm.pausePanel = pauseRoot.gameObject;
        gm.winCharacterImage = happy;

        winRoot.gameObject.SetActive(false);
        pauseRoot.gameObject.SetActive(false);
    }

    // =====================================================================
    //  MENU SAHNASI (daraja tanlash)
    // =====================================================================

    static void BuildMenuScene(int levelCount)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgColor;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        var menuGo = new GameObject("_Menu");
        var menu = menuGo.AddComponent<LevelSelectMenu>();

        var canvas = NewCanvas();
        NewEventSystem();
        Transform ui = canvas.transform;

        var title = NewText(ui, "Title", "Zavod", 32, LightText, TextAnchor.MiddleLeft);
        Place(title, TL, TL, new Vector2(48f, -20f), new Vector2(240f, 44f));

        // Dunyo sahifalarini almashtirish tugmalari
        var prev = NewButton(ui, "PrevWorld", S("ui_round"), PanelColor, S("icon_chevron"), LightText, 22f);
        Place(prev, TC, TC, new Vector2(-40f, -20f), new Vector2(44f, 44f));
        prev.transform.Find("Icon").localScale = new Vector3(-1f, 1f, 1f);
        UnityEventTools.AddPersistentListener(prev.onClick, menu.PrevPage);
        var next = NewButton(ui, "NextWorld", S("ui_round"), PanelColor, S("icon_chevron"), LightText, 22f);
        Place(next, TC, TC, new Vector2(40f, -20f), new Vector2(44f, 44f));
        UnityEventTools.AddPersistentListener(next.onClick, menu.NextPage);

        // Personaj tanlash ekraniga qaytish tugmasi
        var charBtn = NewButton(ui, "CharacterButton", S("ui_round"), PanelColor, null, Color.white, 0f);
        Place(charBtn, TR, TR, new Vector2(-184f, -20f), new Vector2(44f, 44f));
        var charIcon = NewImage(charBtn.transform, "Portrait", SC("robot_idle"), Color.white, false);
        charIcon.preserveAspect = true;
        Place(charIcon, Mid, Mid, Vector2.zero, new Vector2(36f, 36f));
        charIcon.gameObject.AddComponent<SelectedCharacterIcon>();
        UnityEventTools.AddPersistentListener(charBtn.onClick, menu.OpenCharacterSelect);

        var starsPanel = NewImage(ui, "StarsPanel", S("ui_round"), PanelColor, false);
        Place(starsPanel, TR, TR, new Vector2(-24f, -20f), new Vector2(152f, 44f));
        var starsIcon = NewImage(starsPanel.transform, "Icon", S("icon_star"), YellowColor, false);
        Place(starsIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(24f, 24f));
        var starsText = NewText(starsPanel.transform, "Text", "0 / 0", 20, LightText, TextAnchor.MiddleRight);
        Place(starsText, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(80f, 44f));

        var grid = NewRect("Grid", ui);
        Place(grid, TL, TL, new Vector2(48f, -92f), new Vector2(748f, 262f));
        var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(136f, 123f);
        layout.spacing = new Vector2(16f, 16f);
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 5;

        var tiles = new LevelTile[levelCount];
        for (int i = 0; i < levelCount; i++) tiles[i] = NewLevelTile(grid, i + 1);

        // ----- Qulf bannerlari (qulflangan dunyo sahifasida) -----
        var banner = NewButton(ui, "LockBanner", S("ui_round"), OrangeColor, null, Color.white, 0f);
        Place(banner, BC, BC, new Vector2(0f, 14f), new Vector2(420f, 44f));
        var bannerText = NewText(banner.transform, "Text", "Qulf", 18, DarkText, TextAnchor.MiddleCenter);
        Stretch(bannerText);
        UnityEventTools.AddPersistentListener(banner.onClick, menu.OpenGateForPage);

        // ----- Yulduz darvozasi oynasi -----
        var gateRoot = NewImage(ui, "GateOverlay", null, new Color(0f, 0f, 0f, 0.6f), true);
        Stretch(gateRoot);
        var gatePanel = NewImage(gateRoot.transform, "Panel", S("ui_round"), DialogColor, true);
        Place(gatePanel, Mid, Mid, Vector2.zero, new Vector2(460f, 320f));
        var gateStar = NewImage(gatePanel.transform, "Icon", S("icon_star"), YellowColor, false);
        PlaceTop(gateStar, 0f, -34f, 36f, 36f);
        var gateTitle = NewText(gatePanel.transform, "Title", "Dunyo qulflangan", 26, LightText, TextAnchor.MiddleCenter);
        PlaceTop(gateTitle, 0f, -74f, 420f, 36f);
        var gateInfo = NewText(gatePanel.transform, "Info", "", 17, MutedText, TextAnchor.MiddleCenter);
        PlaceTop(gateInfo, 0f, -124f, 420f, 70f);

        var adBtn = NewButton(gatePanel.transform, "AdButton", S("ui_round"), OrangeColor, null, Color.white, 0f);
        PlaceTop(adBtn, 0f, -190f, 340f, 48f);
        var adText = NewText(adBtn.transform, "Text", "Reklama ko'r", 20, DarkText, TextAnchor.MiddleCenter);
        Stretch(adText);
        UnityEventTools.AddPersistentListener(adBtn.onClick, menu.GateWatchAd);

        var buyBtn = NewButton(gatePanel.transform, "BuyButton", S("ui_round"), ButtonDark, null, Color.white, 0f);
        PlaceTop(buyBtn, 0f, -246f, 340f, 48f);
        var buyText = NewText(buyBtn.transform, "Text", "Darhol ochish", 20, LightText, TextAnchor.MiddleCenter);
        Stretch(buyText);
        UnityEventTools.AddPersistentListener(buyBtn.onClick, menu.GateBuy);

        var closeBtn = NewButton(gatePanel.transform, "CloseButton", S("ui_round"), ButtonDark, null, Color.white, 0f);
        Place(closeBtn, TR, TR, new Vector2(-12f, -12f), new Vector2(40f, 40f));
        var closeText = NewText(closeBtn.transform, "Text", "X", 20, LightText, TextAnchor.MiddleCenter);
        Stretch(closeText);
        UnityEventTools.AddPersistentListener(closeBtn.onClick, menu.CloseGate);

        var toast = NewText(ui, "Toast", "", 18, LightText, TextAnchor.MiddleCenter);
        Place(toast, BC, BC, new Vector2(0f, 70f), new Vector2(520f, 32f));
        toast.gameObject.SetActive(false);

        menu.gatePanel = gateRoot.gameObject;
        menu.gateTitle = gateTitle;
        menu.gateInfo = gateInfo;
        menu.adButton = adBtn;
        menu.adButtonText = adText;
        menu.buyButton = buyBtn;
        menu.buyButtonText = buyText;
        menu.lockBanner = banner;
        menu.lockBannerText = bannerText;
        menu.toastText = toast;

        menu.tiles = tiles;
        menu.totalStarsText = starsText;
        menu.titleText = title;
        menu.prevButton = prev;
        menu.nextButton = next;
        menu.tilesPerPage = 10;
        menu.worldNames = new[] { "Zavod", "O'rmon", "Cho'l", "Muz", "Kosmos", "Vulqon", "Okean", "Bulutlar", "Shahar", "Sehrli" };

        EditorSceneManager.SaveScene(scene, MenuScenePath);
    }

    // =====================================================================
    //  PERSONAJ TANLASH SAHNASI (o'yin shundan boshlanadi)
    // =====================================================================

    static void BuildCharacterSelectScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BgColor;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        var menuGo = new GameObject("_CharacterSelect");
        var menu = menuGo.AddComponent<CharacterSelectMenu>();

        var canvas = NewCanvas();
        NewEventSystem();
        Transform ui = canvas.transform;

        var title = NewText(ui, "Title", GameInfo.Title, 44, LightText, TextAnchor.MiddleCenter);
        PlaceTop(title, 0f, -36f, 600f, 56f);
        var subtitle = NewText(ui, "Subtitle", GameInfo.Subtitle, 18, MutedText, TextAnchor.MiddleCenter);
        PlaceTop(subtitle, 0f, -80f, 500f, 26f);

        int n = CharacterCatalog.All.Length;
        var cards = new CharacterCardUI[n];
        for (int i = 0; i < n; i++)
        {
            var character = CharacterCatalog.All[i];
            float x = (i - (n - 1) * 0.5f) * 180f;

            // Tanlanganda ko'rinadigan ramka (kartaning ostida)
            var highlight = NewImage(ui, "Highlight" + (i + 1), S("ui_round"), character.accent, false);
            Place(highlight, Mid, Mid, new Vector2(x, -8f), new Vector2(172f, 222f));

            var card = NewImage(ui, "Card" + (i + 1), S("ui_round"), DialogColor, true);
            Place(card, Mid, Mid, new Vector2(x, -8f), new Vector2(160f, 210f));
            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = card;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            btn.colors = colors;

            var portrait = NewImage(card.transform, "Portrait", SC(character.id + "_idle"), Color.white, false);
            portrait.preserveAspect = true;
            Place(portrait, Mid, Mid, new Vector2(0f, 18f), new Vector2(110f, 130f));

            var nameText = NewText(card.transform, "Name", character.displayName, 22, LightText, TextAnchor.MiddleCenter);
            Place(nameText, Mid, Mid, new Vector2(0f, -84f), new Vector2(150f, 30f));

            cards[i] = new CharacterCardUI { button = btn, portrait = portrait, highlight = highlight, nameText = nameText };
        }

        var play = NewButton(ui, "PlayButton", S("ui_round"), OrangeColor, null, Color.white, 0f);
        Place(play, BC, BC, new Vector2(0f, 14f), new Vector2(220f, 52f));
        var playText = NewText(play.transform, "Text", "O'ynash", 24, DarkText, TextAnchor.MiddleCenter);
        Place(playText, Mid, Mid, new Vector2(-10f, 0f), new Vector2(140f, 52f));
        var playIcon = NewImage(play.transform, "Icon", S("icon_play"), DarkText, false);
        Place(playIcon, Mid, Mid, new Vector2(62f, 0f), new Vector2(22f, 22f));
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);

        menu.cards = cards;
        menu.titleText = title;
        menu.subtitleText = subtitle;
        menu.nextSceneName = "Menu";

        EditorSceneManager.SaveScene(scene, CharacterSelectScenePath);
    }

    static LevelTile NewLevelTile(Transform parent, int level)
    {
        var bg = NewImage(parent, "Level" + level, S("ui_round"), BodyColor, true);
        var btn = bg.gameObject.AddComponent<Button>();
        btn.targetGraphic = bg;
        var colors = btn.colors;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.disabledColor = Color.white;
        btn.colors = colors;

        var number = NewText(bg.transform, "Number", level.ToString(), 44, LightText, TextAnchor.MiddleCenter);
        Place(number, Mid, Mid, new Vector2(0f, 8f), new Vector2(120f, 56f));

        var stars = new Image[3];
        float[] xs = { -28f, 0f, 28f };
        for (int i = 0; i < 3; i++)
        {
            stars[i] = NewImage(bg.transform, "Star" + (i + 1), S("icon_star"), new Color(1f, 1f, 1f, 0.18f), false);
            Place(stars[i], Mid, Mid, new Vector2(xs[i], -36f), new Vector2(24f, 24f));
        }

        var lockImg = NewImage(bg.transform, "Lock", S("icon_lock"), new Color(0.561f, 0.627f, 0.784f, 1f), false);
        Place(lockImg, Mid, Mid, new Vector2(0f, 24f), new Vector2(30f, 30f));
        lockImg.gameObject.SetActive(false);

        var tile = bg.gameObject.AddComponent<LevelTile>();
        tile.level = level;
        tile.button = btn;
        tile.background = bg;
        tile.numberText = number;
        tile.stars = stars;
        tile.lockIcon = lockImg.gameObject;
        return tile;
    }

    // =====================================================================
    //  UI YORDAMCHILARI
    // =====================================================================

    static Canvas NewCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform));
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(844f, 390f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void NewEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    static Font UIFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static Image NewImage(Transform parent, string name, Sprite sprite, Color color, bool raycast)
    {
        var rect = NewRect(name, parent);
        var img = rect.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = raycast;
        if (sprite != null && sprite.border != Vector4.zero) img.type = Image.Type.Sliced;
        return img;
    }

    static Text NewText(Transform parent, string name, string content, int size, Color color, TextAnchor align)
    {
        var rect = NewRect(name, parent);
        var t = rect.gameObject.AddComponent<Text>();
        t.font = UIFont();
        t.text = content;
        t.fontSize = size;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Button NewButton(Transform parent, string name, Sprite bg, Color color, Sprite icon, Color iconColor, float iconSize)
    {
        var img = NewImage(parent, name, bg, color, true);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;
        if (icon != null)
        {
            var ic = NewImage(img.transform, "Icon", icon, iconColor, false);
            Place(ic, Mid, Mid, Vector2.zero, new Vector2(iconSize, iconSize));
        }
        return btn;
    }

    static Image NewTouchButton(Transform parent, string name, Sprite bg, Color color, Sprite icon, Color iconColor, float iconSize, TouchButton.Kind kind)
    {
        var img = NewImage(parent, name, bg, color, true);
        var touch = img.gameObject.AddComponent<TouchButton>();
        touch.kind = kind;
        var ic = NewImage(img.transform, "Icon", icon, iconColor, false);
        Place(ic, Mid, Mid, Vector2.zero, new Vector2(iconSize, iconSize));
        return img;
    }

    static void Place(Component c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var r = (RectTransform)c.transform;
        r.anchorMin = anchor;
        r.anchorMax = anchor;
        r.pivot = pivot;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    /// <summary>Yuqori-markazga langarlangan, markazi (x, y) bo'lgan element (y manfiy = pastga).</summary>
    static void PlaceTop(Component c, float x, float y, float w, float h)
    {
        Place(c, TC, Mid, new Vector2(x, y), new Vector2(w, h));
    }

    static void Stretch(Component c)
    {
        var r = (RectTransform)c.transform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    static Color Hex(string hex)
    {
        Color c;
        if (ColorUtility.TryParseHtmlString("#" + hex, out c)) return c;
        return Color.magenta;
    }
}

/// <summary>Paket import qilingandan keyin bir marta so'raydi: demo sahnalarni yaratamizmi?</summary>
[InitializeOnLoad]
static class SakrovchilarFirstRun
{
    static SakrovchilarFirstRun()
    {
        EditorApplication.delayCall += Ask;
    }

    static void Ask()
    {
        if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("Sakrovchilar_Asked", false)) return;
        SessionState.SetBool("Sakrovchilar_Asked", true);

        if (File.Exists(SakrovchilarBuilder.Level1ScenePath)) return;
        if (!File.Exists("Assets/Sakrovchilar/Sprites/coin.png")) return;

        if (EditorUtility.DisplayDialog("Sakrovchilar",
                "Paket o'rnatildi. Demo sahnalarni (Menu + Level1) hozir yaratamizmi?\n\n" +
                "Keyinroq: yuqori menyu > Sakrovchilar > 1. Hammasini yaratish",
                "Yaratish", "Keyinroq"))
        {
            SakrovchilarBuilder.BuildAll();
        }
    }
}
