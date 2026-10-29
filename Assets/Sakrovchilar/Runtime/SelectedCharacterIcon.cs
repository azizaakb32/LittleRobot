using UnityEngine;
using UnityEngine.UI;

/// <summary>Image'ga tanlangan personajning rasmini qo'yadi (menyudagi "personaj" tugmasi uchun).</summary>
[RequireComponent(typeof(Image))]
public class SelectedCharacterIcon : MonoBehaviour
{
    public string frame = "idle";

    void Start()
    {
        var sprite = CharacterCatalog.Load(CharacterCatalog.Selected, frame);
        if (sprite != null) GetComponent<Image>().sprite = sprite;
    }
}
