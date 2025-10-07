using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// 設定ファイルからのインターフェース設定に応じてシーン遷移を行う汎用スクリプト
/// Setting.csと連携してconfig.jsonの設定を参照
/// </summary>
public class InputSceneChanger : MonoBehaviour
{
    [Header("シーン設定")]
    [SerializeField] private string targetSceneName = "main"; // 遷移先シーン名（インスペクターで設定可能）
    
    [Header("遷移設定")]
    [SerializeField] private bool useFadeTransition = true; // フェード遷移を使用するか
    
    [Header("デバッグ")]
    [SerializeField] private bool showDebugLog = false; // デバッグログを表示するか

    private bool canChangeScene = true; // シーン変更可能フラグ
    
    // Setting.csのInterfaceTypeを参照して入力方法を決定
    private Setting settingScript;
    private string interfaceType;
    
    // Joy-Con関連
    private List<Joycon> joycons;
    private Joycon joycon;
    private JoyconManager joyconManager;

    void Awake()
    {
        // JoyconManagerを取得
        joyconManager = FindFirstObjectByType<JoyconManager>();
    }

    void Start()
    {
        // Setting.csのInterfaceTypeを参照
        settingScript = FindFirstObjectByType<Setting>();
        if (settingScript != null)
        {
            interfaceType = settingScript.InterfaceType;
            Debug.Log($"[InputSceneChanger] interface (from Setting): {interfaceType}");
        }
        else
        {
            interfaceType = "key";
            Debug.LogWarning("[InputSceneChanger] Setting.cs not found, defaulting to 'key'");
        }

        // Joy-Conの接続・初期化処理
        SetupJoycon();

        if (showDebugLog)
        {
            Debug.Log($"InputSceneChanger initialized. Target scene: {targetSceneName}, Interface: {interfaceType}");
        }
    }

    /// <summary>
    /// Joy-Conの接続と初期化を行う関数。
    /// </summary>
    void SetupJoycon()
    {
        if (JoyconManager.Instance != null)
        {
            // JoyconManager から Joy-Con のリストを取得
            joycons = JoyconManager.Instance.j;

            // 少なくとも1つJoy-Conが接続されていたら使う
            if (joycons.Count > 0)
            {
                joycon = joycons[0]; // 0番目のJoy-Conを使用（通常は左）
                Debug.Log("[InputSceneChanger] Joy-Con接続成功");
            }
            else
            {
                Debug.LogWarning("[InputSceneChanger] Joy-Conが接続されていません");
            }
        }
    }

    void Update()
    {
        if (!canChangeScene) return;

        bool shouldChangeScene = false;

        // 入力方法に応じて処理分岐
        switch (interfaceType)
        {
            case "key":
                shouldChangeScene = CheckKeyInput();
                break;
            case "mat":
                shouldChangeScene = CheckMatInput();
                break;
            case "pad":
                shouldChangeScene = CheckPadInput();
                break;
        }

        // Joy-Con入力がある場合は優先して処理
        if (joycon != null)
        {
            shouldChangeScene = shouldChangeScene || CheckJoyconInput();
        }

        // シーン遷移実行
        if (shouldChangeScene)
        {
            ChangeScene();
        }
    }

    /// <summary>
    /// キーボード入力によるシーン遷移チェック。
    /// スペースキーまたはエンターキーでシーン遷移。
    /// </summary>
    bool CheckKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (showDebugLog) Debug.Log("Key pressed - changing scene");
            return true;
        }
        return false;
    }

    /// <summary>
    /// MatActionからの入力によるシーン遷移チェック。
    /// マット上での任意の入力でシーン遷移。
    /// </summary>
    bool CheckMatInput()
    {
        MatAction mat = FindFirstObjectByType<MatAction>();
        if (mat != null)
        {
            // マット上で何らかの入力があればシーン遷移
            if (mat.Up > 0 || mat.Down > 0 || mat.Left > 0 || mat.Right > 0)
            {
                if (showDebugLog) Debug.Log("Mat input detected - changing scene");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ゲームパッドからの入力によるシーン遷移チェック。
    /// New Input Systemを使用してボタン入力を検出。
    /// </summary>
    bool CheckPadInput()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            // 各種ボタンの入力チェック
            if (gamepad.buttonSouth.wasPressedThisFrame ||      // A ボタン (Xbox) / × ボタン (PlayStation)
                gamepad.buttonEast.wasPressedThisFrame ||       // B ボタン (Xbox) / ○ ボタン (PlayStation)
                gamepad.buttonWest.wasPressedThisFrame ||       // X ボタン (Xbox) / □ ボタン (PlayStation)
                gamepad.buttonNorth.wasPressedThisFrame ||      // Y ボタン (Xbox) / △ ボタン (PlayStation)
                gamepad.startButton.wasPressedThisFrame ||      // Start ボタン
                gamepad.selectButton.wasPressedThisFrame)       // Select/Back ボタン
            {
                if (showDebugLog) Debug.Log("Gamepad button pressed - changing scene");
                return true;
            }
        }
        else if (gamepad == null && showDebugLog)
        {
            Debug.LogWarning("Gamepad not connected");
        }
        return false;
    }

    /// <summary>
    /// Joy-Con入力によるシーン遷移チェック。
    /// Joy-Conのボタン入力を検出。
    /// </summary>
    bool CheckJoyconInput()
    {
        if (joycon != null)
        {
            // Joy-Conの各種ボタンをチェック
            if (joycon.GetButtonDown(Joycon.Button.DPAD_DOWN) ||
                joycon.GetButtonDown(Joycon.Button.DPAD_UP) ||
                joycon.GetButtonDown(Joycon.Button.DPAD_LEFT) ||
                joycon.GetButtonDown(Joycon.Button.DPAD_RIGHT) ||
                joycon.GetButtonDown(Joycon.Button.SHOULDER_1) ||
                joycon.GetButtonDown(Joycon.Button.SHOULDER_2) ||
                joycon.GetButtonDown(Joycon.Button.SR) ||
                joycon.GetButtonDown(Joycon.Button.SL))
            {
                if (showDebugLog) Debug.Log("Joy-Con button pressed - changing scene");
                return true;
            }
        }
        return false;
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