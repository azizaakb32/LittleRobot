using UnityEngine;

/// <summary>O'q: to'g'ri uchadi, o'yinchiga tegsa uni o'ldiradi, devor yoki yerga tegsa yo'qoladi.</summary>
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public Vector2 velocity = new Vector2(-5f, 0f);
    public float lifeTime = 5f;

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, -360f * Time.deltaTime);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null) player = other.GetComponentInParent<PlayerController>();
            if (player != null) player.Die();
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);   // yer, devor, platforma
        }
    }
}
