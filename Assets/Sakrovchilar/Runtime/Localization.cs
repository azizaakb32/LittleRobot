using System;
using System.Collections.Generic;
using UnityEngine;

public enum Lang { Uz = 0, En = 1, Ru = 2 }

/// <summary>
/// 3 tilli matnlar: o'zbek, ingliz, rus. Tanlangan til PlayerPrefs'da saqlanadi,
/// birinchi ishga tushganda telefon tiliga qarab tanlanadi (rus / o'zbek, qolganlari ingliz).
/// Yangi matn qo'shish: Add("kalit", "o'zbekcha", "English", "Русский") qatorini qo'shing.
/// Matnni olish: Localization.T("kalit") yoki Localization.T("kalit", arg0, arg1).
/// </summary>
public static class Localization
{
    const string PrefKey = "sakrovchilar_lang";
    static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>();
    static int current = -1;

    /// <summary>Til almashganda chaqiriladi.</summary>
    public static event Action Changed;

    public static Lang Current
    {
        get
        {
            if (current < 0) current = LoadInitial();
            return (Lang)current;
        }
        set
        {
            current = (int)value;
            PlayerPrefs.SetInt(PrefKey, current);
            PlayerPrefs.Save();
            if (Changed != null) Changed();
        }
    }

    /// <summary>Tugmada ko'rsatiladigan qisqa kod: UZ / EN / RU.</summary>
    public static string Code
    {
        get
        {
            switch (Current)
            {
                case Lang.Uz: return "UZ";
                case Lang.Ru: return "RU";
                default: return "EN";
            }
        }
    }

    public static void Next()
    {
        Current = (Lang)(((int)Current + 1) % 3);
    }

    static int LoadInitial()
    {
        if (PlayerPrefs.HasKey(PrefKey)) return Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, 2);
        if (Application.systemLanguage == SystemLanguage.Russian) return (int)Lang.Ru;
        if (Application.systemLanguage == SystemLanguage.English) return (int)Lang.En;
        string two = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        if (two == "uz") return (int)Lang.Uz;
        if (two == "ru") return (int)Lang.Ru;
        return (int)Lang.En;
    }

    public static string T(string key)
    {
        string[] v;
        if (!table.TryGetValue(key, out v)) return key;
        string s = v[(int)Current];
        return string.IsNullOrEmpty(s) ? v[(int)Lang.En] : s;
    }

    public static string T(string key, params object[] args)
    {
        string s = T(key);
        try { return string.Format(s, args); }
        catch (FormatException) { return s; }
    }

    static void Add(string key, string uz, string en, string ru)
    {
        table[key] = new[] { uz, en, ru };
    }

    static Localization()
    {
        // O'yin ichida
        Add("ui.pause", "Pauza", "Pause", "Пауза");
        Add("ui.resume", "Davom etish", "Resume", "Продолжить");
        Add("ui.restart", "Qayta boshlash", "Restart", "Заново");
        Add("ui.next", "Keyingi", "Next", "Далее");
        Add("ui.play", "O'ynash", "Play", "Играть");
        Add("ui.levelDone", "Daraja tugadi!", "Level complete!", "Уровень пройден!");
        Add("ui.levelDoneSmall", "Daraja tugadi", "Level complete", "Уровень пройден");
        Add("ui.timer", "{0} s", "{0} s", "{0} с");
        Add("ui.win.info", "Tangalar: {0}/{1}\nVaqt: {2} s", "Coins: {0}/{1}\nTime: {2} s", "Монеты: {0}/{1}\nВремя: {2} с");
        Add("ui.win.nextWorld", "Keyingi dunyo: {0}/{1} yulduz", "Next world: {0}/{1} stars", "Следующий мир: {0}/{1} звёзд");
        Add("ui.reward2x", "Tangalarni 2 barobar qilish (reklama)", "Double the coins (watch an ad)", "Удвоить монеты (реклама)");

        // Personaj tanlash
        Add("ui.chooseHero", "O'z personajingni tanla", "Choose your hero", "Выбери героя");
        Add("char.robot", "Robot", "Robot", "Робот");
        Add("char.cat", "Mushuk", "Cat", "Кот");
        Add("char.frog", "Qurbaqa", "Frog", "Лягушка");
        Add("char.penguin", "Pingvin", "Penguin", "Пингвин");

        // Dunyolar
        Add("world.0", "Zavod", "Factory", "Завод");
        Add("world.1", "O'rmon", "Forest", "Лес");
        Add("world.2", "Cho'l", "Desert", "Пустыня");
        Add("world.3", "Muz", "Ice", "Лёд");
        Add("world.4", "Kosmos", "Space", "Космос");
        Add("world.5", "Vulqon", "Volcano", "Вулкан");
        Add("world.6", "Okean", "Ocean", "Океан");
        Add("world.7", "Bulutlar", "Clouds", "Облака");
        Add("world.8", "Shahar", "City", "Город");
        Add("world.9", "Sehrli", "Magic", "Волшебство");

        // Yulduz darvozasi
        Add("ui.gate.title", "{0} qulflangan", "{0} is locked", "Мир «{0}» закрыт");
        Add("ui.gate.info", "Ochish uchun oldingi dunyodan {0} yulduz kerak.\nSizda: {1} / {0}",
            "You need {0} stars from the previous world.\nYou have: {1} / {0}",
            "Нужно {0} звёзд из предыдущего мира.\nУ вас: {1} / {0}");
        Add("ui.gate.missing", "\nYetishmayapti: {0}", "\nMissing: {0}", "\nНе хватает: {0}");
        Add("ui.gate.ad", "Reklama ko'r (+{0} yulduz)", "Watch an ad (+{0} stars)", "Смотреть рекламу (+{0} звёзд)");
        Add("ui.gate.buy", "Darhol ochish: {0}", "Unlock now: {0}", "Открыть сразу: {0}");
        Add("ui.gate.banner", "Qulf: {0}/{1} yulduz. Ochish", "Locked: {0}/{1} stars. Unlock", "Закрыто: {0}/{1} звёзд. Открыть");
        Add("ui.store.buy", "Sotib olish", "Buy", "Купить");
        Add("toast.noAd", "Reklama hozir mavjud emas. Keyinroq urinib ko'ring.", "No ad available right now. Try again later.", "Реклама сейчас недоступна. Попробуйте позже.");
        Add("toast.buyFail", "Xarid amalga oshmadi.", "Purchase failed.", "Покупка не удалась.");
        Add("toast.unlocked", "Yangi dunyo ochildi!", "New world unlocked!", "Новый мир открыт!");
    }
}
