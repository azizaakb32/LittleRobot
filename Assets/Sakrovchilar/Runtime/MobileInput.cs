using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Ekran tugmalari (TouchButton) va klaviatura kirishini bir joyda jamlaydi.
/// O'yin boshlanganda o'zi avtomatik yaratiladi, sahnaga qo'lda qo'yish shart emas.
/// Eski Input Manager ham, yangi Input System ham ishlaydi.
/// </summary>
[DefaultExecutionOrder(-100)]
public class MobileInput : MonoBehaviour
{
    public static float Horizontal { get; private set; }
    /// <summary>Yuqoriga +1, pastga -1 (narvon va platformadan tushish uchun).</summary>
    public static float Vertical { get; private set; }
    public static bool JumpPressed { get; private set; }   // shu kadrda bosildi
    public static bool JumpReleased { get; private set; }  // shu kadrda qo'yib yuborildi

    static MobileInput instance;
    static bool leftHeld, rightHeld, upHeld, downHeld;
    static bool jumpDownFlag, jumpUpFlag;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        // O'yin doim gorizontal (landscape) ekranda
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;

        if (instance != null) return;
        var go = new GameObject("MobileInput");
        go.hideFlags = HideFlags.HideInHierarchy;
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MobileInput>();
    }

    // --- UI tugmalari (TouchButton) chaqiradi ---
    public static void SetLeft(bool held) { leftHeld = held; }
    public static void SetRight(bool held) { rightHeld = held; }
    public static void SetUp(bool held) { upHeld = held; }
    public static void SetDown(bool held) { downHeld = held; }
    public static void JumpDown() { jumpDownFlag = true; }
    public static void JumpUp() { jumpUpFlag = true; }

    public static void ResetState()
    {
        leftHeld = false;
        rightHeld = false;
        upHeld = false;
        downHeld = false;
        Vertical = 0f;
        jumpDownFlag = false;
        jumpUpFlag = false;
        Horizontal = 0f;
        JumpPressed = false;
        JumpReleased = false;
    }

    void Update()
    {
        float keyboard = 0f;
        float keyV = 0f;
        bool keyJumpDown = false;
        bool keyJumpUp = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) keyboard -= 1f;
            if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) keyboard += 1f;
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed) keyV += 1f;
            if (kb.downArrowKey.isPressed || kb.sKey.isPressed) keyV -= 1f;
            keyJumpDown = kb.spaceKey.wasPressedThisFrame;
            keyJumpUp = kb.spaceKey.wasReleasedThisFrame;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        keyboard = Input.GetAxisRaw("Horizontal");
        keyV = Input.GetAxisRaw("Vertical");
        keyJumpDown = Input.GetButtonDown("Jump");
        keyJumpUp = Input.GetButtonUp("Jump");
#endif

        float touch = (rightHeld ? 1f : 0f) - (leftHeld ? 1f : 0f);
        Horizontal = Mathf.Abs(touch) > 0.01f ? touch : keyboard;

        float touchV = (upHeld ? 1f : 0f) - (downHeld ? 1f : 0f);
        Vertical = Mathf.Abs(touchV) > 0.01f ? touchV : keyV;

        JumpPressed = jumpDownFlag || keyJumpDown;
        JumpReleased = jumpUpFlag || keyJumpUp;

        jumpDownFlag = false;
        jumpUpFlag = false;
    }
}
