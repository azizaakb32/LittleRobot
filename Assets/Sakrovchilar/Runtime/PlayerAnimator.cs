using UnityEngine;

/// <summary>
/// Robot animatsiyasi (kod orqali, Animator kerak emas):
/// kadr almashtirish (turish, ko'z qisish, yugurish, sakrash, tushish, xursandchilik),
/// squash &amp; stretch va o'lganda miltillash.
///
/// Tuzilma: Robot (Rigidbody2D, PlayerController) > Visual (shu skript, oyoq nuqtasida) > Sprite (SpriteRenderer).
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Ulanishlar")]
    public SpriteRenderer sr;
    public PlayerController controller;
    public Rigidbody2D body;

    [Header("Kadrlar")]
    public Sprite idle;
    public Sprite blink;
    public Sprite jump;
    public Sprite fall;
    public Sprite happy;
    public Sprite[] run;

    [Header("Sozlamalar")]
    [Tooltip("Yoqilgan bo'lsa, kadrlar tanlangan personajdan (Resources/Characters) olinadi")]
    public bool useSelectedCharacter = true;
    public float runFps = 12f;
    public float blinkEvery = 3f;
    public float blinkDuration = 0.12f;
    public float squashSpeed = 14f;

    float runTime;
    float blinkTimer;
    float blinkLeft;
    float landTimer;
    float airSpeed;
    float flashLeft;
    bool wasGrounded = true;
    bool celebrating;
    Vector3 scaleNow = Vector3.one;

    void Awake()
    {
        if (controller == null) controller = GetComponentInParent<PlayerController>();
        if (body == null) body = GetComponentInParent<Rigidbody2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        blinkTimer = blinkEvery;
        if (useSelectedCharacter) ApplyCharacter(CharacterCatalog.Selected);
        if (sr != null && idle != null) sr.sprite = idle;
    }

    /// <summary>Barcha kadrlarni berilgan personajdan yuklaydi (topilmaganlari o'zgarmaydi).</summary>
    public void ApplyCharacter(CharacterDef character)
    {
        idle = Pick(character, "idle", idle);
        blink = Pick(character, "blink", blink);
        jump = Pick(character, "jump", jump);
        fall = Pick(character, "fall", fall);
        happy = Pick(character, "happy", happy);

        var frames = new Sprite[CharacterCatalog.RunFrameCount];
        bool ok = true;
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = CharacterCatalog.Load(character, CharacterCatalog.RunFramePrefix + i);
            if (frames[i] == null) ok = false;
        }
        if (ok) run = frames;
    }

    static Sprite Pick(CharacterDef character, string frame, Sprite fallback)
    {
        var s = CharacterCatalog.Load(character, frame);
        return s != null ? s : fallback;
    }

    void OnEnable()
    {
        if (controller == null) return;
        controller.Died += OnDied;
        controller.Finished += OnFinished;
    }

    void OnDisable()
    {
        if (controller == null) return;
        controller.Died -= OnDied;
        controller.Finished -= OnFinished;
    }

    void OnDied()
    {
        flashLeft = 0.6f;
        landTimer = 0f;
        scaleNow = Vector3.one;
    }

    void OnFinished()
    {
        celebrating = true;
        if (sr != null && happy != null) sr.sprite = happy;
        scaleNow = Vector3.one;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (sr == null || controller == null || body == null) return;
        if (celebrating)
        {
            // Sakrab-sakrab xursand bo'lish
            float hop = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 6f));
            transform.localScale = new Vector3(1f - 0.05f * hop, 1f + 0.12f * hop, 1f);
            return;
        }

        Vector2 v = body.GetVelocity();
        bool grounded = controller.IsGrounded;

        // Tushish tezligini eslab qolamiz (yerga urilganda squash kuchi uchun)
        if (!grounded) airSpeed = Mathf.Min(airSpeed, v.y);
        if (grounded && !wasGrounded)
        {
            if (airSpeed < -5f) landTimer = 0.14f;
            airSpeed = 0f;
        }
        wasGrounded = grounded;

        Sprite frame;
        Vector3 target = Vector3.one;

        if (!grounded)
        {
            runTime = 0f;
            if (v.y > 0.1f)
            {
                frame = jump;
                target = new Vector3(0.9f, 1.12f, 1f);
            }
            else
            {
                frame = fall;
                target = new Vector3(0.94f, 1.07f, 1f);
            }
        }
        else if (Mathf.Abs(v.x) > 0.5f && Mathf.Abs(MobileInput.Horizontal) > 0.1f && run != null && run.Length > 0)
        {
            runTime += Time.deltaTime * runFps * Mathf.Clamp(Mathf.Abs(v.x) / 6f, 0.6f, 1.2f);
            frame = run[(int)runTime % run.Length];
            // yengil tebranish
            target = new Vector3(1f, 1f + 0.03f * Mathf.Sin(runTime * Mathf.PI), 1f);
        }
        else
        {
            runTime = 0f;
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                blinkLeft = blinkDuration;
                blinkTimer = blinkEvery + Random.Range(0f, 2f);
            }
            if (blinkLeft > 0f) blinkLeft -= Time.deltaTime;
            frame = (blinkLeft > 0f && blink != null) ? blink : idle;
            // "nafas olish"
            target = new Vector3(1f, 1f + 0.02f * Mathf.Sin(Time.time * 3f), 1f);
        }

        if (landTimer > 0f)
        {
            landTimer -= Time.deltaTime;
            target = new Vector3(1.18f, 0.82f, 1f);
        }

        if (frame != null) sr.sprite = frame;

        scaleNow = Vector3.Lerp(scaleNow, target, 1f - Mathf.Exp(-squashSpeed * Time.deltaTime));
        transform.localScale = scaleNow;

        // O'lgandan keyingi miltillash
        if (flashLeft > 0f)
        {
            flashLeft -= Time.deltaTime;
            bool on = ((int)(flashLeft / 0.06f)) % 2 == 0;
            Color c = sr.color;
            c.a = on ? 1f : 0.25f;
            sr.color = c;
            if (flashLeft <= 0f)
            {
                c.a = 1f;
                sr.color = c;
            }
        }
    }
}
