using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RandomBuildingGenerator : MonoBehaviour
{
    public int numberOfFloors = 3;  // 階層数
    public int numberOfVerticesPerFloor = 6; // 各階の頂点数（多角形）
    public float floorHeight = 3.0f;  // 各階の高さ
    public float radius = 5.0f;  // 頂点のランダム配置の半径

    void Start()
    {
        // メッシュ生成
        List<Vector3[]> floorVertices = GenerateRandomFloors(numberOfFloors, numberOfVerticesPerFloor, radius, floorHeight);
        GenerateBuilding(floorVertices);
    }

    // ランダムな多角形を生成するメソッド
    List<Vector3[]> GenerateRandomFloors(int numFloors, int numVertices, float radius, float floorHeight)
    {
        List<Vector3[]> floorVertices = new List<Vector3[]>();

        for (int i = 0; i < numFloors; i++)
        {
            Vector3[] vertices = new Vector3[numVertices];
            float angleStep = Mathf.PI * 2 / numVertices;

            for (int j = 0; j < numVertices; j++)
            {
                // ランダムな角度で頂点を配置
                float angle = angleStep * j + Random.Range(-0.1f, 0.1f);  // ランダムに少し角度をずらす
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                float y = i * floorHeight;  // 各階ごとに高さを設定

                vertices[j] = new Vector3(x, y, z);
            }

            floorVertices.Add(vertices);
        }

        return floorVertices;
    }

    // 建物のメッシュを生成
    public void GenerateBuilding(List<Vector3[]> floorVertices)
    {
        if (floorVertices == null || floorVertices.Count < 2)
        {
            Debug.LogError("❌ floorVertices が空か、1階しかないため建物を作成できません！");
            return;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard")) { color = Color.red }; // 赤色のマテリアル

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int vertexOffset = 0;

        for (int i = 0; i < floorVertices.Count - 1; i++)
        {
            Vector3[] baseFloor = floorVertices[i];     // 現在の階の頂点
            Vector3[] topFloor = floorVertices[i + 1]; // その上の階の頂点

            int n = baseFloor.Length;
            if (topFloor.Length != n)
            {
                Debug.LogError("❌ 各階の頂点数が一致していません！");
                return;
            }

            // 各階の頂点を追加
            vertices.AddRange(baseFloor);
            vertices.AddRange(topFloor);

            // 側面の作成
            for (int j = 0; j < n; j++)
            {
                int next = (j + 1) % n;

                // 下の階の三角形
                triangles.Add(vertexOffset + j);
                triangles.Add(vertexOffset + next);
                triangles.Add(vertexOffset + j + n);

                triangles.Add(vertexOffset + next);
                triangles.Add(vertexOffset + next + n);
                triangles.Add(vertexOffset + j + n);

                // 上の階の三角形（側面を作成）
                triangles.Add(vertexOffset + j + n); 
                triangles.Add(vertexOffset + next + n); 
                triangles.Add(vertexOffset + next); 
                
                triangles.Add(vertexOffset + j + n); 
                triangles.Add(vertexOffset + next); 
                triangles.Add(vertexOffset + j);
            }

            // 底面（下の階）の三角形を追加
            AddPolygonTriangles(ref triangles, baseFloor, vertexOffset);

            // 上面（上の階）の三角形を追加
            AddPolygonTriangles(ref triangles, topFloor, vertexOffset + n);

            vertexOffset += n * 2;
        }

        // メッシュに設定
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        Debug.Log("✅ 建物のメッシュを生成完了");
    }

    // ポリゴンの三角形を追加するメソッド（底面や上面）
    void AddPolygonTriangles(ref List<int> triangles, Vector3[] polygon, int offset)
    {
        int n = polygon.Length;
        for (int i = 1; i < n - 1; i++)
        {
            triangles.Add(offset);            // 中心の頂点
            triangles.Add(offset + i);        // 1番目の頂点
            triangles.Add(offset + i + 1);    // 2番目の頂点
        }
    }
}
