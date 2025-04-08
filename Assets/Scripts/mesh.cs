using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class mesh : MonoBehaviour
{
    private string logfolder; // ログフォルダのパス
    public Material defaultMaterial; // 建物に使うマテリアル（Inspectorで設定可能）

    void Start()
    {
        // 設定オブジェクトからログフォルダパスを取得
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;
        LoadInitialConditions(); // 初期状態の読み込みを開始
    }

    void LoadInitialConditions()
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

            // 建物の階数と高さの初期値
            int floor = 1;
            int height = 7;
            List<Vector3> apexList = new List<Vector3>(); // 床形状の頂点リスト
            List<Mesh> meshes = new List<Mesh>(); // メッシュを蓄積するリスト

            // 階数のプロパティを取得
            foreach (var prop in entity["properties"])
            {
                if (prop["urn"].ToObject<int>() == URN.Property.FLOORS)
                    floor = prop["intValue"].ToObject<int>();
            }

            // エッジ情報を元に壁メッシュを作成
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

                        // 壁メッシュを作ってリストに追加
                        meshes.AddRange(MakeWallMeshes(floor, height, startX, startY, endX, endY));
                    }
                }
            }

            // 各階の床メッシュを作成
            meshes.AddRange(MakeFloorMeshes(floor, height, apexList));

            // 建物単位のGameObjectを作成して、子として追加
            GameObject building = new GameObject("Building_" + buildingId++);
            building.transform.parent = this.transform;

            // メッシュとマテリアルを設定
            MeshFilter mf = building.AddComponent<MeshFilter>();
            MeshRenderer mr = building.AddComponent<MeshRenderer>();

            // ここで親のマテリアルを使用
            mr.material = parentMaterial;
            
            // メッシュを統合して1つにまとめてセット
            mf.mesh = CombineMeshes(meshes);
        }
    }

    // 壁のメッシュを前後2面分作成
    List<Mesh> MakeWallMeshes(int floor, int H, int startX, int startY, int endX, int endY)
    {
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(startX / 1000f, 0, startY / 1000f);
        vertices[1] = new Vector3(startX / 1000f, floor * H, startY / 1000f);
        vertices[2] = new Vector3(endX / 1000f, floor * H, endY / 1000f);
        vertices[3] = new Vector3(endX / 1000f, 0, endY / 1000f);

        // 正面の三角形
        int[] trisFront = new int[6] { 0, 1, 2, 0, 2, 3 };
        // 背面の三角形（逆順）
        int[] trisBack = new int[6] { 0, 2, 1, 0, 3, 2 };

        Mesh mesh1 = new Mesh();
        mesh1.vertices = vertices;
        mesh1.triangles = trisFront;
        mesh1.RecalculateNormals();

        Mesh mesh2 = new Mesh();
        mesh2.vertices = vertices;
        mesh2.triangles = trisBack;
        mesh2.RecalculateNormals();

        return new List<Mesh> { mesh1, mesh2 };
    }

    // 各階の床メッシュを作成
    List<Mesh> MakeFloorMeshes(int floor, int H, List<Vector3> list)
    {
        int count = list.Count;
        int triCount = (count - 2) * 3; // 三角形数（扇形に分割）
        List<Mesh> meshes = new List<Mesh>();

        for (int f = 0; f <= floor; f++) // 0階（地面）から屋上まで
        {
            Vector3[] vertices = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                vertices[i] = list[i];
                vertices[i].y = f * H;
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
        }

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
}
