using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class ResultRoad : MonoBehaviour
{
    private string logfolder;
    private List<Mesh> wallMeshes = new List<Mesh>(); //壁のメッシュリスト
    private List<Vector3> apexes;
    bool flag = false;
    //PlayerStartPosition playerStartPosition;

    void Awake()
    {
        //playerStartPosition = FindObjectOfType<PlayerStartPosition>();
    }

    void Start()
    {
        //CombineAllMeshes(); // すべての壁を統合
    }

    public void SetMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // 🔹 デフォルトマテリアルを適用（インスペクタで設定も可能）
        if (meshRenderer.sharedMaterial == null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.gray; // 灰色で表示されるように
            meshRenderer.sharedMaterial = mat;
        }

        meshFilter.mesh = new Mesh();
    }

    public void SetLogFolderPath(string path)
    {
        logfolder = path;
    }

    public void LoadInitialConditions()
    {
        Debug.Log("実行開始");
        string filePath = logfolder + "/INITIAL_CONDITIONS.json";
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JArray entities = (JArray)json["initialCondition"]["entities"];

            foreach (var entity in entities)
            {
                int urn = entity["urn"].ToObject<int>();
                
                apexes = new List<Vector3>();
                if (urn == URN.Entity.ROAD) // 建物の場合 (URN 4356)
                {
                    int apexesCount = 0; // 頂点のカウント
                    
                    Vector3 startposition = new Vector3();
                    Vector3 endposition = new Vector3();
                    int x = 0, y = 0;

                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == URN.Property.EDGES)
                        {
                            var edgeList = prop["edgeList"]["edges"];
                            foreach (var edge in edgeList)
                            {
                                int startX = edge["startX"].ToObject<int>();
                                int startY = edge["startY"].ToObject<int>();
                                int endX = edge["endX"].ToObject<int>();
                                int endY = edge["endY"].ToObject<int>();

                                startposition = new Vector3(startX / 1000f, 0, startY / 1000f);
                                endposition = new Vector3(endX / 1000f, 0, endY / 1000f);

                                apexes.Add(startposition);
                                apexesCount++;
                            }
                        }
                        if (propUrn == URN.Property.X) x = prop["intValue"].ToObject<int>();
                        if (propUrn == URN.Property.Y) y = prop["intValue"].ToObject<int>(); 
                    }
                    MakeFloorMesh(apexes, apexesCount);
                    
                    // if(flag == false)
                    // {
                    //     flag = playerStartPosition.check(x, y);
                    // }
                }
            }
        }
        CombineAllMeshes();

    }

    void MakeFloorMesh(List<Vector3> list, int count)
    {
        int aaa = (count - 2) * 3; //meshは三角形をベースに埋めてく．これは必要な三角形の数＊頂点
        Vector3[] apexes = new Vector3[count];
        for(int j = 0; j < count; j++)
        {
            Vector3 posi = list[j];
            apexes[j] = posi;
        }
            
        int[] mytriangles = new int[aaa];

        for (int k = 0, j = 1; k < aaa; k += 3, j++)
        {
            // 三角形のインデックスを設定（最初は通常の順序）
            mytriangles[k] = 0;
            mytriangles[k + 1] = j;
            mytriangles[k + 2] = j + 1;

            // 法線を計算
            Vector3 normal = Vector3.Cross(
                apexes[mytriangles[k + 1]] - apexes[mytriangles[k]],
                apexes[mytriangles[k + 2]] - apexes[mytriangles[k]]
            );

            // 反時計回りチェック
            if (Vector3.Dot(normal, Vector3.up) < 0) // 時計回りの場合
            {
                // 頂点1と頂点2を入れ替える
                int temp = mytriangles[k + 1];
                mytriangles[k + 1] = mytriangles[k + 2];
                mytriangles[k + 2] = temp;
                
            }
        }

        Mesh myMesh = new Mesh();
        myMesh.vertices = apexes;
        myMesh.triangles = mytriangles;
        myMesh.RecalculateNormals();

        wallMeshes.Add(myMesh);
    }

    public void CombineAllMeshes()
    {
        Debug.Log("combineAllMeshes実行");
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (wallMeshes.Count == 0)
        {
            Debug.Log("meshが空です");
            return;
        }
            

        CombineInstance[] combine = new CombineInstance[wallMeshes.Count];

        for (int i = 0; i < wallMeshes.Count; i++)
        {
            combine[i].mesh = wallMeshes[i];
            combine[i].transform = Matrix4x4.identity;
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine, true, false);

        meshFilter.mesh = finalMesh;
    }
}
