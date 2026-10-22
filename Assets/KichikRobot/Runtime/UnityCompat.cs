using UnityEngine;

/// <summary>
/// Unity versiyalari orasidagi farqlarni yashiradi (Unity 2021 LTS ... Unity 6).
/// </summary>
public static class UnityCompat
{
    public static Vector2 GetVelocity(this Rigidbody2D rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    public static void SetVelocity(this Rigidbody2D rb, Vector2 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }

    public static T FindFirst<T>() where T : UnityEngine.Object
    {
#if UNITY_2023_1_OR_NEWER
        return UnityEngine.Object.FindFirstObjectByType<T>();
#else
        return UnityEngine.Object.FindObjectOfType<T>();
#endif
    }

    public static T[] FindAll<T>() where T : UnityEngine.Object
    {
#if UNITY_2023_1_OR_NEWER
        return UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return UnityEngine.Object.FindObjectsOfType<T>();
#endif
    }
}
