using UnityEngine;

/// <summary>
/// Narvon / arqon. Trigger BoxCollider2D bo'lishi kerak. O'yinchi yuqoriga yoki pastga tugmasini
/// bossa, narvonga ilashadi va yuqoriga-pastga ko'tariladi.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Ladder : MonoBehaviour
{
    [Tooltip("Narvon tepasidan chiqqanda qaysi tomonga sakraydi (+1 o'ng, -1 chap)")]
    public int exitDir = 1;

    public float TopY { get { return GetComponent<BoxCollider2D>().bounds.max.y; } }
    public float BottomY { get { return GetComponent<BoxCollider2D>().bounds.min.y; } }
    public float CenterX { get { return transform.position.x; } }

    void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p != null) p.EnterLadder(this);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponent<PlayerController>();
        if (p != null) p.ExitLadder(this);
    }
}
