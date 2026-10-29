using UnityEngine;

/// <summary>
/// Osmon gradienti: kameraga bog'langan sprite, har doim butun ekranni qoplaydi.
/// Sprite tepada to'q, pastda shaffof (oq rangli): SpriteRenderer.color osmonning tepa rangi.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SkyBackdrop : MonoBehaviour
{
    SpriteRenderer sr;
    Camera cam;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = GetComponentInParent<Camera>();
    }

    void LateUpdate()
    {
        if (cam == null || sr == null || sr.sprite == null) return;
        float h = cam.orthographicSize * 2f * 1.05f;
        float w = h * cam.aspect * 1.05f;
        Vector2 size = sr.sprite.bounds.size;
        transform.localScale = new Vector3(w / size.x, h / size.y, 1f);
        transform.localPosition = new Vector3(0f, 0f, 20f);
    }
}
