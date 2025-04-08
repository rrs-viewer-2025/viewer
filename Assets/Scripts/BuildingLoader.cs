using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class BuildingDrawer : MonoBehaviour
{
    public GameObject cubePrefab; // Cube プレハブ
    private string logfolder; // JSONファイルの名前

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;
        LoadAndDrawBuilding();
    }

    // JSONを読み込んで建物を描画
    void LoadAndDrawBuilding()
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
                if (urn == URN.Entity.BUILDING) // 建物の場合 (URN 4356)
                {
                    int entityID = entity["entityID"].ToObject<int>();
                    int x = 0, y = 0;
                    int height = 7;
                    int floor = 1; //初期値

                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == 4614) x = prop["intValue"].ToObject<int>(); // X座標
                        if (propUrn == 4615) y = prop["intValue"].ToObject<int>(); // Y座標
                        if (propUrn == 4618) {
                            floor = prop["intValue"].ToObject<int>();
                            Debug.Log($"Building {entityID}: Found floor value = {floor} (before edge processing)");
                        }
                    }

                    // 座標とエッジ情報を取得
                    List<Vector3> edges = new List<Vector3>();
                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();

                        if (propUrn == 4627) // エッジ情報
                        {
                            var edgeList = prop["edgeList"]["edges"];
                            foreach (var edge in edgeList)
                            {
                                int startX = edge["startX"].ToObject<int>();
                                int startY = edge["startY"].ToObject<int>();
                                int endX = edge["endX"].ToObject<int>();
                                int endY = edge["endY"].ToObject<int>();

                                Debug.Log($"Building {entityID}: Processing edge from ({startX}, {startY}) to ({endX}, {endY})");

                                for(int i=1; i<=floor; i++){
                                    Debug.Log($"Building {entityID}: Drawing edges for floor {i} of {floor} (inside loop)");
                                    Vector3 start = new Vector3(startX / 1000f, height*i, startY / 1000f);
                                    Vector3 end = new Vector3(endX / 1000f, height*i, endY / 1000f);

                                    edges.Add(start);
                                    edges.Add(end);

                                    Debug.Log($"Building {entityID} - Floor {i}: Edge from {start} to {end}");
                                }

                                Vector3 start1 = new Vector3(startX / 1000f, 0, startY / 1000f);   
                                Vector3 end1 = new Vector3(startX / 1000f, floor * height, startY / 1000f);
                                edges.Add(start1);
                                edges.Add(end1);                            
                            }
                        }
                    }

                    // 建物を描画
                    Debug.Log($"Building {entityID}: Total edges added = {edges.Count}");

                    DrawBuilding(new Vector3(x / 1000f, 0, y / 1000f), edges);
                }
            }
        }
        else
        {
            Debug.LogError("JSONファイルが見つかりません: " + filePath);
        }
    }

    // Cubeを使って建物を描画
    void DrawBuilding(Vector3 buildingPosition, List<Vector3> edges)
    {
        for (int i = 0; i < edges.Count; i += 2)
        {
            Vector3 start = edges[i];
            Vector3 end = edges[i + 1];

            // エッジをCubeで描画
            Vector3 direction = end - start;
            GameObject cube = Instantiate(cubePrefab, (start + end) / 2, Quaternion.LookRotation(direction));
            cube.transform.localScale = new Vector3(0.1f, 0.1f, direction.magnitude); // 厚みを薄く、長さに合わせて調整
        }
    }
}
