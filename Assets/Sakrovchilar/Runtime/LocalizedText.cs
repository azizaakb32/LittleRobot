using UnityEngine;
using UnityEngine.UI;

/// <summary>Text'ga kalit bo'yicha tarjima qo'yadi va til almashganda yangilaydi.</summary>
[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    public string key;

    Text label;

    void Awake() { label = GetComponent<Text>(); }

    void OnEnable()
    {
        Localization.Changed += Apply;
        Apply();
    }

    void OnDisable() { Localization.Changed -= Apply; }

    void Apply()
    {
        if (label == null) label = GetComponent<Text>();
        if (label != null && !string.IsNullOrEmpty(key)) label.text = Localization.T(key);
    }
}
