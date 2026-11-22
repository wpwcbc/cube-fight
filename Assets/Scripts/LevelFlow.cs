using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; // <= add this

public class LevelFlow : MonoBehaviour
{
    [Header("When enemies are cleared")]
    public bool openGateOnClear = true;
    public bool loadNextScene = false;
    public string nextSceneName = "";
    public bool asyncLoad = true;
    public float loadDelayAfterGate = 0.2f;

    private bool _handled;

    [Header("Gate (optional)")]
    public GameObject Gate;
    [Min(0f)] public float gateDropDuration = 0.15f;
    public bool disableGateCollidersAfterOpen = true;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera playerVCam; // assign your player camera
    public CinemachineVirtualCamera gateVCam;   // create another vcam for the gate
    public float cameraHoldBeforeOpen = 0.15f;  // small pause after arriving at gate
    public float cameraHoldAfterOpen = 0.15f;   // small pause before blending back
    public float gateFocusYOffset = 0.5f;       // lift the focus a bit
    public Transform gateFocus;                 // optional: assign; else auto-created

    [Header("Exit Trigger")]
    public GridAgent playerAgent;
    public bool exitOnlyAfterGateOpen = true;
    public float exitDelay = 0f;

    private bool _gateOpened;
    private bool _exiting;

    [Header("Player's Death")]
    public PlayerActor playerActor;
    public float reloadDelayTime = 2f;

    void OnEnable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnAllEnemiesCleared += HandleAllCleared;
            EnemyManager.Instance.OnEnemyCountChanged += HandleCountChanged;
        }

        if (playerAgent != null)
        {
            playerAgent.onMoved.AddListener(OnPlayerMoved);
        }

        if (playerActor != null)
        {
            playerActor.onPlayerDied.AddListener(OnPlayerDied);
        }
    }

    void OnDisable()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnAllEnemiesCleared -= HandleAllCleared;
            EnemyManager.Instance.OnEnemyCountChanged -= HandleCountChanged;
        }

        if (playerAgent != null)
        {
            playerAgent.onMoved.RemoveListener(OnPlayerMoved);
        }

        if (playerActor != null)
        {
            playerActor.onPlayerDied.RemoveListener(OnPlayerDied);
        }
    }

    void Start()
    {
        if (ScreenFader.Instance)
        {
            ScreenFader.Instance.FadeIn(0.5f);
        }
    }

    void HandleCountChanged(int count)
    {
        // update HUD if needed
    }

    void HandleAllCleared()
    {
        if (_handled) return;
        _handled = true;
        StartCoroutine(HandleClearSequence());
    }

    IEnumerator HandleClearSequence()
    {
        if (openGateOnClear && Gate != null)
        {
            // 1) move camera to gate, 2) open, 3) move camera back
            yield return StartCoroutine(FocusGateThenOpenThenRefocusPlayer());
        }
    }

    // --- CAMERA SEQUENCE ---

    private IEnumerator FocusGateThenOpenThenRefocusPlayer()
    {
        if (playerVCam == null || gateVCam == null)
        {
            yield return StartCoroutine(OpenGate());
            yield break;
        }

        CinemachineBrain brain = Camera.main != null ? Camera.main.GetComponent<CinemachineBrain>() : null;

        // Ensure gate focus exists and assign it
        Transform focus = EnsureGateFocus();
        gateVCam.Follow = focus;
        gateVCam.LookAt = focus;

        // Cache originals
        int playerPriorOriginal = playerVCam.Priority;
        int gatePriorOriginal = gateVCam.Priority;

        // Use a clear "boost" band above any existing priorities
        int boost = Mathf.Max(playerPriorOriginal, gatePriorOriginal) + 10;

        // --- Blend TO gate ---
        gateVCam.Priority = boost;
        playerVCam.Priority = playerPriorOriginal; // keep player at original (lower than boost)

        // Wait for blend
        yield return null;
        if (brain != null) { while (brain.IsBlending) yield return null; }

        if (cameraHoldBeforeOpen > 0f) yield return new WaitForSeconds(cameraHoldBeforeOpen);

        // Open
        yield return StartCoroutine(OpenGate());

        if (cameraHoldAfterOpen > 0f) yield return new WaitForSeconds(cameraHoldAfterOpen);

        // --- Blend BACK to player ---
        playerVCam.Priority = boost;        // force player to win
        gateVCam.Priority = gatePriorOriginal; // drop gate back to its original

        // Wait for blend
        yield return null;
        if (brain != null) { while (brain.IsBlending) yield return null; }

        // Restore player's original priority (optional but tidy)
        playerVCam.Priority = playerPriorOriginal;
    }

    private Transform EnsureGateFocus()
    {
        if (gateFocus != null) return gateFocus;

        // Compute bounds from renderers under Gate
        Renderer[] rens = Gate.GetComponentsInChildren<Renderer>(false);
        Vector3 center;
        if (rens.Length > 0)
        {
            Bounds b = rens[0].bounds;
            for (int i = 1; i < rens.Length; i++) b.Encapsulate(rens[i].bounds);
            center = b.center;
        }
        else
        {
            center = Gate.transform.position;
        }

        GameObject child = new GameObject("GateFocus");
        child.transform.SetParent(Gate.transform);
        child.transform.position = new Vector3(center.x, center.y + gateFocusYOffset, center.z);
        gateFocus = child.transform;
        return gateFocus;
    }

    #region --- GATE OPEN ---

    private IEnumerator OpenGate()
    {
        GridBlocker[] gateBlocksArray = Gate.GetComponentsInChildren<GridBlocker>(false);
        int count = gateBlocksArray.Length;
        if (count == 0) yield break;

        float drop = GridSettings.cellSize * 0.95f;
        float duration = Mathf.Max(0.01f, gateDropDuration);

        List<Coroutine> running = new List<Coroutine>(count);
        for (int i = 0; i < count; i++)
        {
            Transform t = gateBlocksArray[i].transform;
            Coroutine c = StartCoroutine(DropOneCell(t, drop, duration));
            running.Add(c);
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < count; i++)
            gateBlocksArray[i].SetAsNonBlock();

        if (disableGateCollidersAfterOpen)
        {
            Collider[] cols = Gate.GetComponentsInChildren<Collider>(false);
            for (int i = 0; i < cols.Length; i++)
                cols[i].enabled = false;
        }

        _gateOpened = true;
    }

    private IEnumerator DropOneCell(Transform target, float dropDistance, float duration)
    {
        Vector3 start = target.position;
        Vector3 end = new Vector3(start.x, start.y - dropDistance, start.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float a = Mathf.Clamp01(t);
            target.position = Vector3.Lerp(start, end, a);
            yield return null;
        }

        target.position = end;
    }
    #endregion

    #region Exit Trigger

    private void OnPlayerMoved(Vector2Int cell)
    {
        if (_exiting) return;
        if (TriggerGrid.Instance == null) return;
        if (exitOnlyAfterGateOpen && !_gateOpened) return;

        if (TriggerGrid.Instance.IsExit(cell))
        {
            _exiting = true;
            StartCoroutine(ExitSequence());
        }
    }

    private IEnumerator ExitSequence()
    {
        if (exitDelay > 0f) yield return new WaitForSeconds(exitDelay);

        // Fade to black
        if (ScreenFader.Instance) yield return ScreenFader.Instance.FadeOut(0.35f);

        // Queue the fade-in for the next scene before we activate it
        ScreenFader.QueueFadeInOnNextScene(0.35f);

        // Begin async load and hold at 0.9 until we allow activation
        AsyncOperation op;
        if (!string.IsNullOrEmpty(nextSceneName))
            op = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        else
            op = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);

        op.allowSceneActivation = false;

        // Wait until background load reaches the 0.9f barrier
        while (op.progress < 0.9f) yield return null;                     // Unity stalls at ~0.9 until activation :contentReference[oaicite:4]{index=4}

        // Switch scenes while black; ScreenFader will auto-fade-in on sceneLoaded
        op.allowSceneActivation = true;
    }

    #endregion

    #region Player Death
    public void OnPlayerDied()
    {
        // e.g., show Game Over UI, then reload
        StartCoroutine(ReloadAfter(reloadDelayTime));
    }

    IEnumerator ReloadAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        var idx = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(idx);
    }
    #endregion
}
