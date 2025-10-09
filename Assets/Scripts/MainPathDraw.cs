using UnityEngine;
using System.Collections.Generic;

public class MainPathDraw : MonoBehaviour
{
    Dictionary<int, RoadNode> roadGraph = Globaldata.roadGraph;
    List<int> path = Globaldata.path;
    public GameObject prefab;

    public void PathDraw()
    {
        for (int i = 0; i < path.Count; i++)
        {
            Instantiate(prefab, roadGraph[path[i]].posi, Quaternion.identity);
        }
    }
}
