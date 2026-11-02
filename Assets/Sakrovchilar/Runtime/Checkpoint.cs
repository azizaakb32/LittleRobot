using UnityEngine;

/// <summary>
/// Nazorat bayrog'i. O'yinchi tegsa, o'lganda shu yerdan qayta boshlaydi.
/// Trigger Collider2D bo'lishi kerak.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    public SpriteRenderer flag;
    public Color activeColor = new Color(0.55f, 1f, 0.6f, 1f);
    bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        var p = other.GetComponent<PlayerController>();
        if (p == null) return;
        used = true;
        p.SetCheckpoint(transform.position + new Vector3(0f, 0.2f, 0f));
        if (flag != null) flag.color = activeColor;
    }
}
