using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCharaControl : MonoBehaviour
{
    public Timer timerScript; //Timerスクリプトをアタッチする
    public float forwardSpeed = 5.0f;//前進速度
    public float rotationSpeed = 100.0f;//回転速度
    public GameObject Info_end; // ゴールオブジェクト
    public GameObject Info_collide;   // 瓦礫衝突時の UI
    public GameObject Info_timeover;
    private int collideCount = 0;   // 接触している瓦礫の数
    private Coroutine blinkCoroutine;
    private bool isBlinking = false;
    public float blinkInterval = 1.0f; // 点滅間隔
    [Header("HP Settings")]
    public Slider hpSlider;
    public float maxHP = 100f;
    public float currentHP;
    public float damagePerSecond = 1f; // 瓦礫に触れている間の毎秒ダメージ
    private bool isTouchingDebris = false;
    private float debrisTouchTime = 0f;   // 瓦礫に触れている時間
    public float requireTouchTime = 1f;   // ダメージ開始までの待ち時間（秒）
    private bool debrisDamageActive = false; // ダメージ発生中かどうか
    private float damageTimer = 0f;
    public float damageInterval = 2.0f;  // ダメージ間隔（秒）
    private bool collideForcedOffByTimeover = false;
    public GameObject time_delete;   // 時間減少表示UI
    public float timeDeleteDisplayTime = 1.5f; // 表示時間（秒）
    private Coroutine timeDeleteCoroutine;





    private Animator anim;
    private bool runFlag;
    Rigidbody rb;

    //private bool RefugeOn; //避難所に到達したかを管理する

    // コントローラの入力値
    private float v;
    private float h;
    [Header("Controller Index")]
    [Tooltip("Gamepad index used from Unity InputSystem (Gamepad.all)")]
    [SerializeField] private int gamepadIndex = 0;
    [Tooltip("Joy-Con index used from JoyconManager.Instance.j")]
    [SerializeField] private int joyconIndex = 0;
    private List<Joycon> joycons; // Joy-Conのリスト
    private Joycon joycon;        // 今使うJoy-Con（片方
    private bool left = false;

    // Setting.csのInterfaceTypeを参照して共有
    private Setting settingScript;
    private string interfaceType;
    JoyconManager joyconManager;

    PlayerTrail pt;
    PlayerPosition pp;

    void Awake()
    {
        // JoyconManagerを取得
        joyconManager = FindFirstObjectByType<JoyconManager>();
        pt = FindFirstObjectByType<PlayerTrail>();
        pp = FindFirstObjectByType<PlayerPosition>();
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

        // Inspector の値を優先するが、保存済みの値があればそれを読み込む
        if (PlayerPrefs.HasKey("gamepadIndex")) gamepadIndex = PlayerPrefs.GetInt("gamepadIndex", gamepadIndex);
        if (PlayerPrefs.HasKey("joyconIndex")) joyconIndex = PlayerPrefs.GetInt("joyconIndex", joyconIndex);

        // Joy-Conの接続・初期化処理を関数化
        SetupJoycon();

        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        if (Info_timeover != null)
            Info_timeover.SetActive(false);
        if (time_delete != null)
            time_delete.SetActive(false);


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
            if (joyconIndex >= 0 && joyconIndex < joycons.Count)
            {
                joycon = joycons[joyconIndex];
                Debug.Log($"Joy-Con接続成功 (index={joyconIndex})");
            }
            else
            {
                joycon = joycons[0];
                Debug.LogWarning($"joyconIndex {joyconIndex} が範囲外のため index=0 を使用します");
            }
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

        // 瓦礫接触時間を計測し，1秒後にダメージ開始
        if (isTouchingDebris)
        {
            debrisTouchTime += Time.deltaTime;

            if (!debrisDamageActive && debrisTouchTime >= requireTouchTime)
            {
                debrisDamageActive = true;

                Info_collide.SetActive(true);

                if (!isBlinking)
                {
                    blinkCoroutine = StartCoroutine(BlinkUI());
                    isBlinking = true;
                }
            }

        }
        else
        {
            // 触れていないのでリセット
            debrisTouchTime = 0f;
            debrisDamageActive = false;
        }

        //ダメージ継続処理（一定間隔で減らす）
        if (debrisDamageActive)
        {
            damageTimer += Time.deltaTime;

            // damageIntervalごとに1回ダメージ（繰り返し）
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;

                Info_collide.SetActive(true);

                if (timerScript != null)
                {
                    timerScript.timeRemaining -= 5f;

                    // 赤
                    timerScript.SetWarning(true);

                    // 少し後に元の色へ戻す
                    StartCoroutine(ResetTimerColor());

                    ShowTimeDelete();

                    if (timerScript.timeRemaining <= 0)
                    {
                        timerScript.timeRemaining = 0;
                        ForceDisableCollideUI();
                        PlayerDie();
                    }
                }
            }

        }
        else
        {
            // 離れたらリセット
            damageTimer = 0f;
        }


        if (!collideForcedOffByTimeover &&
        timerScript != null &&
        timerScript.timeRemaining <= 0f)
        {
            collideForcedOffByTimeover = true;

            if (Info_collide != null && Info_collide.activeSelf)
                Info_collide.SetActive(false);
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

        // if(Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     pp.SetPosition();
        //     pt.Resetposilist();
        // }
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
    // ...existing code...
    void pad()
    {
        // 全ての接続されているゲームパッドを取得
        var pads = Gamepad.all;
        Gamepad gamepad = null;

        // 指定された index のゲームパッドを取得
        if (pads.Count > 0)
        {
            if (gamepadIndex >= 0 && gamepadIndex < pads.Count)
            {
                gamepad = pads[gamepadIndex];
            }
            else
            {
                // 範囲外なら先頭を使う（ログを残す）
                gamepad = pads[0];
                Debug.LogWarning($"gamepadIndex {gamepadIndex} が範囲外のため index=0 を使用します");
            }
        }
        else
        {
            // デバイスリストが空なら current を試す
            gamepad = Gamepad.current;
        }

        if (gamepad != null)
        {
            v = gamepad.leftStick.y.ReadValue();
            h = gamepad.leftStick.x.ReadValue();

            // if (gamepad.buttonSouth.wasPressedThisFrame || Input.GetKeyDown(KeyCode.JoystickButton0))
            // {
            //     pt.Resetposilist();
            //     pp.SetPosition();
            // }
        }
        else
        {
            Debug.LogWarning("Gamepad not connected");
        }
    }

    /// <summary>
    /// ランタイムで gamepad index を変更する。
    /// `saveToPrefs` を true にすると PlayerPrefs に保存され、次回起動時に復元されます。
    /// </summary>
    public void SetGamepadIndex(int index, bool saveToPrefs = false)
    {
        gamepadIndex = index;
        if (saveToPrefs) PlayerPrefs.SetInt("gamepadIndex", gamepadIndex);
    }

    /// <summary>
    /// ランタイムで joycon index を変更する（即座に適用）。
    /// </summary>
    public void SetJoyconIndex(int index, bool saveToPrefs = false)
    {
        joyconIndex = index;
        if (saveToPrefs) PlayerPrefs.SetInt("joyconIndex", joyconIndex);
        // 再選択
        if (joycons != null && joycons.Count > 0)
        {
            if (joyconIndex >= 0 && joyconIndex < joycons.Count) joycon = joycons[joyconIndex];
            else joycon = joycons[0];
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
            ForceDisableCollideUI();

            // ↓↓↓ ここから下は元のまま ↓↓↓
            Globaldata.playerposi = pt.getPlayerPosiList();

            if (timerScript != null)
            {
                timerScript.StopTimer();
                GameData.clearTime = timerScript.GetTime();
                GameData.hinan = true; //避難成功を格納
            }

            Info_end.SetActive(true);
            Invoke("LoadGoalScene", 5f); // 5秒後にシーン遷移
        }
    }


    //瓦礫に当たり続けているときの処理するメソッド
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Blockade"))
        {
            collideCount++;
            isTouchingDebris = true;

        
        }
    }



    //瓦礫から離れた時の処理するメソッド
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Blockade"))
        {
            collideCount--;

            if (collideCount <= 0)
            {
                collideCount = 0;
                isTouchingDebris = false;

                debrisTouchTime = 0f;
                debrisDamageActive = false;

                if (timerScript != null)
                    timerScript.SetWarning(false);


                // 点滅停止
                if (blinkCoroutine != null)
                    StopCoroutine(blinkCoroutine);
                isBlinking = false;
                // 完全に非表示に戻す
                Info_collide.SetActive(false);
            }

        }
    }


    //UIの点滅表示メソッド
    private IEnumerator BlinkUI()
    {
        // ON
        Info_collide.SetActive(true);
        yield return new WaitForSeconds(blinkInterval);

        // OFF
        Info_collide.SetActive(false);
        yield return new WaitForSeconds(blinkInterval);

        // この点滅サイクルが終わったので解除
        isBlinking = false;

        // まだ瓦礫に当たっているなら再度点滅を開始
        if (collideCount > 0)
        {
            blinkCoroutine = StartCoroutine(BlinkUI());
            isBlinking = true;
        }
    }


    // コルーチンのため別メソッド
    void LoadGoalScene()
    {
        SceneManager.LoadScene("result");
    }

    void PlayerDie()
    {
        ForceDisableCollideUI();

        if (currentHP == 0)
            Info_timeover.SetActive(true);

        Invoke("LoadGoalScene", 5f);
    }


    //ゲーム終了時に瓦礫UIを強制OFFするメソッド
    void ForceDisableCollideUI()
    {
        if (Info_collide != null && Info_collide.activeSelf)
            Info_collide.SetActive(false);

        // 点滅していたら止める
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        isBlinking = false;

        // 瓦礫関連の状態を止める
        collideCount = 0;
        isTouchingDebris = false;
        debrisTouchTime = 0f;
        debrisDamageActive = false;
        damageTimer = 0f;

        if (timerScript != null)
            timerScript.SetWarning(false);
    }

    //time_delateを表示するメソッド
    void ShowTimeDelete()
    {
        if (time_delete == null) return;

        // すでに表示中ならリセット
        if (timeDeleteCoroutine != null)
            StopCoroutine(timeDeleteCoroutine);

        timeDeleteCoroutine = StartCoroutine(TimeDeleteRoutine());
    }

    //time_delateを一定時間表示するメソッド
    IEnumerator TimeDeleteRoutine()
    {
        time_delete.SetActive(true);
        yield return new WaitForSeconds(timeDeleteDisplayTime);
        time_delete.SetActive(false);
    }

    //time_delateを消すメソッド
    IEnumerator ResetTimerColor()
    {
        yield return new WaitForSeconds(1.0f); // 赤表示

        if (timerScript != null)
            timerScript.SetWarning(false);
    }


}
