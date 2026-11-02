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

    [Header("Narvon")]
    public float climbSpeed = 3.6f;

    [Header("Devordan sakrash")]
    public float wallSlideSpeed = 2.2f;
    public Vector2 wallJumpForce = new Vector2(7.5f, 12.5f);
    public float wallJumpLock = 0.18f;
    public float wallCheckDistance = 0.12f;

    [Header("Platformadan tushish")]
    public float dropThroughTime = 0.35f;

    [Header("O'lim")]
    [Tooltip("Shu balandlikdan pastga tushsa o'ladi")]
    public float killY = -9f;

    /// <summary>Robot yerda turibdimi (animatsiya uchun).</summary>
    public bool IsGrounded { get; private set; }
    /// <summary>Narvonda turibdi (animatsiya uchun).</summary>
    public bool IsClimbing { get; private set; }
    /// <summary>Devorga ilashib sirg'anyapti (animatsiya uchun).</summary>
    public bool IsWallSliding { get; private set; }
    /// <summary>Robot o'lganda (tuzoq yoki chuqur) chaqiriladi.</summary>
    public event System.Action Died;
    /// <summary>Daraja tugaganda chaqiriladi.</summary>
    public event System.Action Finished;

    Rigidbody2D rb;
    Vector3 spawnPoint;
    float coyoteCounter;
    float bufferCounter;
    bool frozen;

    Collider2D col;
    Ladder currentLadder;
    float defaultGravity;
    float controlLock;
    int wallDir;
    float wallCoyote;
    readonly System.Collections.Generic.List<Collider2D> ignored = new System.Collections.Generic.List<Collider2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        defaultGravity = rb.gravityScale;
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
        controlLock -= Time.deltaTime;

        float h = MobileInput.Horizontal;
        float v = MobileInput.Vertical;

        // --- Narvon ---
        if (currentLadder != null && !IsClimbing && Mathf.Abs(v) > 0.5f)
        {
            // Narvon pastida yerda turib "pastga" bosilsa ilashmaydi
            bool canGrab = !(grounded && v < 0f && transform.position.y < currentLadder.BottomY + 0.9f);
            if (canGrab) GrabLadder();
        }
        if (IsClimbing)
        {
            if (currentLadder == null) { StopClimbing(); }
            else if (bufferCounter > 0f)
            {
                // Narvondan sakrab tushish
                StopClimbing();
                Vector2 vel = rb.GetVelocity();
                rb.SetVelocity(new Vector2(h * moveSpeed, jumpForce * 0.85f));
                bufferCounter = 0f; coyoteCounter = 0f;
                controlLock = 0.1f;
            }
            else if (grounded && v < -0.5f && transform.position.y < currentLadder.BottomY + 0.9f)
            {
                StopClimbing(); // narvon pastiga yetdi
            }
        }

        // --- Devor ---
        wallDir = 0;
        if (!grounded && !IsClimbing) wallDir = DetectWall();
        bool pushing = wallDir != 0 && Mathf.Sign(h) == wallDir && Mathf.Abs(h) > 0.1f;
        IsWallSliding = pushing && rb.GetVelocity().y < 0.5f && !IsClimbing;
        if (wallDir != 0) wallCoyote = 0.12f; else wallCoyote -= Time.deltaTime;

        if (!IsClimbing)
        {
            bool dropped = false;
            // Platformadan pastga tushish: pastga + sakrash
            if (grounded && v < -0.5f && bufferCounter > 0f) dropped = TryDropThrough();
            if (dropped)
            {
                bufferCounter = 0f; coyoteCounter = 0f;
            }
            else if (bufferCounter > 0f && coyoteCounter > 0f)
            {
                Vector2 vel = rb.GetVelocity();
                rb.SetVelocity(new Vector2(vel.x, jumpForce));
                bufferCounter = 0f;
                coyoteCounter = 0f;
            }
            else if (bufferCounter > 0f && !grounded && wallCoyote > 0f && wallDir != 0)
            {
                // Devordan sakrash
                rb.SetVelocity(new Vector2(-wallDir * wallJumpForce.x, wallJumpForce.y));
                controlLock = wallJumpLock;
                bufferCounter = 0f; wallCoyote = 0f;
                Vector3 sc = transform.localScale;
                sc.x = Mathf.Abs(sc.x) * -wallDir;
                transform.localScale = sc;
            }

            // Tugmani erta qo'yib yuborsa, sakrash pastroq bo'ladi
            if (MobileInput.JumpReleased)
            {
                Vector2 vel = rb.GetVelocity();
                if (vel.y > 0f && controlLock <= 0f) rb.SetVelocity(new Vector2(vel.x, vel.y * 0.5f));
            }

            // Devorda sirg'anish
            if (IsWallSliding)
            {
                Vector2 vel = rb.GetVelocity();
                if (vel.y < -wallSlideSpeed) rb.SetVelocity(new Vector2(vel.x, -wallSlideSpeed));
            }
        }

        // Tomonga qarash
        Vector3 s = transform.localScale;
        if (controlLock <= 0f)
        {
            if (IsWallSliding) s.x = -Mathf.Abs(s.x) * wallDir; // devordan teskari qaraydi
            else if (h > 0.01f) s.x = Mathf.Abs(s.x);
            else if (h < -0.01f) s.x = -Mathf.Abs(s.x);
        }
        transform.localScale = s;

        // Chuqurga tushib ketdi
        if (transform.position.y < killY) Die();
    }

    void FixedUpdate()
    {
        if (frozen) return;
        Vector2 v = rb.GetVelocity();
        if (IsClimbing)
        {
            float vy = MobileInput.Vertical * climbSpeed;
            float vx = MobileInput.Horizontal * moveSpeed * 0.45f;
            if (Mathf.Abs(MobileInput.Horizontal) < 0.1f && currentLadder != null)
                vx = Mathf.Clamp((currentLadder.CenterX - transform.position.x) * 8f, -2f, 2f);
            // Yuqoriga chiqib, tepasidan oshganda: sakrab chiqib ketadi
            if (currentLadder != null && vy > 0f && transform.position.y - 0.6f > currentLadder.TopY - 0.15f)
            {
                int dir = currentLadder.exitDir;
                StopClimbing();
                rb.SetVelocity(new Vector2(dir * moveSpeed, 8.5f));
                controlLock = 0.18f;
                return;
            }
            rb.SetVelocity(new Vector2(vx, vy));
            return;
        }
        if (controlLock > 0f) return; // devordan sakraganda havoda ushlab turadi
        rb.SetVelocity(new Vector2(MobileInput.Horizontal * moveSpeed, v.y));
    }

    /// <summary>Nazorat nuqtasi: o'lganda shu yerda qayta tug'iladi.</summary>
    public void SetCheckpoint(Vector3 pos) { spawnPoint = pos; }

    // ---------------- Narvon ----------------
    public void EnterLadder(Ladder l) { currentLadder = l; }
    public void ExitLadder(Ladder l)
    {
        if (currentLadder != l) return;
        currentLadder = null;
        if (IsClimbing) StopClimbing();
    }

    void GrabLadder()
    {
        IsClimbing = true;
        rb.gravityScale = 0f;
        rb.SetVelocity(Vector2.zero);
        // Narvon o'rtasiga yaqinlashtirish
        Vector3 p = transform.position;
        p.x = Mathf.MoveTowards(p.x, currentLadder.CenterX, 0.35f);
        transform.position = p;
        coyoteCounter = 0f;
    }

    void StopClimbing()
    {
        IsClimbing = false;
        rb.gravityScale = defaultGravity;
    }

    // ---------------- Devor ----------------
    int DetectWall()
    {
        if (col == null) return 0;
        Bounds b = col.bounds;
        float y = b.center.y;
        for (int side = -1; side <= 1; side += 2)
        {
            Vector2 origin = new Vector2(b.center.x + side * (b.extents.x - 0.02f), y);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, new Vector2(side, 0f), wallCheckDistance + 0.02f, groundLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                var c = hits[i].collider;
                if (c == null || c.isTrigger || c == col) continue;
                if (c.GetComponent<PlatformEffector2D>() != null) continue; // bir tomonlama platforma devor emas
                return side;
            }
        }
        return 0;
    }

    // ---------------- Platformadan tushish ----------------
    bool TryDropThrough()
    {
        if (groundCheck == null || col == null) return false;
        Collider2D[] found = Physics2D.OverlapCircleAll(groundCheck.position, groundRadius + 0.05f, groundLayer);
        bool any = false;
        for (int i = 0; i < found.Length; i++)
        {
            var c = found[i];
            if (c.GetComponent<PlatformEffector2D>() == null) continue;
            Physics2D.IgnoreCollision(col, c, true);
            ignored.Add(c);
            any = true;
        }
        if (any)
        {
            CancelInvoke("RestoreDropThrough");
            Invoke("RestoreDropThrough", dropThroughTime);
            rb.SetVelocity(new Vector2(rb.GetVelocity().x, -2f));
        }
        return any;
    }

    void RestoreDropThrough()
    {
        for (int i = 0; i < ignored.Count; i++)
            if (ignored[i] != null && col != null) Physics2D.IgnoreCollision(col, ignored[i], false);
        ignored.Clear();
    }

    /// <summary>Dushmanni bosganda yuqoriga sakratish.</summary>
    public void Bounce(float force)
    {
        Vector2 v = rb.GetVelocity();
        rb.SetVelocity(new Vector2(v.x, force));
        coyoteCounter = 0f;
    }

    public float VerticalSpeed { get { return rb != null ? rb.GetVelocity().y : 0f; } }

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
        controlLock = 0f;
        if (IsClimbing) StopClimbing();
        currentLadder = null;
        RestoreDropThrough();
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
