using UnityEngine;

/// <summary>
/// Tanga yoki kalit. Collider2D da "Is Trigger" yoqilgan bo'lishi kerak.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Pickup : MonoBehaviour
{
    public enum PickupType { Coin, Key }
    public PickupType type = PickupType.Coin;

    [Header("Tebranish effekti")]
    public float bobHeight = 0.1f;
    public float bobSpeed = 3f;

    Vector3 startPos;
    bool taken;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed + startPos.x) * bobHeight;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (taken || !other.CompareTag("Player")) return;
        taken = true;

        if (GameManager.Instance != null)
        {
            if (type == PickupType.Coin) GameManager.Instance.CollectCoin();
            else GameManager.Instance.CollectKey();
        }

        // Bu yerga ovoz/zarracha effekti qo'shsa bo'ladi
        Destroy(gameObject);
    }
}
