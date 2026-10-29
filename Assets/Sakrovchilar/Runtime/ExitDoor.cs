using UnityEngine;

/// <summary>
/// Chiqish eshigi. Kalit olingandan keyingina ochiladi.
/// Collider2D da "Is Trigger" yoqilgan bo'lishi kerak.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ExitDoor : MonoBehaviour
{
    public SpriteRenderer doorSprite;
    public Color lockedColor = new Color(0.6f, 0.6f, 0.65f, 1f);
    public Color openColor = Color.white;

    void Update()
    {
        if (doorSprite != null && GameManager.Instance != null)
            doorSprite.color = GameManager.Instance.HasKey ? openColor : lockedColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null && GameManager.Instance.HasKey)
            GameManager.Instance.CompleteLevel();
    }
}
