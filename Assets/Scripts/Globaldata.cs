using UnityEngine;
using System.Collections.Generic;

public static class Globaldata
{
    // 静的変数　どのシーンからもアクセスできる
    public static List<int> path = new List<int>();
    public static Dictionary<int, RoadNode> roadGraph = new Dictionary<int, RoadNode>();
    public static List<Vector3> playerposi = new List<Vector3>();
}
