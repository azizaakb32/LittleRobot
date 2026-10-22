using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ekrandagi ushlab turiladigan tugma (chap, o'ng, sakrash).
/// Bir xil obyektda raycast qabul qiladigan UI Image bo'lishi kerak.
/// </summary>
public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Kind { Left, Right, Jump }
    public Kind kind = Kind.Left;

    bool down;

    public void OnPointerDown(PointerEventData eventData) { Set(true); }
    public void OnPointerUp(PointerEventData eventData) { Set(false); }
    public void OnPointerExit(PointerEventData eventData) { if (down) Set(false); }
    void OnDisable() { if (down) Set(false); }

    void Set(bool value)
    {
        if (down == value) return;
        down = value;
        switch (kind)
        {
            case Kind.Left: MobileInput.SetLeft(value); break;
            case Kind.Right: MobileInput.SetRight(value); break;
            case Kind.Jump:
                if (value) MobileInput.JumpDown(); else MobileInput.JumpUp();
                break;
        }
    }
}
