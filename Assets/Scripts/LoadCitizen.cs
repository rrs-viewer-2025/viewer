using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;

public class LoadCitizen : MonoBehaviour
{
    public GameObject citizenPrefab; // 市民のプレハブ
    public int MaxStep = 270; // 最大ステップ
    public int Step = 1; // 現在のステップ
    public string logfolder;

    private Dictionary<int, GameObject> civilians = new Dictionary<int, GameObject>(); // エンティティIDとGameObjectの紐づけ
    public float updateInterval = 1.0f; // 更新間隔（秒）

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;
        LoadCitizenUpload();
    }

    void LoadCitizenUpload() //市民のパラメータの変更を記録する処理を後で追加しようかな
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
}
