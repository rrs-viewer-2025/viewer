using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerCharaControl : MonoBehaviour
{
    public Timer timerScript; //Timerスクリプトをアタッチする
    public float forwardSpeed = 5.0f;//前進速度
    public float rotationSpeed = 100.0f;//回転速度
    public GameObject Info_end; // ゴールオブジェクト
    private Animator anim;
    private bool runFlag;
    Rigidbody rb;

    //private bool RefugeOn; //避難所に到達したかを管理する

    // コントローラの入力値
    private float v;
    private float h;
    private List<Joycon> joycons; // Joy-Conのリスト
    private Joycon joycon;        // 今使うJoy-Con（片方
    private bool left = false;

    // Setting.csのInterfaceTypeを参照して共有
    private Setting settingScript;
    private string interfaceType;
    JoyconManager joyconManager;

    PlayerTrail pt;

    void Awake()
    {
        // JoyconManagerを取得
        joyconManager = FindFirstObjectByType<JoyconManager>();
        pt = FindFirstObjectByType<PlayerTrail>();
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Setting.csのInterfaceTypeを参照
        settingScript = FindFirstObjectByType<Setting>();
        if (settingScript != null)
        {
            interfaceType = settingScript.InterfaceType;
            Debug.Log($"[PlayerCharaControl] interface (from Setting): {interfaceType}");
        }
        else
        {
            interfaceType = "key";
            Debug.LogWarning("[PlayerCharaControl] Setting.cs not found, defaulting to 'key'");
        }

        // Joy-Conの接続・初期化処理を関数化
        SetupJoycon();
    }

    /// <summary>
    /// Joy-Conの接続と初期化を行う関数。
    /// </summary>
    void SetupJoycon()
    {
        // JoyconManager から Joy-Con のリストを取得
        joycons = JoyconManager.Instance.j;

        // 少なくとも1つJoy-Conが接続されていたら使う
        if (joycons.Count > 0)
        {
            joycon = joycons[0]; // 0番目のJoy-Conを使用（通常は左）
            Debug.Log("Joy-Con接続成功");
        }
        else
        {
            Debug.LogWarning("Joy-Conが接続されていません");
        }
    }

    void Update()
    {
        // 入力方法に応じて処理分岐
        switch (interfaceType)
        {
            case "key":
                key();
                break;
            case "mat":
                Mat();
                break;
            case "pad":
                pad();
                break;
        }

        // Joy-Con入力がある場合は優先して処理
        if (joycon != null)
        {
            HandleJoyconInput();
        }

        // 共通処理（Runアニメーションと移動・回転）
        if (Mathf.Abs(v) > 0.1f || Mathf.Abs(h) > 0.1f)
        {
            runFlag = true;
        }
        else
        {
            runFlag = false;
        }

        anim.SetBool("Run", runFlag); // 入力値に応じて「走る」アニメーションを切り替え
        transform.position += transform.forward * forwardSpeed * v * Time.deltaTime; // 前進・後退の移動処理
        transform.Rotate(0, rotationSpeed * h * Time.deltaTime, 0); // 左右の回転処理
    }

    /// <summary>
    /// Joy-Con入力がある場合の移動・回転値の更新処理。
    /// </summary>
    void HandleJoyconInput()
    {
        var stick = joycon.GetStick();
        left = joyconManager.getLeftRight();

        if (left)
        {
            v = stick[0];
            h = -stick[1];
        }
        else
        {
            v = -stick[0];
            h = stick[1];
        }
    }

    /// <summary>
    /// キーボード入力によるプレイヤーの移動・ジャンプ処理。
    /// 縦横の入力値を取得し、スペースキーでジャンプアニメーションと物理ジャンプを実行。
    /// </summary>
    void key()
    {
        v = Input.GetAxis("Vertical");
        h = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Jump");
            rb.AddForce(transform.up * 1000 * 8, ForceMode.Force);
        }
    }

    /// <summary>
    /// MatActionオブジェクトからの入力によるプレイヤーの移動処理。
    /// Up/Down/Left/Rightの値に応じて移動方向を決定。
    /// </summary>
    void Mat()
    {
        MatAction mat = FindFirstObjectByType<MatAction>();

        if (mat != null)
        {
            v = 0.0f;
            h = 0.0f;
            if (mat.Up > 0) v = 1.0f;
            if (mat.Down > 0) v = -1.0f;
            if (mat.Left > 0) h = -1.0f;
            if (mat.Right > 0) h = 1.0f;
        }
    }

    /// <summary>
    /// ゲームパッドからの入力によるプレイヤーの移動処理。
    /// 縦横の入力値を取得し、移動方向を決定。
    /// </summary>
    void pad()
    {
        // ゲームパッド（デバイスの取得）
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            v = gamepad.leftStick.y.ReadValue();
            h = gamepad.leftStick.x.ReadValue();
        } else if (gamepad == null) {
            Debug.LogWarning("Gamepad not connected");
        }
    }

    // 歩くアニメーションのメソッド
    public void OnFootstep()
    {
        // Debug.Log("Footstep!");
        // AudioSource.PlayClipAtPoint(footstepClip, transform.position); なども可
    }

    // Goalシーンに遷移するメソッド
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Refuge")
        {
            Globaldata.playerposi = pt.getPlayerPosiList();
            if (timerScript != null)
            {
                //クリア時間取得
                GameData.clearTime = timerScript.LimitTime - timerScript.timeRemaining;
                GameData.hinan = true; //避難成功を格納
                timerScript.StopTimer(); //タイマーを停止させる
            }
            Info_end.SetActive(true);
            Invoke("LoadGoalScene", 5f); // 5秒後にシーン遷移
        }
    }

    // コルーチンのため別メソッド
    void LoadGoalScene()
    {
        SceneManager.LoadScene("result");
    }
}
