using System;
using UnityEngine;

/// <summary>
/// Rewarded reklama. Haqiqiy reklamani ulash (masalan Google AdMob):
/// SDK'ni o'rnatib, o'yin boshida (masalan, bitta MonoBehaviour.Awake ichida)
///   AdService.RewardedHandler = done => { ...reklamani ko'rsat...; done(true/false); };
/// deb yozing. Reklama oxirigacha ko'rilsa done(true), aks holda done(false).
/// Handler berilmagan bo'lsa: Editor va Development build'da reklama ko'rilgandek hisoblanadi (sinov uchun),
/// release build'da reklama yo'q deb javob qaytadi.
/// </summary>
public static class AdService
{
    public static Action<Action<bool>> RewardedHandler;

    public static bool IsAvailable
    {
        get { return RewardedHandler != null || SimulateAllowed; }
    }

    static bool SimulateAllowed
    {
        get { return Application.isEditor || Debug.isDebugBuild; }
    }

    public static void ShowRewarded(Action<bool> done)
    {
        if (RewardedHandler != null) { RewardedHandler(done); return; }
        if (SimulateAllowed)
        {
            Debug.Log("[Sakrovchilar] Sinov rejimi: rewarded reklama ko'rildi deb hisoblandi.");
            done(true);
            return;
        }
        done(false);
    }
}

/// <summary>
/// Xaridlar. Haqiqiy do'konni ulash (Unity IAP / Google Play Billing):
///   StoreService.PurchaseHandler = (productId, done) => { ...xaridni boshla...; done(true/false); };
///   StoreService.PriceHandler = productId => "$1.99";   // do'kondan kelgan narx (ixtiyoriy)
/// Mahsulot ID'lari: "unlock_world_1" ... "unlock_world_9" (Play Console'da shu nomlar bilan yarating).
/// Handler yo'q bo'lsa: Editor va Development build'da xarid muvaffaqiyatli deb hisoblanadi.
/// </summary>
public static class StoreService
{
    public static Action<string, Action<bool>> PurchaseHandler;
    public static Func<string, string> PriceHandler;

    public static string ProductForWorld(int world) { return "unlock_world_" + world; }

    public static string PriceText(string productId)
    {
        if (PriceHandler != null)
        {
            string p = PriceHandler(productId);
            if (!string.IsNullOrEmpty(p)) return p;
        }
        return "Sotib olish";
    }

    public static void Purchase(string productId, Action<bool> done)
    {
        if (PurchaseHandler != null) { PurchaseHandler(productId, done); return; }
        if (Application.isEditor || Debug.isDebugBuild)
        {
            Debug.Log("[Sakrovchilar] Sinov rejimi: '" + productId + "' xarid qilindi deb hisoblandi.");
            done(true);
            return;
        }
        done(false);
    }
}
