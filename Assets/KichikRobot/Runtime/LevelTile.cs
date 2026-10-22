using UnityEngine;
using UnityEngine.UI;

/// <summary>Daraja tanlash ekranidagi bitta katakcha.</summary>
public class LevelTile : MonoBehaviour
{
    public int level = 1;
    public Button button;
    public Image background;
    public Text numberText;
    public Image[] stars;
    public GameObject lockIcon;

    [Header("Ranglar")]
    public Color doneColor = new Color(0.173f, 0.227f, 0.361f, 1f);
    public Color currentColor = new Color(1f, 0.541f, 0.239f, 1f);
    public Color lockedColor = new Color(0.133f, 0.18f, 0.302f, 1f);
    public Color lightText = new Color(0.918f, 0.941f, 1f, 1f);
    public Color darkText = new Color(0.078f, 0.125f, 0.239f, 1f);
    public Color lockedText = new Color(0.561f, 0.627f, 0.784f, 1f);
    public Color starOn = new Color(1f, 0.788f, 0.235f, 1f);
    public Color starOff = new Color(1f, 1f, 1f, 0.18f);

    public void Refresh(int unlockedLevel, int starCount)
    {
        bool locked = level > unlockedLevel;
        bool current = level == unlockedLevel;

        if (background != null)
            background.color = locked ? lockedColor : (current ? currentColor : doneColor);

        if (numberText != null)
        {
            numberText.text = level.ToString();
            numberText.color = locked ? lockedText : (current ? darkText : lightText);
            numberText.fontSize = locked ? 28 : 44;
            numberText.rectTransform.anchoredPosition = locked ? new Vector2(0f, -16f) : new Vector2(0f, 8f);
        }

        if (lockIcon != null) lockIcon.SetActive(locked);

        if (stars != null)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] == null) continue;
                stars[i].gameObject.SetActive(!locked);
                if (i < starCount) stars[i].color = starOn;
                else stars[i].color = current ? new Color(0.078f, 0.125f, 0.239f, 0.3f) : starOff;
            }
        }

        if (button != null) button.interactable = !locked;
    }
}
