using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

public class RoadNode
{
    public int EntityID; //道路ID
    public Vector3 posi; // 道路の中心座標
    public List<int> neighbours; // 隣接している道路のIDを管理するリスト

    public RoadNode()
    {
        neighbours = new List<int>();
    }
}

public class Road : MonoBehaviour
{
    private string logfolder;
    private List<Mesh> wallMeshes = new List<Mesh>(); //壁のメッシュリスト
    private List<Vector3> apexes;
    bool flag = false;
    //PlayerStartPosition playerStartPosition;

    public Dictionary<int, RoadNode> roadGragh = new Dictionary<int, RoadNode>();

    void Awake()
    {
        //playerStartPosition = FindObjectOfType<PlayerStartPosition>();
    }

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh(); // 初期化

        //CombineAllMeshes(); // すべての壁を統合
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
                RoadNode node = new RoadNode();
                node.EntityID = entity["entityID"].ToObject<int>(); //道路のエンティティID格納


                apexes = new List<Vector3>();
                if (urn == URN.Entity.ROAD) // 建物の場合 (URN 4356)
                {
                    int apexesCount = 0; // 頂点のカウント
                    
                    Vector3 position = new Vector3();
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
                                position = new Vector3(startX / 1000f, 0, startY / 1000f);
                                apexes.Add(position);
                                apexesCount++;

                                int neighbourID = edge["neighbour"].ToObject<int>();
                                if(neighbourID != 0)
                                {
                                    node.neighbours.Add(neighbourID); //隣接する道路のIDリストに格納
                                }
                            }
                        }
                        if (propUrn == URN.Property.X) x = prop["intValue"].ToObject<int>();
                        if (propUrn == URN.Property.Y) y = prop["intValue"].ToObject<int>(); 
                    }
                    MakeFloorMesh(apexes, apexesCount);
                    node.posi = position; //道路の座標格納

                    roadGragh[node.EntityID] = node;

                    // if(flag == false)
                    // {
                    //     flag = playerStartPosition.check(x, y);
                    // }
                }
            }
        }


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
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (wallMeshes.Count == 0)
            return;

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

    public Dictionary<int, RoadNode> getRoadGragh()
    {
        return roadGragh;
    }
}
