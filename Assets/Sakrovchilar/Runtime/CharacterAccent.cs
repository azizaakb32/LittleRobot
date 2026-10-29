using UnityEngine;
using UnityEngine.UI;

/// <summary>Image rangini tanlangan personajning rangiga o'zgartiradi (masalan, sakrash tugmasi).</summary>
[RequireComponent(typeof(Image))]
public class CharacterAccent : MonoBehaviour
{
    void Start()
    {
        GetComponent<Image>().color = CharacterCatalog.Selected.accent;
    }
}
