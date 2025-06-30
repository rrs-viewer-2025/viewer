using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class RefugeLoader : MonoBehaviour
{
    public GameObject cubePrefab; // Cube プレハブ
    private string logfolder;
    public Material defaultMaterial; // 建物に使うマテリアル（Inspectorで設定可能）

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

            int RefugeId = 0; // 各建物に一意な名前をつけるためのID

            Material parentMaterial = GetComponent<MeshRenderer>()?.sharedMaterial ?? defaultMaterial;

            foreach (var entity in entities)
            {
                int urn = entity["urn"].ToObject<int>();
                if (urn == 4357) // 避難所
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
                            // Debug.Log($"Building {entityID}: Found floor value = {floor} (before edge processing)");
                        }
                    }

                    // 座標とエッジ情報を取得
                    List<Vector3> edges = new List<Vector3>();
                    List<Vector3> apexList = new List<Vector3>(); // 床形状の頂点リスト
                    List<Mesh> meshes = new List<Mesh>(); // メッシュを蓄積するリスト
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

                                // Debug.Log($"Building {entityID}: Processing edge from ({startX}, {startY}) to ({endX}, {endY})");

                                for(int i=1; i<=floor; i++){
                                    // Debug.Log($"Building {entityID}: Drawing edges for floor {i} of {floor} (inside loop)");
                                    Vector3 start = new Vector3(startX / 1000f, height*i, startY / 1000f);
                                    Vector3 end = new Vector3(endX / 1000f, height*i, endY / 1000f);

                                    edges.Add(start);
                                    edges.Add(end);

                                    // Debug.Log($"Building {entityID} - Floor {i}: Edge from {start} to {end}");
                                }

                                Vector3 start1 = new Vector3(startX / 1000f, 0, startY / 1000f);   
                                Vector3 end1 = new Vector3(startX / 1000f, floor * height, startY / 1000f);
                                edges.Add(start1);
                                edges.Add(end1);
                                apexList.Add(start1); //床ポリゴン用                            
                            }
                        }
                    }

                    // 建物を描画
                    // DrawBuilding(new Vector3(x / 1000f, 0, y / 1000f), edges);
                    meshes.AddRange(MakeMeshes(apexList));

                    // 建物単位のGameObjectを作成して、子として追加
                    GameObject building = new GameObject("Refuge_" + RefugeId++);
                    building.transform.parent = this.transform;

                    DrawBuilding(new Vector3(x / 1000f, 0, y / 1000f), edges);

                    // メッシュとマテリアルを設定
                    MeshFilter mf = building.AddComponent<MeshFilter>();
                    MeshRenderer mr = building.AddComponent<MeshRenderer>();

                    // ここで親のマテリアルを使用
                    mr.material = parentMaterial;

                    // メッシュを統合して1つにまとめてセット
                    mf.mesh = CombineMeshes(meshes);
                    
                }
            }
        }
        else
        {
            // Debug.LogError("JSONファイルが見つかりません: " + filePath);
        }
    }

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

            // BoxCollider の設定
            BoxCollider collider = cube.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = cube.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.size = new Vector3(1f, 70f, 1f);   // 必要に応じてX/Zも調整可
            collider.center = new Vector3(0f, -35f, 0f); // Yセンターを下にずらす

            // タグ設定
            cube.tag = "Refuge";
        }
    }

    // meshの生成
    List<Mesh> MakeMeshes(List<Vector3> list)
    {
        int count = list.Count;
        int triCount = (count - 2) * 3;
        List<Mesh> meshes = new List<Mesh>();

        Vector3[] vertices = new Vector3[count];
        for(int i = 0; i < count; i++)
        {
            vertices[i] = list[i];
            vertices[i].y = 0.1f;
        }

        int[] tris = new int[triCount];
        for(int i = 0, j = 1; i < triCount; i += 3, j++)
        {
            tris[i] = 0;
            tris[i + 1] = j;
            tris[i + 2] = j + 1;

            Vector3 normal = Vector3.Cross(
                vertices[tris[i + 1]] - vertices[tris[i]],
                vertices[tris[i + 2]] - vertices[tris[i]]
            );
            if (Vector3.Dot(normal, Vector3.up) < 0)
            {
                int temp = tris[i + 1];
                tris[i + 1] = tris[i + 2];
                tris[i + 2] = temp;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        meshes.Add(mesh);

        return meshes;
    }

    Mesh CombineMeshes(List<Mesh> meshes)
    {
        CombineInstance[] combine = new CombineInstance[meshes.Count];
        for (int i = 0; i < meshes.Count; i++)
        {
            combine[i].mesh = meshes[i];
            combine[i].transform = Matrix4x4.identity;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 頂点数6万以上対応
        finalMesh.CombineMeshes(combine, true, false);
        return finalMesh;
    }
}
