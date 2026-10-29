using UnityEngine;

/// <summary>
/// Dushman: ikki nuqta orasida yurib turadi. Yon tomondan tegsa o'yinchi boshiga qaytadi,
/// ustidan sakrab bossa dushman yo'q bo'ladi va o'yinchi yuqoriga sakraydi.
/// Collider2D (Is Trigger) va "Player" tegi bor o'yinchi bilan ishlaydi.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public float minX;
    public float maxX;
    public float speed = 1.5f;
    public float bounceForce = 10f;
    public SpriteRenderer sr;
    public Sprite frameA;
    public Sprite frameB;
    public Sprite deadFrame;
    public float stepTime = 0.22f;

    int dir = 1;
    float timer;
    bool dead;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        // Tasodifiy boshlang'ich yo'nalish: har bir dushman bir xil emas
        dir = ((int)(transform.position.x * 10f) % 2 == 0) ? 1 : -1;
    }

    void Update()
    {
        if (dead) return;

        Vector3 p = transform.position;
        p.x += dir * speed * Time.deltaTime;
        if (p.x >= maxX) { p.x = maxX; dir = -1; }
        else if (p.x <= minX) { p.x = minX; dir = 1; }
        transform.position = p;

        if (sr != null)
        {
            sr.flipX = dir < 0;
            timer += Time.deltaTime;
            bool a = ((int)(timer / stepTime)) % 2 == 0;
            Sprite f = a ? frameA : frameB;
            if (f != null) sr.sprite = f;
        }
    }

    void OnTriggerEnter2D(Collider2D other) { Touch(other); }
    void OnTriggerStay2D(Collider2D other) { Touch(other); }

    void Touch(Collider2D other)
    {
        if (dead || !other.CompareTag("Player")) return;
        var player = other.GetComponent<PlayerController>();
        if (player == null) player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        // O'yinchi oyoqlari dushmanning yuqori qismida va pastga tushayotgan bo'lsa: bosib o'ldiradi
        bool above = other.bounds.min.y > col.bounds.center.y;
        if (above && player.VerticalSpeed <= 1f)
        {
            Kill();
            player.Bounce(bounceForce);
        }
        else
        {
            player.Die();
        }
    }

    void Kill()
    {
        dead = true;
        col.enabled = false;
        if (sr != null && deadFrame != null) sr.sprite = deadFrame;
        Destroy(gameObject, 0.35f);
    }
}
