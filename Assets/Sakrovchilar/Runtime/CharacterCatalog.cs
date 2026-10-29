using UnityEngine;

/// <summary>Bitta personaj haqida ma'lumot.</summary>
public class CharacterDef
{
    public readonly string id;
    public readonly string displayName;
    public readonly Color accent;

    public CharacterDef(string id, string displayName, Color accent)
    {
        this.id = id;
        this.displayName = displayName;
        this.accent = accent;
    }
}

/// <summary>
/// Personajlar ro'yxati va tanlangan personaj (PlayerPrefs'da saqlanadi).
/// Kadrlar Resources/Characters/{id}_{kadr}.png fayllaridan yuklanadi:
/// idle, blink, run0..run5, jump, fall, happy.
/// Yangi personaj qo'shish: All ro'yxatiga qo'shing va shu nomli 11 ta kadrni Resources/Characters ga tashlang.
/// </summary>
public static class CharacterCatalog
{
    public static readonly CharacterDef[] All =
    {
        new CharacterDef("robot", "Robot", new Color32(0xFF, 0x8A, 0x3D, 0xFF)),
        new CharacterDef("cat", "Mushuk", new Color32(0xB3, 0x88, 0xFF, 0xFF)),
        new CharacterDef("frog", "Qurbaqa", new Color32(0x7C, 0xE3, 0x8B, 0xFF)),
        new CharacterDef("penguin", "Pingvin", new Color32(0x4D, 0xD0, 0xE1, 0xFF)),
    };

    public const string RunFramePrefix = "run";
    public const int RunFrameCount = 6;

    const string PrefKey = "sakrovchilar_character";

    public static int SelectedIndex
    {
        get { return Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, All.Length - 1); }
        set
        {
            PlayerPrefs.SetInt(PrefKey, Mathf.Clamp(value, 0, All.Length - 1));
            PlayerPrefs.Save();
        }
    }

    public static CharacterDef Selected
    {
        get { return All[SelectedIndex]; }
    }

    public static Sprite Load(CharacterDef character, string frame)
    {
        return Resources.Load<Sprite>("Characters/" + character.id + "_" + frame);
    }
}
