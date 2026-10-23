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
    public string[] worldNames = { "Zavod", "O'rmon", "Cho'l", "Muz", "Kosmos" };
    public string sceneNameFormat = "Level{0}";

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
        int unlocked = GameManager.GetUnlockedLevel();
        int totalStars = 0;

        if (tiles != null)
        {
            foreach (var tile in tiles)
            {
                if (tile == null) continue;
                int stars = GameManager.GetStars(tile.level);
                totalStars += stars;
                tile.Refresh(unlocked, stars);

                int level = tile.level;
                if (tile.button != null) tile.button.onClick.AddListener(() => Open(level));
            }
        }

        if (totalStarsText != null)
            totalStarsText.text = totalStars + " / " + ((tiles != null ? tiles.Length : 0) * 3);

        // Oxirgi ochilgan daraja turgan dunyodan boshlaymiz
        page = Mathf.Clamp((unlocked - 1) / Mathf.Max(1, tilesPerPage), 0, PageCount - 1);
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

        bool multi = PageCount > 1;
        if (prevButton != null) prevButton.gameObject.SetActive(multi);
        if (nextButton != null) nextButton.gameObject.SetActive(multi);
    }

    public void Open(int level)
    {
        string sceneName = string.Format(sceneNameFormat, level);
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("[KichikRobot] '" + sceneName + "' sahnasi yo'q. Menyu: Kichik Robot > 4. Darajalarni JSON'dan qayta yaratish.");
    }

    /// <summary>Sinov uchun: barcha saqlashni o'chiradi.</summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
