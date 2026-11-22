using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] private CanvasGroup group;

    // queue a fade-in for the next scene activation
    static bool _pendingFadeIn;
    static float _pendingDuration;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);                               // persist across scenes (DDOL) :contentReference[oaicite:1]{index=1}
        if (!group) group = GetComponent<CanvasGroup>();
        group.alpha = Mathf.Clamp01(group.alpha);                    // keep current alpha (e.g., 1 after FadeOut)
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;   // subscribe to scene load event :contentReference[oaicite:2]{index=2}
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pendingFadeIn)
        {
            _pendingFadeIn = false;
            StartCoroutine(FadeInAfterOneFrame(_pendingDuration));
        }
    }

    IEnumerator FadeInAfterOneFrame(float seconds)
    {
        // ensure at least one fully-black frame shows, then start lerp
        yield return null;
        yield return new WaitForEndOfFrame();                        // run just before display of the next frame :contentReference[oaicite:3]{index=3}
        yield return FadeTo(0f, seconds);
    }

    public static void QueueFadeInOnNextScene(float seconds)
    {
        _pendingFadeIn = true;
        _pendingDuration = Mathf.Max(0.0001f, seconds);
    }

    public Coroutine FadeOut(float seconds) => StartCoroutine(FadeTo(1f, seconds));
    public Coroutine FadeIn(float seconds) => StartCoroutine(FadeTo(0f, seconds));

    IEnumerator FadeTo(float target, float seconds)
    {
        float start = group.alpha;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, seconds);
        group.blocksRaycasts = true;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;                        // ignore timeScale
            group.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        group.alpha = target;
        group.blocksRaycasts = target > 0.001f;                       // block clicks only while visibly black
    }
}
