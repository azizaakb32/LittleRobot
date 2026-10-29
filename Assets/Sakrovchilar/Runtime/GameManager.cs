using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Daraja holati: tangalar, kalit, vaqt, yulduzlar, pauza va saqlash.
/// Har bir daraja sahnasida bitta nusxa bo'lishi kerak.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Daraja")]
    [Tooltip("Daraja raqami (1 dan boshlanadi). Saqlash uchun kerak.")]
    public int levelNumber = 1;
    public string worldName = "Zavod";
    [Tooltip("Shu dunyoning birinchi darajasi raqami (menyudagi 1-10, 11-20 ... uchun)")]
    public int worldFirstLevel = 1;
    [Tooltip("Shu vaqtdan tez tugatsa, 3-yulduz (soniya)")]
    public float parTime = 35f;
    [Tooltip("Keyingi darajaning sahna nomi")]
    public string nextSceneName = "Level2";
    public string menuSceneName = "Menu";

    [Header("HUD")]
    public Text levelLabel;
    public Text coinText;
    public Text timerText;
    public Image keyImage;

    [Header("G'alaba oynasi")]
    public GameObject winPanel;
    public Image[] winStars;
    public Text winInfoText;
    [Tooltip("G'alaba oynasida tanlangan personajning xursand rasmi")]
    public Image winCharacterImage;
    public Color starOn = new Color(1f, 0.788f, 0.235f, 1f);
    public Color starOff = new Color(0.843f, 0.863f, 0.922f, 1f);

    [Header("Pauza oynasi")]
    public GameObject pausePanel;

    public bool HasKey { get; private set; }

    int coins;
    int totalCoins;
    float timer;
    bool finished;
    PlayerController player;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        MobileInput.ResetState();
    }

    void Start()
    {
        player = UnityCompat.FindFirst<PlayerController>();

        totalCoins = 0;
        foreach (var p in UnityCompat.FindAll<Pickup>())
            if (p.type == Pickup.PickupType.Coin) totalCoins++;

        if (winPanel != null) winPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (winCharacterImage != null)
        {
            var happy = CharacterCatalog.Load(CharacterCatalog.Selected, "happy");
            if (happy != null) winCharacterImage.sprite = happy;
        }
        if (levelLabel != null) levelLabel.text = worldName + " · " + (levelNumber - worldFirstLevel + 1);
        SetKeyVisual(false);
        UpdateUI();
    }

    void Update()
    {
        if (finished) return;
        timer += Time.deltaTime;
        UpdateUI();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Time.timeScale = 1f;
    }

    void UpdateUI()
    {
        if (coinText != null) coinText.text = coins + " / " + totalCoins;
        if (timerText != null) timerText.text = timer.ToString("0.0") + " s";
    }

    void SetKeyVisual(bool has)
    {
        if (keyImage == null) return;
        Color c = keyImage.color;
        c.a = has ? 1f : 0.35f;
        keyImage.color = c;
    }

    // ---------- O'yin hodisalari ----------

    public void CollectCoin()
    {
        coins++;
        UpdateUI();
    }

    public void CollectKey()
    {
        HasKey = true;
        SetKeyVisual(true);
    }

    public void OnPlayerDied()
    {
        // Kalit va tangalar saqlanadi (jazo yengil). O'limlar sonini shu yerda sanash mumkin.
    }

    public void CompleteLevel()
    {
        if (finished) return;
        finished = true;
        if (player != null) player.Freeze();

        int stars = 1;
        if (coins >= totalCoins) stars++;
        if (timer <= parTime) stars++;

        SaveStars(stars);

        // Daromad uchun: har 3-4 darajadan keyin shu yerda interstitial reklama ko'rsating.

        if (winPanel != null) winPanel.SetActive(true);
        if (winStars != null)
        {
            for (int i = 0; i < winStars.Length; i++)
                if (winStars[i] != null) winStars[i].color = i < stars ? starOn : starOff;
        }
        if (winInfoText != null)
            winInfoText.text = "Tangalar: " + coins + "/" + totalCoins + "\nVaqt: " + timer.ToString("0.0") + " s";
        // Dunyoning oxirgi darajasi: keyingi dunyo uchun yulduz holatini ko'rsatamiz
        if (winInfoText != null && levelNumber % GameInfo.LevelsPerWorld == 0)
        {
            int next = WorldGate.WorldOf(levelNumber) + 1;
            winInfoText.text += "\nKeyingi dunyo: " + WorldGate.CurrentStars(next) + "/" + WorldGate.Required + " yulduz";
        }
    }

    // ---------- Tugmalar uchun ----------

    public void TogglePause()
    {
        if (finished) return;
        bool pause = Time.timeScale > 0f;
        Time.timeScale = pause ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(pause);
        if (pause) MobileInput.ResetState();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        // Keyingi dunyo qulflangan bo'lsa yoki oxirgi daraja bo'lsa: menyuga qaytamiz (u yerda yulduz darvozasi chiqadi)
        bool nextWorldLocked = !WorldGate.IsLevelUnlocked(levelNumber + 1);
        if (!nextWorldLocked && !string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
            LoadOrRestart(nextSceneName);
        else
            GoToMenu();
    }
    public void GoToMenu() { LoadOrRestart(menuSceneName); }

    /// <summary>Reklama ulangandan keyin: tangalarni 2 barobar qilish (hozircha o'rin egasi).</summary>
    public void OnRewardedDoubleCoins()
    {
        Debug.Log("[Sakrovchilar] Rewarded reklama shu yerda ko'rsatiladi (AdMob).");
    }

    void LoadOrRestart(string sceneName)
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("[Sakrovchilar] '" + sceneName + "' sahnasi Build Settings'da yo'q. Joriy daraja qayta yuklanmoqda.");
            RestartLevel();
        }
    }

    // ---------- Saqlash ----------

    public static int GetStars(int level)
    {
        return PlayerPrefs.GetInt("stars_level_" + level, 0);
    }

    public static int GetUnlockedLevel()
    {
        return Mathf.Max(1, PlayerPrefs.GetInt("unlocked_level", 1));
    }

    void SaveStars(int stars)
    {
        string key = "stars_level_" + levelNumber;
        if (stars > PlayerPrefs.GetInt(key, 0))
            PlayerPrefs.SetInt(key, stars);

        if (levelNumber + 1 > GetUnlockedLevel())
            PlayerPrefs.SetInt("unlocked_level", levelNumber + 1);

        PlayerPrefs.Save();
    }
}
