using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCharaControl : MonoBehaviour
{
    public float forwardSpeed = 5.0f;//前進速度
    public float rotationSpeed = 100.0f;//回転速度
    private Animator anim;
    private bool runFlag;
    Rigidbody rb;
    private List<Joycon> joycons; // Joy-Conのリスト
    private Joycon joycon;        // 今使うJoy-Con（片方）
    private float h;
    private float v;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // リセットボタンの設定
        Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSimulation); // リセットボタンをクリックしたときにResetSimulationを呼び出す
        
         // JoyconManager から Joy-Con のリストを取得
        joycons = JoyconManager.Instance.j;

        // 少なくとも1つJoy-Conが接続されていたら使う
        if (joycons.Count > 0)
        {
            joycon = joycons[0]; // 0番目のJoy-Conを使用（通常は左）
            //Debug.Log("Joy-Con接続成功");
        }
        else
        {
            //Debug.Log("Joy-Conが接続されていません");
        }
    }

    // Update is called once per frame
    void Update()
    {
    if(joycon != null){
        var stick = joycon.GetStick();
        //Debug.Log("Joy-Con Input:" + stick[0] + stick[1]);  // 入力値を確認
        v = -stick[0];
        h = stick[1];
    }
    else{
        v = Input.GetAxis("Vertical"); //上下キーの取得
        h = Input.GetAxis("Horizontal"); //左右キーの取得
        }

        if(v>0.1||v<-0.1||h>0.1||h<-0.1){
            runFlag = true;
        }
        else{
            runFlag = false;
        }
        anim.SetBool("Run", runFlag);
        transform.position += transform.forward * forwardSpeed * v * Time.deltaTime; //プレイヤー移動
        transform.Rotate(0, rotationSpeed * h * Time.deltaTime, 0); //プレイヤー回転

        if(Input.GetKeyDown(KeyCode.Space)){
            anim.SetTrigger("Jump");
            rb.AddForce(transform.up * 1000 * 8, ForceMode.Force);
        }
    }

    // リセット処理
    void ResetSimulation()
    {
        transform.position = new Vector3(250,0,120);
    }

}
