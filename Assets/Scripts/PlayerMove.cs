using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;

public class PlayerCharaControl : MonoBehaviour
{
    public float forwardSpeed = 5.0f;//前進速度
    public float rotationSpeed = 100.0f;//回転速度
    private Animator anim;
    private bool runFlag;
    Rigidbody rb;

    private float v;
    private float h;

    private string interfaceType = "key"; // デフォルトはキーボード

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // リセットボタンの設定
        Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSimulation);

        // config.jsonからinterfaceを読み込む
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            JObject config = JObject.Parse(json);
            interfaceType = config["interface"]?.ToString() ?? "key";
            Debug.Log($"[PlayerCharaControl] interface: {interfaceType}");
        }
        else
        {
            Debug.LogError($"[PlayerCharaControl] config.json not found at {path}");
        }
    }

    void Update()
    {
        // 入力方法に応じて処理分岐
        if (interfaceType == "key")
        {
            key();
        }
        else if (interfaceType == "mat")
        {
            Mat();
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

        anim.SetBool("Run", runFlag);
        transform.position += transform.forward * forwardSpeed * v * Time.deltaTime;
        transform.Rotate(0, rotationSpeed * h * Time.deltaTime, 0);
    }

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

    void Mat()
    {
        v = 0.0f;
        h = 0.0f;

        MatAction mat = FindObjectOfType<MatAction>();
        if (mat != null)
        {
            if (mat.Up > 0) v = 1.0f;
            if (mat.Down > 0) v = -1.0f;
            if (mat.Left > 0) h = -1.0f;
            if (mat.Right > 0) h = 1.0f;
        }
    }

    void ResetSimulation()
    {
        transform.position = new Vector3(250, 0, 120);
    }
}
