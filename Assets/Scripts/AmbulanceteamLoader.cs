using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class AmbulanceteamLoader : MonoBehaviour
{
    public GameObject AmbulanceteamPrefab; // 救急隊のプレハブ
    // public int MaxStep = 270; //最大ステップ
    // public int Step = 1; //現在のステップ
    public string logfolder;
    private Dictionary<int, GameObject> Ambulanceteams = new Dictionary<int, GameObject>(); //IDとオブジェクトの紐付け
    // private float lastUpdateTime = 0f; //最後に更新した時間
    // public float updateInterval = 1.0f; //更新間隔（秒）
    private Coroutine notifyCoroutine;

    StepManager stepManager;

    void Awake()
    {
        stepManager = FindFirstObjectByType<StepManager>();
    }

    public void SetLogFolderPath(string path)
    {
        logfolder = path;
    }

    public void LoadInitialConditions()
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

    public void StartStep()
    {
        if (notifyCoroutine != null)
        {
            StopCoroutine(notifyCoroutine);
        }
        getstepdata();
        notifyCoroutine = StartCoroutine(NotifyStepCompletedWithDelay());
    }

    void getstepdata()
    {
        int Step = stepManager.GetCurrentStep();
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
            List<Vector3> MovePath = new List<Vector3>(); // 経由地点リスト
            int distance = 0;

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

                //経由地点の取得
                if (propUrn == URN.Property.POSITION_HISTORY && prop["intList"]?["values"] != null)
                {
                    JArray values = (JArray)prop["intList"]["values"];
                    for (int i = 0; i < values.Count; i += 2)
                    {
                        int historyX = values[i].ToObject<int>();
                        int historyY = values[i + 1].ToObject<int>();
                        Vector3 his_posi = new Vector3(historyX / 1000f, 0, historyY / 1000f);
                        MovePath.Add(his_posi);
                    }
                }

                //移動距離の取得
                if (propUrn == URN.Property.TRAVEL_DISTANCE)
                {
                    distance = prop["intValue"].ToObject<int>();
                }

            }

            if (shouldUpdatePosition)
            {
                Vector3 newPosition = new Vector3(x / 1000f, 0, y / 1000f); //ゴール地点
                MovePath.Add(newPosition);
                // Ambulanceteams[entityID].transform.position = newPosition;
                // MoveAlongPath(Ambulanceteams[entityID], MovePath);
                StartCoroutine(MoveAlongPath(Ambulanceteams[entityID], MovePath, distance));
            }
        }
    }

    IEnumerator MoveAlongPath(GameObject obj, List<Vector3> path, int distance)
    {
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 target = path[i];

            while (Vector3.Distance(obj.transform.position, target) > 0.05f)
            {
                Vector3 direction = (target - obj.transform.position).normalized;
                if (direction != Vector3.zero)
                    obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);

                obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, distance / 1000f / 2f * Time.deltaTime); // 移動速度 5f
                yield return null;
            }
        }

        // Debug.Log($"【移動完了】{obj.name} が目的地に到達");
    }


    IEnumerator NotifyStepCompletedWithDelay()
    {
        yield return new WaitForSeconds(3f); // 2秒待つ
        stepManager.NotifyCompleted();
    }


    // リセット処理
    public void Reset()
    {
        if (notifyCoroutine != null)
        {
            StopCoroutine(notifyCoroutine);
            notifyCoroutine = null;
        }
        // Step = 1; // ステップを1に戻す
        foreach (var Ambulance in Ambulanceteams.Values)
        {
            Destroy(Ambulance); // 救急隊を削除
        }
        Ambulanceteams.Clear(); // 救急隊の辞書をクリア
    }
}
