using UnityEngine;
using System.Collections.Generic;

public class ResultPathDraw : MonoBehaviour
{
    Dictionary<int, RoadNode> roadGraph = Globaldata.roadGraph;
    List<int> path = Globaldata.path;

    List<Vector3> player1Path = Globaldata.player1Pos;
    List<Vector3> player2Path = Globaldata.player2Pos;

    public Material lineMaterialRoad;   // 想定経路
    public Material lineMaterialP1;     // Player1
    public Material lineMaterialP2;     // Player2

    public void PathDraw()
    {
        // ===== 想定経路 =====
        CreateLine(
            "RoadPath",
            path.Count,
            lineMaterialRoad,
            i => roadGraph[path[i]].posi
        );

        // ===== Player1 経路 =====
        if (player1Path != null && player1Path.Count > 0)
        {
            CreateLine(
                "Player1Path",
                player1Path.Count,
                lineMaterialP1,
                i => player1Path[i]
            );
        }

        // ===== Player2 経路 =====
        if (player2Path != null && player2Path.Count > 0)
        {
            CreateLine(
                "Player2Path",
                player2Path.Count,
                lineMaterialP2,
                i => player2Path[i]
            );
        }
    }

    // LineRenderer生成を共通化
    void CreateLine(string name, int count, Material mat, System.Func<int, Vector3> getPos)
    {
        GameObject go = new GameObject(name);
        var lr = go.AddComponent<LineRenderer>();

        lr.positionCount = count;
        lr.material = mat;
        lr.widthMultiplier = 5.0f;
        lr.useWorldSpace = true;

        for (int i = 0; i < count; i++)
        {
            lr.SetPosition(i, getPos(i));
        }
    }
}
