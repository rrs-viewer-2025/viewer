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


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // リセットボタンの設定
        Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSimulation); // リセットボタンをクリックしたときにResetSimulationを呼び出す
    }

    // Update is called once per frame
    void Update()
    {
        float v = Input.GetAxis("Vertical"); //上下キーの取得
        float h = Input.GetAxis("Horizontal"); //左右キーの取得

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
