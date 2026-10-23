using UnityEngine;

/// <summary>
/// Robotning harakati: yurish, sakrash (coyote time + jump buffer + o'zgaruvchan balandlik).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Harakat")]
    public float moveSpeed = 6f;
    public float jumpForce = 13f;
    [Tooltip("Yerdan tushgandan keyin ham sakrash mumkin bo'lgan vaqt")]
    public float coyoteTime = 0.1f;
    [Tooltip("Yerga tegishdan sal oldin bosilgan sakrashni eslab qolish")]
    public float jumpBufferTime = 0.1f;

    [Header("Yerni aniqlash")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("O'lim")]
    [Tooltip("Shu balandlikdan pastga tushsa o'ladi")]
    public float killY = -9f;

    /// <summary>Robot yerda turibdimi (animatsiya uchun).</summary>
    public bool IsGrounded { get; private set; }
    /// <summary>Robot o'lganda (tuzoq yoki chuqur) chaqiriladi.</summary>
    public event System.Action Died;
    /// <summary>Daraja tugaganda chaqiriladi.</summary>
    public event System.Action Finished;

    Rigidbody2D rb;
    Vector3 spawnPoint;
    float coyoteCounter;
    float bufferCounter;
    bool frozen;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPoint = transform.position;
    }

    void Update()
    {
        if (frozen) return;

        bool grounded = groundCheck != null &&
                        Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer) != null;
        IsGrounded = grounded;

        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.deltaTime;
        bufferCounter = MobileInput.JumpPressed ? jumpBufferTime : bufferCounter - Time.deltaTime;

        // Sakrash
        if (bufferCounter > 0f && coyoteCounter > 0f)
        {
            Vector2 v = rb.GetVelocity();
            rb.SetVelocity(new Vector2(v.x, jumpForce));
            bufferCounter = 0f;
            coyoteCounter = 0f;
        }

        // Tugmani erta qo'yib yuborsa, sakrash pastroq bo'ladi
        if (MobileInput.JumpReleased)
        {
            Vector2 v = rb.GetVelocity();
            if (v.y > 0f) rb.SetVelocity(new Vector2(v.x, v.y * 0.5f));
        }

        // Tomonga qarash
        float h = MobileInput.Horizontal;
        Vector3 s = transform.localScale;
        if (h > 0.01f) s.x = Mathf.Abs(s.x);
        else if (h < -0.01f) s.x = -Mathf.Abs(s.x);
        transform.localScale = s;

        // Chuqurga tushib ketdi
        if (transform.position.y < killY) Die();
    }

    void FixedUpdate()
    {
        if (frozen) return;
        Vector2 v = rb.GetVelocity();
        rb.SetVelocity(new Vector2(MobileInput.Horizontal * moveSpeed, v.y));
    }

    /// <summary>Tuzoqqa tushganda yoki chuqurga tushganda chaqiriladi.</summary>
    public void Die()
    {
        if (frozen) return;
        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
        if (Died != null) Died();
        Respawn();
    }

    public void Respawn()
    {
        transform.SetParent(null);
        transform.position = spawnPoint;
        rb.SetVelocity(Vector2.zero);
        coyoteCounter = 0f;
        bufferCounter = 0f;
    }

    /// <summary>Daraja tugaganda robotni to'xtatish.</summary>
    public void Freeze()
    {
        frozen = true;
        if (Finished != null) Finished();
        rb.SetVelocity(Vector2.zero);
        rb.simulated = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
