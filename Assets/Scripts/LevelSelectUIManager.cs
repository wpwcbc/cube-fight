using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectSimple : MonoBehaviour
{
    [Header("Assign these 8 Buttons in the Inspector")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;
    public Button level5Button;
    public Button level6Button;
    public Button level7Button;
    public Button level8Button;

    private void Awake()
    {
        if (level1Button != null) level1Button.onClick.AddListener(LoadLevel1);
        if (level2Button != null) level2Button.onClick.AddListener(LoadLevel2);
        if (level3Button != null) level3Button.onClick.AddListener(LoadLevel3);
        if (level4Button != null) level4Button.onClick.AddListener(LoadLevel4);
        if (level5Button != null) level5Button.onClick.AddListener(LoadLevel5);
        if (level6Button != null) level6Button.onClick.AddListener(LoadLevel6);
        if (level7Button != null) level7Button.onClick.AddListener(LoadLevel7);
        if (level8Button != null) level8Button.onClick.AddListener(LoadLevel8);
    }

    private void LoadLevel1() { LoadSceneByName("Level 1"); }
    private void LoadLevel2() { LoadSceneByName("Level 2"); }
    private void LoadLevel3() { LoadSceneByName("Level 3"); }
    private void LoadLevel4() { LoadSceneByName("Level 4"); }
    private void LoadLevel5() { LoadSceneByName("Level 5"); }
    private void LoadLevel6() { LoadSceneByName("Level 6"); }
    private void LoadLevel7() { LoadSceneByName("Level 7"); }
    private void LoadLevel8() { LoadSceneByName("Level 8"); }

    private void LoadSceneByName(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Scene not found in Build Settings: " + sceneName);
        }
    }
}
