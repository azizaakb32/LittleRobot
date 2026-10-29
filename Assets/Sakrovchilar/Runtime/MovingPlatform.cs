using UnityEngine;

/// <summary>
/// A nuqtadan B nuqtaga borib-keladigan platforma (lift).
/// Rigidbody2D = Kinematic. Robot ustida tursa, platforma bilan birga harakatlanadi.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Tooltip("Boradigan nuqta (bo'sh obyekt). Platformaning bolasi bo'lmasin!")]
    public Transform pointB;
    public float speed = 2f;
    public float waitTime = 0.3f;

    Rigidbody2D rb;
    Collider2D myCollider;
    Vector2 a, b, target;
    float waitCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        a = transform.position;
        b = pointB != null ? (Vector2)pointB.position : a;
        target = b;
    }

    void FixedUpdate()
    {
        if (waitCounter > 0f)
        {
            waitCounter -= Time.fixedDeltaTime;
            return;
        }

        Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        if (Vector2.Distance(next, target) < 0.01f)
        {
            target = (target == b) ? a : b;
            waitCounter = waitTime;
        }
    }

    // Robot platforma ustida turganda, uni platforma bilan birga olib yuramiz
    void OnCollisionStay2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        if (col.transform.parent == transform) return;

        Collider2D playerCol = col.gameObject.GetComponent<Collider2D>();
        if (playerCol == null || myCollider == null) return;

        // Robotning oyog'i platformaning tepasidan yuqorida bo'lsa
        if (playerCol.bounds.min.y >= myCollider.bounds.max.y - 0.2f)
            col.transform.SetParent(transform);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") && col.transform.parent == transform)
            col.transform.SetParent(null);
    }

    void OnDrawGizmosSelected()
    {
        if (pointB == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, pointB.position);
        Gizmos.DrawWireSphere(pointB.position, 0.2f);
    }
}
