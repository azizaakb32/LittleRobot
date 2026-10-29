using UnityEngine;

/// <summary>
/// Kamera robotni silliq kuzatadi. Ortografik kamera bilan ishlaydi.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector2 offset = new Vector2(0f, 1.5f);

    [Header("Daraja chegaralari (dunyo koordinatalari)")]
    public bool useBounds = false;
    [Tooltip("Darajaning pastki-chap burchagi")]
    public Vector2 minPos = new Vector2(-100f, -100f);
    [Tooltip("Darajaning yuqori-o'ng burchagi")]
    public Vector2 maxPos = new Vector2(100f, 100f);

    Vector3 velocity;
    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (target != null) transform.position = Clamp(Desired());
    }

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.SmoothDamp(transform.position, Clamp(Desired()), ref velocity, smoothTime);
    }

    Vector3 Desired()
    {
        return new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
    }

    Vector3 Clamp(Vector3 p)
    {
        if (!useBounds) return p;
        float halfH = (cam != null && cam.orthographic) ? cam.orthographicSize : 5f;
        float halfW = halfH * (cam != null ? cam.aspect : 1.78f);
        p.x = ClampAxis(p.x, minPos.x + halfW, maxPos.x - halfW);
        p.y = ClampAxis(p.y, minPos.y + halfH, maxPos.y - halfH);
        return p;
    }

    static float ClampAxis(float v, float lo, float hi)
    {
        if (lo > hi) return (lo + hi) * 0.5f;
        return Mathf.Clamp(v, lo, hi);
    }
}
