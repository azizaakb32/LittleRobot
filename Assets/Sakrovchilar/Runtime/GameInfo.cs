/// <summary>
/// O'yin nomi va monetizatsiya sozlamalari. Nomni o'zgartirish uchun shu yerni tahrirlang,
/// so'ng "Sakrovchilar &gt; 4. Darajalarni JSON'dan qayta yaratish" ni bosing
/// (Player Settings'dagi Product Name ham yangilanadi).
/// </summary>
public static class GameInfo
{
    public const string Title = "Sakrovchilar";
    public const string Subtitle = "O'z personajingni tanla";

    /// <summary>Har dunyoda nechta daraja bor.</summary>
    public const int LevelsPerWorld = 10;
    /// <summary>Keyingi dunyoga o'tish uchun oldingi dunyodan yig'ilishi kerak bo'lgan yulduz (maksimum 30).</summary>
    public const int StarsToUnlockWorld = 20;
    /// <summary>Bitta reklama ko'rilganda beriladigan bonus yulduz.</summary>
    public const int StarsPerAd = 5;
}
