using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class BuildingNode
{
    public int EntityID; //ID
    public Vector3 posi; // 中心座標
    public List<Vector3> apexes; //頂点リスト
    public int brokenness = 0; // 倒壊度
}

public class Building : MonoBehaviour
{
    private string logfolder; // ログフォルダのパス
    public Material defaultMaterial; // 建物に使うマテリアル（Inspectorで設定可能）
    public Dictionary<int, BuildingNode> buildingGraph = new Dictionary<int, BuildingNode>();
    public Dictionary<int, MeshRenderer> buildingRenderers = new Dictionary<int, MeshRenderer>();

    public void SetLogFolderPath(string path)
    {
        logfolder = path;
    }

    public void LoadInitialConditions()
    {
        string filePath = logfolder + "/INITIAL_CONDITIONS.json";
        if (!File.Exists(filePath)) return;

        string jsonText = File.ReadAllText(filePath);
        JObject json = JObject.Parse(jsonText);
        JArray entities = (JArray)json["initialCondition"]["entities"];

        int buildingId = 0; // 各建物に一意な名前をつけるためのID

        // 親の MeshRenderer からマテリアルを取得（null 安全確認つき）
        Material parentMaterial = GetComponent<MeshRenderer>()?.sharedMaterial ?? defaultMaterial;

        foreach (var entity in entities)
        {
            // 建物以外のエンティティは無視
            int urn = entity["urn"].ToObject<int>();
            if (urn != URN.Entity.BUILDING) continue;

            BuildingNode node = new BuildingNode();
            node.EntityID = entity["entityID"].ToObject<int>();

            // 建物の初期値
            List<Vector3> apexList = new List<Vector3>(); // 床形状の頂点リスト
            List<Mesh> meshes = new List<Mesh>(); // メッシュを蓄積するリスト

            int x = 0, y = 0;
            foreach (var prop in entity["properties"])
            {
                if (prop["urn"].ToObject<int>() == URN.Property.EDGES)
                {
                    var edgeList = prop["edgeList"]["edges"];
                    foreach (var edge in edgeList)
                    {
                        int startX = edge["startX"].ToObject<int>();
                        int startY = edge["startY"].ToObject<int>();
                        int endX = edge["endX"].ToObject<int>();
                        int endY = edge["endY"].ToObject<int>();

                        Vector3 pos = new Vector3(startX / 1000f, 0, startY / 1000f);
                        apexList.Add(pos); // 床ポリゴンのために保存
                    }
                }

                if (prop["urn"].ToObject<int>() == URN.Property.X) 
                x = prop["intValue"].ToObject<int>();
                if (prop["urn"].ToObject<int>() == URN.Property.Y) 
                y = prop["intValue"].ToObject<int>();
            }

            node.posi = new Vector3(x / 1000f, 0, y / 1000f);
            node.apexes = apexList;

            buildingGraph[node.EntityID] = node;

            // 各階の床メッシュを作成
            meshes.AddRange(MakeFloorMeshes(apexList));

            // 建物単位のGameObjectを作成して、子として追加
            GameObject building = new GameObject("Building_" + buildingId++);
            building.transform.parent = this.transform;

            // メッシュとマテリアルを設定
            MeshFilter mf = building.AddComponent<MeshFilter>();
            MeshRenderer mr = building.AddComponent<MeshRenderer>();

            mr.material = new Material(parentMaterial); // インスタンスを作ると個別色変更可能
            buildingRenderers[node.EntityID] = mr;       // ここで保存

            mf.mesh = CombineMeshes(meshes);
        }
    }

    public void SetBrokennes()
    {
        string updatePath = logfolder + "/1/UPDATES.json";

        if (!File.Exists(updatePath))
        {
            Debug.LogError($"UPDATES.json が見つかりません: {updatePath}");
            return;
        }

        string jsonText = File.ReadAllText(updatePath);
        JObject json = JObject.Parse(jsonText);

        // `changes` が null の場合は処理を中断
        if (json["update"]?["changes"]?["changes"] == null) return;

        JArray changes = (JArray)json["update"]["changes"]["changes"];

        foreach (var change in changes)
        {
            int urn = change["urn"].ToObject<int>();
            if (urn != URN.Entity.BUILDING) continue; //建物以外は無視

            int entityID = change["entityID"].ToObject<int>();

            // `change["properties"]` が null の場合はスキップ
            if (change["properties"] == null)
            {
                Debug.LogWarning($"エンティティ {entityID} のプロパティが見つかりません。");
                continue;
            }

            int brokenness = 0;

            foreach (var prop in change["properties"])
            {
                if (prop["intValue"] != null)
                {
                    brokenness = prop["intValue"].ToObject<int>();
                }
            }

            buildingGraph[entityID].brokenness = brokenness;

            // 色変更
            if (buildingRenderers.TryGetValue(entityID, out var renderer))
            {
                renderer.material.color = GetColorByBrokenness(brokenness);
            }
        }
    }

    

    // 各階の床メッシュを作成
    List<Mesh> MakeFloorMeshes(List<Vector3> list)
    {
        int count = list.Count;
        int triCount = (count - 2) * 3; // 三角形数（扇形に分割）
        List<Mesh> meshes = new List<Mesh>();

        Vector3[] vertices = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            vertices[i] = list[i];
            vertices[i].y = 0;
        }

        // 三角形インデックス（扇状分割）
        int[] tris = new int[triCount];
        for (int i = 0, j = 1; i < triCount; i += 3, j++)
        {
            tris[i] = 0;
            tris[i + 1] = j;
            tris[i + 2] = j + 1;
            // 上向きになるように法線チェックして反転
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

        // メッシュ作成
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        meshes.Add(mesh);

        return meshes;
    }

    // 複数メッシュを1つに統合する
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

    Color GetColorByBrokenness(int brokenness)
    {
        float alpha = 1.0f;
        //if (brokenness >= 80) return new Color(128/255f, 0, 128/255f, alpha);  // 紫
        if (brokenness >= 75) return new Color(1f, 0, 0, alpha);
        //if (brokenness >= 40) return new Color(1f, 153/255f, 0, alpha);         // オレンジ
        if (brokenness >= 50) return new Color(1f, 153/255f, 0, alpha);
        return new Color(123/255f, 123/255f, 123/255f, alpha);                  // 灰色
    }

    public Dictionary<int, BuildingNode> getBuildingGraph()
    {
        return buildingGraph;
    }

}
