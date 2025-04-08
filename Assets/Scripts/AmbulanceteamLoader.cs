using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class AmbulanceteamLoader : MonoBehaviour
{
    public GameObject AmbulanceteamPrefab; // 救急隊のプレハブ
    public int MaxStep = 270; //最大ステップ
    public int Step = 1; //現在のステップ
    public string logfolder;
    private Dictionary<int, GameObject> Ambulanceteams = new Dictionary<int, GameObject>(); //IDとオブジェクトの紐付け
    private float lastUpdateTime = 0f; //最後に更新した時間
    public float updateInterval = 1.0f; //更新間隔（秒）

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;
        LoadInitialConditions(); //初期状態の読み込み

        // リセットボタンの設定
        Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSimulation); // リセットボタンをクリックしたときにResetSimulationを呼び出す
    }

    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            if (Step <= MaxStep)
            {
                getstepdata();
                Step++;
                lastUpdateTime = Time.time; // 更新時間をリセット
            }
        }
    }

    void LoadInitialConditions()
    {
        string filePath = logfolder + "/INITIAL_CONDITIONS.json";

        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JArray entities = (JArray)json["initialCondition"]["entities"];

            foreach (var entity in entities)
            {
                int urn = entity["urn"].ToObject<int>();
                if (urn == URN.Entity.AMBULANCE_TEAM) // 救急隊
                {
                    int entityID = entity["entityID"].ToObject<int>();
                    int x = 0, y = 0;
                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == URN.Property.X) x = prop["intValue"].ToObject<int>(); // X座標
                        if (propUrn == URN.Property.Y) y = prop["intValue"].ToObject<int>(); // Y座標
                    }

                    Vector3 position = new Vector3(x / 1000f, 0, y / 1000f); // スケール調整
                    GameObject Ambulanceteam = Instantiate(AmbulanceteamPrefab, position, Quaternion.identity);

                    Ambulanceteams[entityID] = Ambulanceteam; //IDとオブジェクトの紐付け
                    
                    // コライダーがなければ追加
                    if (Ambulanceteam.GetComponent<Collider>() == null)
                    {
                        BoxCollider collider = Ambulanceteam.AddComponent<BoxCollider>();
                        collider.size = new Vector3(1f, 2f, 1f); // 元のスケール用
                        collider.center = new Vector3(0f, 1f, 0f);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("JSONファイルが見つかりません: " + filePath);
        }
    }

    void getstepdata()
    {
        string updatePath = logfolder + "/" + Step + "/UPDATES.json";

        if (!File.Exists(updatePath))
        {
            Debug.LogError($"UPDATES.json が見つかりません: {updatePath}");
            return;
        }

        string jsonText = File.ReadAllText(updatePath);
        JObject json = JObject.Parse(jsonText);

        // `changes` が null の場合は処理を中断
        if (json["update"]?["changes"]?["changes"] == null)
        {
            Debug.LogWarning($"ステップ {Step} の変更データが存在しません。");
            return;
        }

        JArray changes = (JArray)json["update"]["changes"]["changes"];

        foreach (var change in changes)
        {
            int urn = change["urn"].ToObject<int>();
            if (urn != URN.Entity.AMBULANCE_TEAM) continue; //救急隊以外は無視

            int entityID = change["entityID"].ToObject<int>();

            // `civilians` にエンティティIDが存在しない場合はスキップ
            if (!Ambulanceteams.ContainsKey(entityID) || Ambulanceteams[entityID] == null)
            {
                Debug.LogWarning($"エンティティ {entityID} が辞書に存在しない、または null です。");
                continue;
            }

            // `change["properties"]` が null の場合はスキップ
            if (change["properties"] == null)
            {
                Debug.LogWarning($"エンティティ {entityID} のプロパティが見つかりません。");
                continue;
            }

            int x = 0, y = 0;
            bool shouldUpdatePosition = false;

            foreach (var prop in change["properties"])
            {
                int propUrn = prop["urn"].ToObject<int>();

                if (propUrn == URN.Property.X && prop["intValue"] != null)
                {
                    x = prop["intValue"].ToObject<int>();
                    shouldUpdatePosition = true;
                }
                if (propUrn == URN.Property.Y && prop["intValue"] != null)
                {
                    y = prop["intValue"].ToObject<int>();
                }
            }

            if (shouldUpdatePosition)
            {
                Vector3 newPosition = new Vector3(x / 1000f, 0, y / 1000f);
                Ambulanceteams[entityID].transform.position = newPosition;
            }
        }
    }

    // リセット処理
    void ResetSimulation()
    {
        Step = 1; // ステップを1に戻す
        foreach (var Ambulance in Ambulanceteams.Values)
        {
            Destroy(Ambulance); // 救急隊を削除
        }
        Ambulanceteams.Clear(); // 救急隊の辞書をクリア

        LoadInitialConditions(); // 初期状態から再読込
    }
}
