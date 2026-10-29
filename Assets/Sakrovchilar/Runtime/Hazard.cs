using UnityEngine;

/// <summary>
/// Tikan va boshqa xavfli narsalar. Robot tegsa boshiga qaytadi.
/// Collider2D da "Is Trigger" yoqilgan bo'lishi kerak.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.GetComponent<PlayerController>();
        if (player != null) player.Die();
    }
}
