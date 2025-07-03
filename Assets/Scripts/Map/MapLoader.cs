using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    // JSONデータの構造
    [System.Serializable]
    public class MapData
    {
        public Dictionary<string, List<float>> nodes;
        public List<Edge> edges;
        public List<Building> buildings;
        public List<Road> roads;
    }

    [System.Serializable]
    public class Edge
    {
        public string id;
        public List<string> nodes;
    }

    [System.Serializable]
    public class Building
    {
        public string id;
        public List<string> edges;
        public int floors;
        public int buildingCode;
        public int importance;
    }

    [System.Serializable]
    public class Road
    {
        public string id;
        public List<string> edges;
    }

    public Material edgeMaterial; // インスペクタで設定できるマテリアル

    void Start()
    {
        // JSONファイルのパス
        string jsonFilePath = Path.Combine(Application.streamingAssetsPath, "map.json");

        // JSONファイルの読み込み
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            MapData mapData = JsonConvert.DeserializeObject<MapData>(json);

            // ノード（座標）の表示
            foreach (var node in mapData.nodes)
            {
                Vector3 position = new Vector3(node.Value[0]+24f, 0, node.Value[1]);
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = position;
                sphere.transform.localScale = Vector3.one * 0.5f; // 少し小さく
                sphere.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value); // 色をランダムに設定
            }

            // エッジ（線）の表示
            foreach (var edge in mapData.edges)
            {
                // ノードIDを使って位置を取得
                Vector3 startPos = new Vector3(mapData.nodes[edge.nodes[0]][0]+24f, 0, mapData.nodes[edge.nodes[0]][1]);
                Vector3 endPos = new Vector3(mapData.nodes[edge.nodes[1]][0]+24f, 0, mapData.nodes[edge.nodes[1]][1]);
                
                // 線を描画
                GameObject line = new GameObject("Edge " + edge.id);
                LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
                lineRenderer.SetPositions(new Vector3[] { startPos, endPos });
                // 線の太さを設定
                lineRenderer.startWidth = 0.5f;
                lineRenderer.endWidth = 0.5f;

                if (edgeMaterial != null)
                {
                    lineRenderer.material = edgeMaterial; // インスペクタで設定したマテリアルを使用
                }
                else
                {
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                }
            }

            // 建物や道路の処理（仮の処理例）
            /*
            foreach (var building in mapData.buildings)
            {
                Vector3 buildingPosition = new Vector3(mapData.nodes[building.edges[0]][0], 0, mapData.nodes[building.edges[0]][1]);
                GameObject buildingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                buildingObject.transform.position = buildingPosition;
                buildingObject.transform.localScale = new Vector3(5, building.floors * 2, 5); // 階数に応じた高さ
                buildingObject.GetComponent<Renderer>().material.color = Color.green;
            }
            */
        }
        else
        {
            Debug.LogError("map.json ファイルが見つかりません");
        }
    }
}
