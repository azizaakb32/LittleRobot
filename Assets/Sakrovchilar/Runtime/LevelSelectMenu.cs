using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Daraja tanlash ekrani. Katakchalar ochiq/qulflangan holatini PlayerPrefs'dan oladi.
/// Darajalar sahifalarga (dunyolarga) bo'lingan: har sahifada tilesPerPage ta daraja.
/// Sahnalar "Level1", "Level2", ... nomi bilan Build Settings'da bo'lishi kerak.
/// </summary>
public class LevelSelectMenu : MonoBehaviour
{
    public LevelTile[] tiles;
    public Text totalStarsText;
    public Text titleText;
    public Button prevButton;
    public Button nextButton;
    public int tilesPerPage = 10;
    public string[] worldNames = { "Zavod", "O'rmon", "Cho'l", "Muz", "Kosmos", "Vulqon", "Okean", "Bulutlar", "Shahar", "Sehrli" };
    public string sceneNameFormat = "Level{0}";

    [Header("Yulduz darvozasi")]
    public GameObject gatePanel;
    public Text gateTitle;
    public Text gateInfo;
    public Button adButton;
    public Text adButtonText;
    public Button buyButton;
    public Text buyButtonText;
    public Button lockBanner;
    public Text lockBannerText;
    public Text toastText;

    int page;

    int PageCount
    {
        get
        {
            if (tiles == null || tilesPerPage <= 0) return 1;
            return Mathf.Max(1, (tiles.Length + tilesPerPage - 1) / tilesPerPage);
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        if (tiles != null)
        {
            foreach (var tile in tiles)
            {
                if (tile == null || tile.button == null) continue;
                int level = tile.level;
                tile.button.onClick.AddListener(() => Open(level));
            }
        }

        // Oxirgi ochilgan daraja turgan dunyodan boshlaymiz
        int unlocked = GameManager.GetUnlockedLevel();
        page = Mathf.Clamp((unlocked - 1) / Mathf.Max(1, tilesPerPage), 0, PageCount - 1);
        if (gatePanel != null) gatePanel.SetActive(false);
        if (toastText != null) toastText.gameObject.SetActive(false);
        RefreshAll();

        // Yangi dunyo qulflangan bo'lsa, darvoza oynasini o'zi ochamiz
        if (!WorldGate.IsUnlocked(page) && WorldGate.PreviousWorldFinished(page)) ShowGate(page);
    }

    void RefreshAll()
    {
        int unlocked = GameManager.GetUnlockedLevel();
        int totalStars = 0;
        if (tiles != null)
        {
            foreach (var tile in tiles)
            {
                if (tile == null) continue;
                int stars = GameManager.GetStars(tile.level);
                totalStars += stars;
                tile.Refresh(unlocked, stars, !WorldGate.IsLevelUnlocked(tile.level));
            }
        }
        if (totalStarsText != null)
            totalStarsText.text = totalStars + " / " + ((tiles != null ? tiles.Length : 0) * 3);
        ShowPage();
    }

    public void NextPage()
    {
        page = (page + 1) % PageCount;
        ShowPage();
    }

    public void PrevPage()
    {
        page = (page - 1 + PageCount) % PageCount;
        ShowPage();
    }

    void ShowPage()
    {
        if (tiles != null)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == null) continue;
                tiles[i].gameObject.SetActive(i / Mathf.Max(1, tilesPerPage) == page);
            }
        }

        if (titleText != null)
        {
            string name = (worldNames != null && page < worldNames.Length) ? worldNames[page] : ("Dunyo " + (page + 1));
            titleText.text = name;
        }

        if (lockBanner != null)
        {
            bool locked = !WorldGate.IsUnlocked(page);
            lockBanner.gameObject.SetActive(locked);
            if (locked && lockBannerText != null)
                lockBannerText.text = "Qulf: " + WorldGate.CurrentStars(page) + "/" + WorldGate.Required + " yulduz. Ochish";
        }

        bool multi = PageCount > 1;
        if (prevButton != null) prevButton.gameObject.SetActive(multi);
        if (nextButton != null) nextButton.gameObject.SetActive(multi);
    }

    public void Open(int level)
    {
        int world = WorldGate.WorldOf(level);
        if (!WorldGate.IsUnlocked(world))
        {
            ShowGate(world);
            return;
        }
        string sceneName = string.Format(sceneNameFormat, level);
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("[Sakrovchilar] '" + sceneName + "' sahnasi yo'q. Menyu: Sakrovchilar > 4. Darajalarni JSON'dan qayta yaratish.");
    }

    // ---------- Yulduz darvozasi ----------

    int gateWorld;

    public void OpenGateForPage() { ShowGate(page); }

    void ShowGate(int world)
    {
        if (gatePanel == null || world <= 0) return;
        gateWorld = world;
        gatePanel.SetActive(true);
        RefreshGate();
    }

    void RefreshGate()
    {
        int have = WorldGate.CurrentStars(gateWorld);
        int need = WorldGate.Required;
        string name = (worldNames != null && gateWorld < worldNames.Length) ? worldNames[gateWorld] : ("Dunyo " + (gateWorld + 1));
        if (gateTitle != null) gateTitle.text = name + " qulflangan";
        if (gateInfo != null)
        {
            int missing = Mathf.Max(0, need - have);
            gateInfo.text = "Ochish uchun oldingi dunyodan " + need + " yulduz kerak.\nSizda: " + have + " / " + need +
                            (missing > 0 ? "\nYetishmayapti: " + missing : "");
        }
        if (adButtonText != null) adButtonText.text = "Reklama ko'r (+" + GameInfo.StarsPerAd + " yulduz)";
        if (adButton != null) adButton.gameObject.SetActive(WorldGate.PreviousWorldFinished(gateWorld));
        if (buyButtonText != null) buyButtonText.text = "Darhol ochish: " + StoreService.PriceText(StoreService.ProductForWorld(gateWorld));
    }

    public void CloseGate()
    {
        if (gatePanel != null) gatePanel.SetActive(false);
    }

    public void GateWatchAd()
    {
        int world = gateWorld;
        AdService.ShowRewarded(ok =>
        {
            if (!ok) { Toast("Reklama hozir mavjud emas. Keyinroq urinib ko'ring."); return; }
            WorldGate.AddAdStars(world);
            AfterGateChange(world);
        });
    }

    public void GateBuy()
    {
        int world = gateWorld;
        StoreService.Purchase(StoreService.ProductForWorld(world), ok =>
        {
            if (!ok) { Toast("Xarid amalga oshmadi."); return; }
            WorldGate.UnlockByPurchase(world);
            AfterGateChange(world);
        });
    }

    void AfterGateChange(int world)
    {
        RefreshAll();
        if (WorldGate.IsUnlocked(world))
        {
            CloseGate();
            page = Mathf.Clamp(world, 0, PageCount - 1);
            ShowPage();
            Toast("Yangi dunyo ochildi!");
        }
        else
        {
            RefreshGate();
        }
    }

    void Toast(string message)
    {
        if (toastText == null) { Debug.Log("[Sakrovchilar] " + message); return; }
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        CancelInvoke("HideToast");
        Invoke("HideToast", 2.5f);
    }

    void HideToast()
    {
        if (toastText != null) toastText.gameObject.SetActive(false);
    }

    /// <summary>Personaj tanlash ekraniga qaytish.</summary>
    public void OpenCharacterSelect()
    {
        if (Application.CanStreamedLevelBeLoaded("CharacterSelect"))
            SceneManager.LoadScene("CharacterSelect");
    }

    /// <summary>Sinov uchun: barcha saqlashni o'chiradi.</summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
