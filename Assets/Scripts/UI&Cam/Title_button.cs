using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    public string targetSceneName = "intro"; // インスペクタで設定可

    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        // フェード付きでシーン遷移
        if (FadeSceneController.Instance != null)
        {
            FadeSceneController.Instance.StartFadeOutToScene(targetSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}
