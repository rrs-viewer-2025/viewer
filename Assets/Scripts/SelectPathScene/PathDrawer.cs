using UnityEngine;
using System.Collections.Generic;

public class PathDrawer : MonoBehaviour
{
    public Material lineMaterial; // インスペクタから設定してね
    Dictionary<int, RoadNode> roadGraph;

    public void SetRoadGragh(Dictionary<int, RoadNode> graph)
    {
        roadGraph = graph;
    }

    public void DrawPath(List<int> path, string layerName)
    {
        GameObject go = new GameObject("PathLine_" + layerName);
        var lr = go.AddComponent<LineRenderer>();

        lr.positionCount = path.Count;
        lr.material = lineMaterial;
        lr.widthMultiplier = 5.0f;
        lr.useWorldSpace = true;

        for (int i = 0; i < path.Count; i++)
        {
            lr.SetPosition(i, roadGraph[path[i]].posi);
        }

        // レイヤー設定
        int layer = LayerMask.NameToLayer(layerName);
        go.layer = layer;
    }
}
