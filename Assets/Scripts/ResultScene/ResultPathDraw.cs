using UnityEngine;
using System.Collections.Generic;

public class ResultPathDraw : MonoBehaviour
{
    Dictionary<int, RoadNode> roadGraph = Globaldata.roadGraph;
    List<int> path = Globaldata.path;
    List<Vector3> playerPath = Globaldata.playerposi;

    public Material lineMaterial1; // インスペクタから設定
    public Material lineMaterial2; // インスペクタから設定

    public void PathDraw()
    {
        GameObject go1 = new GameObject("PathLine1");
        var lr1 = go1.AddComponent<LineRenderer>();

        GameObject go2 = new GameObject("PathLine2");
        var lr2 = go2.AddComponent<LineRenderer>();

        lr1.positionCount = path.Count;
        lr1.material = lineMaterial1;
        lr1.widthMultiplier = 5.0f;
        lr1.useWorldSpace = true;

        lr2.positionCount = playerPath.Count;
        lr2.material = lineMaterial2;
        lr2.widthMultiplier = 5.0f;
        lr2.useWorldSpace = true;

        for (int i = 0; i < path.Count; i++)
        {
            lr1.SetPosition(i, roadGraph[path[i]].posi);
        }

        int j = -1;
        foreach(Vector3 posi in playerPath)
        {
            j++;
            lr2.SetPosition(j, posi);
        }
    }
}
