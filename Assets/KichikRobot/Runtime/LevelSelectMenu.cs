using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Daraja tanlash ekrani. Katakchalar ochiq/qulflangan holatini PlayerPrefs'dan oladi.
/// Darajalar sahnalari "Level1", "Level2", ... nomi bilan Build Settings'da bo'lishi kerak.
/// </summary>
public class LevelSelectMenu : MonoBehaviour
{
    public LevelTile[] tiles;
    public Text totalStarsText;
    public string sceneNameFormat = "Level{0}";

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
    }

    public void Open(int level)
    {
        string sceneName = string.Format(sceneNameFormat, level);
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("[KichikRobot] '" + sceneName + "' sahnasi yo'q. Menyu: Kichik Robot > Yangi daraja qo'shish.");
    }

    /// <summary>Sinov uchun: barcha saqlashni o'chiradi.</summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
