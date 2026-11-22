using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class ReturnToStartButton : MonoBehaviour
{
    [SerializeField] private string startSceneName = "Start Screen";

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("[ReturnToStartButton] Button component missing.");
        }
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(GoBackToStart);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(GoBackToStart);
        }
    }

    public void GoBackToStart()
    {
        if (Application.CanStreamedLevelBeLoaded(startSceneName))
        {
            SceneManager.LoadScene(startSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Start scene not found in Build Settings: " + startSceneName);
        }
    }
}
