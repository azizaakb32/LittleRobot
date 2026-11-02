using UnityEngine;
using UnityEngine.UI;

/// <summary>Til tugmasi: bosilganda UZ, EN, RU orasida almashadi. Tugma matnida joriy til kodi ko'rinadi.</summary>
public class LanguageButton : MonoBehaviour
{
    public Text label;

    void OnEnable()
    {
        Localization.Changed += Refresh;
        Refresh();
    }

    void OnDisable() { Localization.Changed -= Refresh; }

    void Refresh()
    {
        if (label != null) label.text = Localization.Code;
    }

    /// <summary>Button.onClick shu metodga ulanadi.</summary>
    public void Cycle()
    {
        Localization.Next();
    }
}
