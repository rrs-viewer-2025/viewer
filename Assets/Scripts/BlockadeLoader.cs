using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.UI;

public class BlockadeLoader : MonoBehaviour
{
    public GameObject BlockadePrefab;
    public int MaxStep = 270;
    public int Step = 1;
    public string logfolder;
    private float lastUpdateTime = 0f; //最後に更新した時間
    public float updateInterval = 1.0f; //更新間隔（秒）
    // IDと頂点リストの格納用
    private Dictionary<int, List<GameObject>> Blockades = new Dictionary<int, List<GameObject>>();

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;

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

    void getstepdata()
    {
        string filePath = logfolder + "/" + Step + "/UPDATES.json";

        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JArray entities = (JArray)json["update"]["changes"]["changes"];
            List<int> KeepID = new List<int>(); //残す瓦礫のIDリスト

            foreach (var entity in entities)
            {
                int urn = entity["urn"].ToObject<int>();

                if (urn == URN.Entity.BLOCKADE)
                {
                    int EntityID = entity["entityID"].ToObject<int>();
                    List<GameObject> blockadeList = new List<GameObject>();

                    // プロパティの中身を見る
                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == URN.Property.APEXES)
                        {
                            JArray values = (JArray)prop["intList"]["values"];
                            for (int i = 0; i < values.Count; i += 2)
                            {
                                int x = values[i].ToObject<int>();
                                int y = values[i + 1].ToObject<int>();
                                Vector3 position = new Vector3(x / 1000f, 0, y / 1000f);
                                GameObject Blockade = Instantiate(BlockadePrefab, position, Quaternion.identity);
                                blockadeList.Add(Blockade);
                            }
                        }
                    }

                    if (Step == 1)
                    {
                        Blockades[EntityID] = blockadeList;
                    }
                    else
                    {
                        if (Blockades.ContainsKey(EntityID))
                        {
                            ListChange(Blockades, blockadeList, EntityID);
                        }
                        else
                        {
                            findDeleteIdAndDelete(Blockades, blockadeList);
                            Blockades[EntityID] = blockadeList;
                        }
                    }
                }
            }
        }
    }


    //辞書に登録されたか確認するための関数　true または false を返す
    bool findIDfromDictionary(Dictionary<int, List<GameObject>> dictionary, int id)
    {
        foreach (var aaaa in dictionary)
        {
            if(aaaa.Key == id)
            {
                return true;
            }
        }

        return false;
    }

    void ListChange(Dictionary<int, List<GameObject>> dictionary, List<GameObject> newlist, int id)
    {
        // ゲームオブジェクトを破棄
        foreach (var obj in dictionary[id])
        {
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
        dictionary[id].Clear(); //既存のリストを消す
        dictionary[id] = newlist; //新しいリストと紐付け
    }

    void findDeleteIdAndDelete(Dictionary<int, List<GameObject>> dictionary, List<GameObject> newlist)
    {
        HashSet<GameObject> newListSet = new HashSet<GameObject>(newlist);
        List<int> deleteIDs = new List<int>();

        // 削除対象のIDを探す
        foreach (var D in dictionary)
        {
            if (D.Value.Any(obj => newListSet.Contains(obj)))
            {
                deleteIDs.Add(D.Key);
            }
        }

        // 見つかったIDの辞書を削除
        foreach (int id in deleteIDs.ToList())
        {
            if (!dictionary.ContainsKey(id)) continue;

            foreach (var obje in dictionary[id])
            {
                if (obje != null)
                {
                    GameObject.Destroy(obje); // 次のフレームで削除
                }
            }
            dictionary.Remove(id);
        }
    }

    void ResetSimulation()
    {
        Step = 1; // ステップを1に戻す
        foreach (var list in Blockades.Values)
        {
            foreach (var obj in list)
            {
                Destroy(obj); // 瓦礫の削除
            }
        }
        Blockades.Clear(); // 瓦礫リストをクリア
    }
}