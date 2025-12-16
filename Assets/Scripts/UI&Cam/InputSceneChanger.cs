using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// スペースキーまたは外部コントローラーでシーン遷移を行う汎用スクリプト
/// </summary>
public class InputSceneChanger : MonoBehaviour
{
    [Header("シーン設定")]
    [SerializeField] private string targetSceneName = "main"; // 遷移先シーン名（インスペクターで設定可能）
    
    [Header("入力設定")]
    [SerializeField] private bool useSpaceKey = true; // スペースキーを使用するか
    [SerializeField] private bool useController = true; // コントローラーを使用するか
    [SerializeField] private bool useFadeTransition = true; // フェード遷移を使用するか
    
    [Header("コントローラー設定")]
    [SerializeField] private string controllerButtonName = "Submit"; // コントローラーボタン名（Submit, Jump, Fire1など）
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugLog = false; // デバッグログを表示するか

    private bool canChangeScene = true; // シーン変更可能フラグ

    public int LastPressedButton { get; private set; } = -1; // 最後に押されたボタン番号
    public bool ButtonPressed { get; private set; } = false; // ボタンが押された瞬間

    void Start()
    {
        if (showDebugLog)
        {
            Debug.Log($"InputSceneChanger initialized. Target scene: {targetSceneName}");
        }
    }

    void Update()
    {
        if (!canChangeScene) return;

        bool shouldChangeScene = false;

        ButtonPressed = false;
        LastPressedButton = -1;

        // スペースキー入力チェック
        if (useSpaceKey && Input.GetKeyDown(KeyCode.Space))
        {
            // shouldChangeScene = true;
            if (showDebugLog) Debug.Log("Space key pressed - changing scene");
        }

        // コントローラー入力チェック（旧Input Systemを使用）
        if (useController && Input.GetButtonDown(controllerButtonName))
        {
            // shouldChangeScene = true;
            if (showDebugLog) Debug.Log($"Controller button '{controllerButtonName}' pressed - changing scene");
        }

        // 追加のコントローラーボタンチェック（汎用性を高めるため）
        if (useController)
        {
            // ゲームパッドのAボタン（Xbox）、×ボタン（PlayStation）など
            // if (Input.GetKeyDown(KeyCode.JoystickButton0) || 
            //     Input.GetKeyDown(KeyCode.JoystickButton1) || 
            //     Input.GetKeyDown(KeyCode.JoystickButton2) || 
            //     Input.GetKeyDown(KeyCode.JoystickButton3))
            // {
            //     shouldChangeScene = true;
            //     if (showDebugLog) Debug.Log("Controller button pressed - changing scene");
            // }

            if (Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                Debug.Log("bottunX");
                LastPressedButton = 2;
                ButtonPressed = true;
            }
            else if(Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                Debug.Log("bottunA");
                LastPressedButton = 0;
                ButtonPressed = true;
            }
            else if(Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                Debug.Log("bottunB");
                LastPressedButton = 1;
                ButtonPressed = true;
            }
            else if(Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                Debug.Log("bottunY");
                LastPressedButton = 3;
                ButtonPressed = true;
            }
        }

        // シーン遷移実行
        if (shouldChangeScene)
        {
            ChangeScene();
        }
    }

    /// <summary>
    /// シーン遷移を実行
    /// </summary>
    public void ChangeScene()
    {
        if (!canChangeScene) return;

        canChangeScene = false; // 重複実行防止

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set!");
            canChangeScene = true;
            return;
        }

        if (showDebugLog)
        {
            Debug.Log($"Changing scene to: {targetSceneName}");
        }

        // フェード遷移を使用する場合
        if (useFadeTransition && FadeSceneController.Instance != null)
        {
            FadeSceneController.Instance.StartFadeOutToScene(targetSceneName);
        }
        else
        {
            // 直接シーン遷移
            SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// 外部からシーン名を設定（スクリプトからの呼び出し用）
    /// </summary>
    /// <param name="sceneName">遷移先シーン名</param>
    public void SetTargetScene(string sceneName)
    {
        targetSceneName = sceneName;
        if (showDebugLog)
        {
            Debug.Log($"Target scene changed to: {targetSceneName}");
        }
    }

    /// <summary>
    /// シーン変更の有効/無効を切り替え
    /// </summary>
    /// <param name="enabled">有効にするかどうか</param>
    public void SetSceneChangeEnabled(bool enabled)
    {
        canChangeScene = enabled;
        if (showDebugLog)
        {
            Debug.Log($"Scene change enabled: {enabled}");
        }
    }

    /// <summary>
    /// 即座にシーン遷移（外部呼び出し用）
    /// </summary>
    /// <param name="sceneName">遷移先シーン名</param>
    public void ChangeSceneImmediately(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SetTargetScene(sceneName);
        }
        ChangeScene();
    }

    // Unity Editor用：インスペクターでボタンを表示
    #if UNITY_EDITOR
    [ContextMenu("Test Scene Change")]
    private void TestSceneChange()
    {
        ChangeScene();
    }
    #endif
}