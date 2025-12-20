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
    public string controllerName { get; private set; } = "aaa";

    public bool flag = false;

    PanelController pc;

    void Start()
    {
        if (showDebugLog)
        {
            Debug.Log($"InputSceneChanger initialized. Target scene: {targetSceneName}");
        }

        string[] controllers = Input.GetJoystickNames();
        foreach (string c in controllers)
        {
            Debug.Log("接続中コントローラー: " + c);
        }

        if (controllers.Length > 0)
        {
            controllerName = controllers[0];
            Globaldata.controllerType = controllerName;
            Debug.Log(controllerName);

            if (controllerName.ToLower().Contains("Sony") || controllerName.ToLower().Contains("dualshock"))
            {
                //controllerName = "Sony";
                Debug.Log("PlayStation系コントローラーが接続されています");
            }
            else if (controllerName.ToLower().Contains("Logi") || controllerName.ToLower().Contains("f310") || controllerName.ToLower().Contains("f710"))
            {
                //controllerName = "Logi";
                Debug.Log("Logicool(ロジクール)コントローラーが接続されています");
            }
            else
            {
                Debug.Log("その他のコントローラーが接続されています");
            }
        }
        else
        {
            Debug.Log("コントローラーは接続されていません");
        }

        pc = FindObjectOfType<PanelController>();
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
            // 現在のシーン名を取得
            string currentSceneName = SceneManager.GetActiveScene().name;
            if(currentSceneName == "Title" || currentSceneName == "result")
            {
                // ゲームパッドのAボタン（Xbox）、×ボタン（PlayStation）など
                if (Input.GetKeyDown(KeyCode.JoystickButton0) || 
                    Input.GetKeyDown(KeyCode.JoystickButton1) || 
                    Input.GetKeyDown(KeyCode.JoystickButton2) || 
                    Input.GetKeyDown(KeyCode.JoystickButton3))
                {
                    shouldChangeScene = true;
                    if (currentSceneName == "result")
                    {
                        GameData.hinan = false;
                        GameData.ChildGoal = false;
                        GameData.ParentGoal = false;
                    }
                    if (showDebugLog) Debug.Log("Controller button pressed - changing scene");
                }
            }

            if(currentSceneName == "SelectPath" && pc.flag)
            {
                if(Input.GetKeyDown(KeyCode.JoystickButton3))
                {
                    pc.selectY();
                }
                else if(Input.GetKeyDown(KeyCode.JoystickButton2))
                {
                    pc.selectB();
                }
                else if(Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    pc.selectX();
                }
            }
            

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