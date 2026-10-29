using UnityEngine;

/// <summary>
/// O'q otadigan dushman (to'p): joyida turadi, o'yinchi yaqinlashganda unga qarab o'q uzadi.
/// O'qdan sakrab o'tish mumkin. Ustidan sakrab bossangiz, to'p yo'q bo'ladi.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShooterEnemy : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite idleFrame;
    public Sprite fireFrame;
    public Sprite deadFrame;
    public Sprite bulletSprite;
    [Tooltip("Shu masofadan yaqin bo'lsa o'q uzadi")]
    public float range = 11f;
    public float interval = 2.2f;
    public float bulletSpeed = 5f;
    public float bounceForce = 10f;

    float timer;
    float flashLeft;
    bool dead;
    Collider2D col;
    PlayerController player;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        // Hammasi bir vaqtda otmasligi uchun boshlang'ich vaqt har xil
        timer = interval * Mathf.Repeat(transform.position.x * 0.37f, 1f);
    }

    void Start()
    {
        player = UnityCompat.FindFirst<PlayerController>();
    }

    void Update()
    {
        if (dead) return;
        if (player == null) player = UnityCompat.FindFirst<PlayerController>();
        if (player == null) return;

        Vector2 d = player.transform.position - transform.position;
        bool left = d.x < 0f;
        if (sr != null) sr.flipX = !left;   // sprite chapga qaragan chizilgan

        if (flashLeft > 0f)
        {
            flashLeft -= Time.deltaTime;
            if (flashLeft <= 0f && sr != null && idleFrame != null) sr.sprite = idleFrame;
        }

        if (Mathf.Abs(d.x) > range || Mathf.Abs(d.y) > 5f) { timer = Mathf.Min(timer, interval * 0.5f); return; }

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            Fire(left ? -1 : 1);
        }
    }

    void Fire(int dir)
    {
        var go = new GameObject("Bullet");
        go.transform.position = transform.position + new Vector3(dir * 0.55f, 0.08f, 0f);
        var bsr = go.AddComponent<SpriteRenderer>();
        bsr.sprite = bulletSprite;
        bsr.sortingOrder = 8;
        var c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.14f;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var p = go.AddComponent<Projectile>();
        p.velocity = new Vector2(dir * bulletSpeed, 0f);

        if (sr != null && fireFrame != null)
        {
            sr.sprite = fireFrame;
            flashLeft = 0.18f;
        }
    }

    void OnTriggerEnter2D(Collider2D other) { Touch(other); }
    void OnTriggerStay2D(Collider2D other) { Touch(other); }

    void Touch(Collider2D other)
    {
        if (dead || !other.CompareTag("Player")) return;
        var pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;

        bool above = other.bounds.min.y > col.bounds.center.y;
        if (above && pc.VerticalSpeed <= 1f)
        {
            dead = true;
            col.enabled = false;
            if (sr != null && deadFrame != null) sr.sprite = deadFrame;
            pc.Bounce(bounceForce);
            Destroy(gameObject, 0.35f);
        }
        else
        {
            pc.Die();
        }
    }
}
