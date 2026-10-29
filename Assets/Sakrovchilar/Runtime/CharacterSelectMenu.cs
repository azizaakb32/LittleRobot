using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Personaj tanlash ekranidagi bitta karta.</summary>
[System.Serializable]
public class CharacterCardUI
{
    public Button button;
    public Image portrait;
    public Image highlight;
    public Text nameText;
}

/// <summary>
/// O'yin boshida personaj tanlash ekrani. Tanlangan personaj saqlanadi
/// va o'yin davomida (PlayerAnimator orqali) ishlatiladi.
/// </summary>
public class CharacterSelectMenu : MonoBehaviour
{
    public CharacterCardUI[] cards;
    public Text titleText;
    public Text subtitleText;
    public string nextSceneName = "Menu";
    public float previewFps = 9f;

    int selected;
    Sprite[][] runFrames;
    Sprite[] idleFrames;

    void Start()
    {
        Time.timeScale = 1f;
        if (titleText != null) titleText.text = GameInfo.Title;
        if (subtitleText != null) subtitleText.text = GameInfo.Subtitle;

        int count = Mathf.Min(cards != null ? cards.Length : 0, CharacterCatalog.All.Length);
        runFrames = new Sprite[count][];
        idleFrames = new Sprite[count];

        for (int i = 0; i < count; i++)
        {
            var info = CharacterCatalog.All[i];
            idleFrames[i] = CharacterCatalog.Load(info, "idle");
            runFrames[i] = new Sprite[CharacterCatalog.RunFrameCount];
            for (int f = 0; f < runFrames[i].Length; f++)
                runFrames[i][f] = CharacterCatalog.Load(info, CharacterCatalog.RunFramePrefix + f);

            var card = cards[i];
            if (card.nameText != null) card.nameText.text = info.displayName;
            if (card.highlight != null) card.highlight.color = info.accent;
            if (card.portrait != null && idleFrames[i] != null) card.portrait.sprite = idleFrames[i];

            int index = i;
            if (card.button != null) card.button.onClick.AddListener(() => Select(index));
        }

        Select(Mathf.Clamp(CharacterCatalog.SelectedIndex, 0, Mathf.Max(0, count - 1)));
    }

    void Update()
    {
        if (cards == null || runFrames == null) return;

        // Tanlangan personaj yuguradi, qolganlari turadi
        for (int i = 0; i < runFrames.Length; i++)
        {
            var card = cards[i];
            if (card == null || card.portrait == null) continue;

            if (i == selected)
            {
                int frame = (int)(Time.unscaledTime * previewFps) % runFrames[i].Length;
                var sprite = runFrames[i][frame];
                if (sprite != null) card.portrait.sprite = sprite;
            }
            else if (idleFrames[i] != null)
            {
                card.portrait.sprite = idleFrames[i];
            }
        }
    }

    public void Select(int index)
    {
        selected = index;
        CharacterCatalog.SelectedIndex = index;

        for (int i = 0; i < cards.Length; i++)
        {
            var card = cards[i];
            if (card == null) continue;
            bool on = i == index;
            if (card.highlight != null) card.highlight.gameObject.SetActive(on);
            if (card.portrait != null)
                card.portrait.transform.localScale = on ? new Vector3(1.12f, 1.12f, 1f) : Vector3.one;
        }
    }

    /// <summary>"O'ynash" tugmasi.</summary>
    public void Play()
    {
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[Sakrovchilar] '" + nextSceneName + "' sahnasi Build Settings'da yo'q.");
    }
}
