using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerCharaControl : MonoBehaviour
{
    [Header("Movement Settings")]
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

    // コントローラの入力値
    private float v;
    private float h;
    public enum PlayerID
    {
        P1,
        P2
    }

    [Header("Player Settings")]
    public PlayerID playerID = PlayerID.P1;

    // Setting.csのInterfaceTypeを参照して共有
    private Setting settingScript;
    private string interfaceType;

    PlayerTrail pt;
    PlayerPosition pp;

    void Awake()
    {
        pt = GetComponent<PlayerTrail>();
        pp = FindFirstObjectByType<PlayerPosition>();
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

    void Update()
    {
        if(GameData.ParentGoal && GameData.ChildGoal)
        {
            syuryou();
        }
        // 入力方法に応じて処理分岐
        switch (interfaceType)
        {
            case "key":
                key();
                break;
            case "pad":
                pad();
                break;
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
    /// ゲームパッドからの入力によるプレイヤーの移動処理。
    /// 縦横の入力値を取得し、移動方向を決定。
    /// </summary>
    // ...existing code...
    void pad()
    {
        if (playerID == PlayerID.P1)
        {
            v = Input.GetAxis("Vertical_P1") * -1f;
            h = Input.GetAxis("Horizontal_P1");
        }

        else if (playerID == PlayerID.P2)
        {
            v = Input.GetAxis("Vertical_P2") * -1f;
            h = Input.GetAxis("Horizontal_P2");
        }
    }

    // 歩くアニメーションのメソッド
    public void OnFootstep()
    {
        // Debug.Log("Footstep!");
        // AudioSource.PlayClipAtPoint(footstepClip, transform.position); なども可
    }

    // Goalシーンに遷移するメソッド
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.tag == "Refuge")
    //     {
    //         ForceDisableCollideUI();

    //         // ↓↓↓ ここから下は元のまま ↓↓↓
    //         Globaldata.playerposi = pt.getPlayerPosiList();

    //         if (timerScript != null)
    //         {
    //             timerScript.StopTimer();
    //             GameData.clearTime = timerScript.GetTime();
    //             GameData.hinan = true; //避難成功を格納
    //         }

    //         Info_end.SetActive(true);
    //         Invoke("LoadGoalScene", 5f); // 5秒後にシーン遷移
    //     }
    // }

    public void syuryou()
    {
        ForceDisableCollideUI();

        if (playerID == PlayerID.P1)
        {
            Globaldata.player1Pos = pt.getPlayerPosiList();
        }
        else if (playerID == PlayerID.P2)
        {
            Globaldata.player2Pos = pt.getPlayerPosiList();
        }

        if (timerScript != null)
        {
            timerScript.StopTimer();
            GameData.clearTime = timerScript.GetTime();
            GameData.hinan = true; //避難成功を格納
        }

        Info_end.SetActive(true);
        Invoke("LoadGoalScene", 5f); // 5秒後にシーン遷移
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
