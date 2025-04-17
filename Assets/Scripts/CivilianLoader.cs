using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;

public class CivilianLoader : MonoBehaviour
{
    public GameObject citizenPrefab; // 市民のプレハブ
    public int MaxStep = 270; // 最大ステップ
    public int Step = 1; // 現在のステップ
    public string logfolder;
    
    private Dictionary<int, GameObject> civilians = new Dictionary<int, GameObject>(); // エンティティIDとGameObjectの紐づけ
    private float lastUpdateTime = 0f; // 最後に更新した時間
    public float updateInterval = 1.0f; // 更新間隔（秒）

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;
        LoadInitialConditions();

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
                LoadCitizenLoad();
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
                if (urn == URN.Entity.CIVILIAN) // 市民
                {
                    int entityID = entity["entityID"].ToObject<int>();
                    int x = 0, y = 0;
                    int hp = 0, stamina = 0, damage = 0, buriedness = 0, traveldistance = 0;
                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == URN.Property.X) x = prop["intValue"].ToObject<int>(); // X座標
                        if (propUrn == URN.Property.Y) y = prop["intValue"].ToObject<int>(); // Y座標
                        if (propUrn == URN.Property.HP) hp = prop["intValue"].ToObject<int>(); // hp
                        if (propUrn == URN.Property.STAMINA) stamina = prop["intValue"].ToObject<int>(); // stamina
                        if (propUrn == URN.Property.DAMAGE) damage = prop["intValue"].ToObject<int>(); //  damage
                        if (propUrn == URN.Property.BURIEDNESS) buriedness = prop["intValue"].ToObject<int>(); // buriedness
                        if (propUrn == URN.Property.TRAVEL_DISTANCE) traveldistance = prop["intValue"].ToObject<int>(); // traveldistance
                    }

                    Vector3 position = new Vector3(x / 1000f, 0, y / 1000f);
                    GameObject citizen = Instantiate(citizenPrefab, position, Quaternion.identity);

                    civilians[entityID] = citizen; // エンティティIDとオブジェクトを紐づけ

                    if (citizen.GetComponent<Collider>() == null)
                    {
                        BoxCollider collider = citizen.AddComponent<BoxCollider>();
                        collider.size = new Vector3(1f, 2f, 1f);
                        collider.center = new Vector3(0f, 1f, 0f);
                    }

                    SetEntityID citizenScript = citizen.GetComponent<SetEntityID>();
                    CivilianState stateScript = citizen.GetComponent<CivilianState>();
                    if (citizenScript != null)
                    {
                        citizenScript.SetEntityIDtoEntity(entityID);
                    }
                    if (stateScript != null)
                    {
                        stateScript.SetState(stamina, hp, damage, buriedness, traveldistance);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("INITIAL_CONDITIONS.json が見つかりません: " + filePath);
        }
    }

    void getstepdata() //市民のパラメータの変更を記録する処理を後で追加しようかな
    {
        Debug.Log($"ステップ: {Step}");
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
            if (urn != URN.Entity.CIVILIAN) continue; // 市民以外は無視

            int entityID = change["entityID"].ToObject<int>();

            // `civilians` にエンティティIDが存在しない場合はスキップ
            if (!civilians.ContainsKey(entityID) || civilians[entityID] == null)
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

            bool flag = true;
            int x = 0, y = 0;
            bool shouldUpdatePosition = false;

            foreach (var prop in change["properties"])
            {
                int propUrn = prop["urn"].ToObject<int>();
                
                if (propUrn == URN.Property.POSITION_HISTORY)
                {
                    if (prop["defined"] != null)
                    {
                        flag = prop["defined"].ToObject<bool>();
                    }
                    if (!flag)
                    {
                        flag = true;
                        continue;
                    }
                }
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

            // 救急隊が市民を運ぶときにエラー吐かれる問題を修正
            if (shouldUpdatePosition)
            {
                Vector3 newPosition = new Vector3(x / 1000f, 0, y / 1000f);
                civilians[entityID].transform.position = newPosition;
            }
        }
    }

    void LoadCitizenLoad()
    {
        Debug.Log($"ステップ: {Step}");
        string updatePath = logfolder + "/" + Step + "/COMMANDS.json";

        if (!File.Exists(updatePath))
        {
            Debug.LogError($"COMMANDS.json が見つかりません: {updatePath}");
            return;
        }

        string jsonText = File.ReadAllText(updatePath);
        JObject json = JObject.Parse(jsonText);

        // `changes` が null の場合は処理を中断
        if (json["command"]?["commands"] == null)
        {
            Debug.LogWarning($"ステップ {Step} の変更データが存在しません。");
            return;
        }

        JArray commands = (JArray)json["command"]["commands"];

        foreach (var command in commands)
        {
            int urn = command["urn"].ToObject<int>();
            if (urn != URN.Command.AK_LOAD) continue; // LOADコマンドのみ対象（4867）

            JObject components = (JObject)command["components"];

            // エージェントIDの取得（514）
            if (components.TryGetValue(URN.ComponentControlMSG.AgentID.ToString(), out JToken agentToken) &&
                components.TryGetValue(URN.ComponentCommand.Target.ToString(), out JToken targetToken))
            {
                int agentID = agentToken["entityID"].ToObject<int>();
                int targetID = targetToken["entityID"].ToObject<int>();

                Debug.Log($"Agent {agentID} が Civilian {targetID} をloadしました! ");

                // 必要であれば、civilians[targetID] を非表示などにする処理もここで行える
                // マップ上から市民を非表示にする
                if (civilians.ContainsKey(targetID))
                {
                    civilians[targetID].SetActive(false); // 非表示
                    // Destroy(civilians[targetID]); // 完全に削除したい場合はこちら
                    Debug.Log($"Civilian {targetID} をマップから非表示にしました。");
                }
                else
                {
                    Debug.LogWarning($"Civilian {targetID} の GameObject が存在しません。");
                }
            }
        }

        foreach (var command in commands)
        {
            int urn = command["urn"].ToObject<int>();
            if (urn != URN.Command.AK_UNLOAD) continue; // UNLOADコマンドのみ対象

            JObject components = (JObject)command["components"];

            if (components.TryGetValue(URN.ComponentControlMSG.AgentID.ToString(), out JToken agentToken) &&
                components.TryGetValue(URN.ComponentCommand.Target.ToString(), out JToken targetToken))
            {
                int agentID = agentToken["entityID"].ToObject<int>();
                int targetID = targetToken["entityID"].ToObject<int>();

                Debug.Log($" Agent {agentID} が Civilian {targetID} を UNLOAD しました！");

                // 救急車の位置を取得
                if (civilians.ContainsKey(targetID) && civilians[targetID] != null)
                {
                    civilians[targetID].SetActive(true); // 再出現

                    if (civilians.ContainsKey(agentID) && civilians[agentID] != null)
                    {
                        // 救急車と同じ位置に移動（避難所と仮定）
                        civilians[targetID].transform.position = civilians[agentID].transform.position;
                        Debug.Log($"Civilian {targetID} を Agent {agentID} の位置に移動させました。");
                    }
                    else
                    {
                        Debug.LogWarning($"Agent {agentID} の GameObject が見つかりませんでした。");
                    }
                }
                else
                {
                    Debug.LogWarning($"Civilian {targetID} の GameObject が存在しません。");
                }
            }
        }

    }

    // リセット処理
    void ResetSimulation()
    {
        Step = 1; // ステップを1に戻す
        foreach (var citizen in civilians.Values)
        {
            Destroy(citizen); // 市民を削除
        }
        civilians.Clear(); // 市民の辞書をクリア

        LoadInitialConditions(); // 初期状態から再読込
    }
}
