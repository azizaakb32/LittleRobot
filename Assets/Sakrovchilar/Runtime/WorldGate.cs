using UnityEngine;

/// <summary>
/// Dunyolar orasidagi "yulduz darvozasi": keyingi 10 talik ochilishi uchun oldingi 10 ta darajadan
/// GameInfo.StarsToUnlockWorld yulduz kerak. Yetmasa: rewarded reklama ko'rib bonus yulduz olish
/// yoki sotib olib ochish mumkin.
/// </summary>
public static class WorldGate
{
    public static int WorldOf(int level)
    {
        return Mathf.Max(0, (level - 1) / GameInfo.LevelsPerWorld);
    }

    public static int FirstLevelOf(int world)
    {
        return world * GameInfo.LevelsPerWorld + 1;
    }

    /// <summary>Oldingi dunyodan haqiqatda yig'ilgan yulduzlar.</summary>
    public static int EarnedStars(int world)
    {
        if (world <= 0) return 0;
        int sum = 0;
        int first = FirstLevelOf(world - 1);
        for (int i = 0; i < GameInfo.LevelsPerWorld; i++) sum += GameManager.GetStars(first + i);
        return sum;
    }

    public static int BonusStars(int world)
    {
        return PlayerPrefs.GetInt("bonus_stars_" + world, 0);
    }

    /// <summary>Yig'ilgan + reklama orqali olingan yulduzlar.</summary>
    public static int CurrentStars(int world)
    {
        return EarnedStars(world) + BonusStars(world);
    }

    public static int Required { get { return GameInfo.StarsToUnlockWorld; } }

    public static bool IsUnlocked(int world)
    {
        if (world <= 0) return true;
        if (PlayerPrefs.GetInt("world_unlocked_" + world, 0) == 1) return true;
        if (CurrentStars(world) >= Required && PreviousWorldFinished(world))
        {
            SetUnlocked(world);
            return true;
        }
        return false;
    }

    public static bool IsLevelUnlocked(int level)
    {
        return IsUnlocked(WorldOf(level));
    }

    /// <summary>Oldingi dunyoning oxirgi darajasi o'tilganmi (keyingi dunyoning 1-darajasi ochilganmi).</summary>
    public static bool PreviousWorldFinished(int world)
    {
        return GameManager.GetUnlockedLevel() >= FirstLevelOf(world);
    }

    static void SetUnlocked(int world)
    {
        PlayerPrefs.SetInt("world_unlocked_" + world, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Reklama ko'rilgandan keyin bonus yulduz qo'shadi.</summary>
    public static void AddAdStars(int world)
    {
        PlayerPrefs.SetInt("bonus_stars_" + world, BonusStars(world) + GameInfo.StarsPerAd);
        PlayerPrefs.Save();
    }

    /// <summary>Sotib olinganda dunyoni to'g'ridan-to'g'ri ochadi.</summary>
    public static void UnlockByPurchase(int world)
    {
        SetUnlocked(world);
        if (GameManager.GetUnlockedLevel() < FirstLevelOf(world))
        {
            PlayerPrefs.SetInt("unlocked_level", FirstLevelOf(world));
            PlayerPrefs.Save();
        }
    }
}
