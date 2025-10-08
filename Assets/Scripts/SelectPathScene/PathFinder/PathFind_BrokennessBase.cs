using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PathFind_BrokennessBase : MonoBehaviour
{
    Dictionary<int, RoadNode> roadGraph = new Dictionary<int, RoadNode>();
    Dictionary<int, BuildingNode> buildingGraph = new Dictionary<int, BuildingNode>();

    List<int> PathList = new List<int>(); //経路のIDを格納
    List<int> L1 = new List<int>(); //これから探索するリスト
    List<int> L2 = new List<int>(); //探索済のリスト

    public int startID; //スタートの道路ID
    public int goalID; //ゴールのID

    public float rad = 7f; // 半径

    public void SetStartGoal(int start, int goal)
    {
        startID = start;
        goalID = goal;
    }

    public void SetRoadGraph(Dictionary<int, RoadNode> dictionary)
    {
        roadGraph = dictionary;
    }

    public void SetBuildingGraph(Dictionary<int, BuildingNode> dictionary)
    {
        buildingGraph = dictionary;
    }

    public List<int> FindPath()
    {
        L1.Add(startID); //出発地点のIDをL1に格納
        PathList.Add(startID);
        int n; //節点

        Dictionary<int, int> parent = new Dictionary<int, int>(); // 経路復元用
        Dictionary<int, double> gScore = new Dictionary<int, double>(); // g(n)
        gScore[startID] = 0;


        while(true)
        {
            //L1が空かチェック
            if(L1.Count == 0)
            {
                Debug.Log("探索失敗");
                return null; //終了
            }

            //L1の先頭を取り出しL2に格納
            n = L1[0];
            L1.RemoveAt(0);
            L2.Add(n);
            
            //ゴール判定
            if(n==goalID)
            {
                Debug.Log("探索成功");
                // 経路復元
                PathList.Clear();
                int current = goalID;
                while (parent.ContainsKey(current))
                {
                    PathList.Add(current);
                    current = parent[current];
                }
                PathList.Add(startID);
                PathList.Reverse();
                return PathList;
            }

            //隣接している道路があるかチェック
            if(roadGraph[n].neighbours.Count == 0)
            {
                Debug.Log("展開不可");
                continue; //以降の処理をスキップ
            }

            //隣接道路の各コストの計算
            foreach(int neighbourID in roadGraph[n].neighbours)
            {
                //隣接道路が探索済みの場合は次
                if(L2.Contains(neighbourID)) continue;

                if (!roadGraph.ContainsKey(neighbourID))
                {
                    // 道路でない場合は無視
                    continue;
                }

                // --- コスト計算 ---
                // 倒壊度をどれくらい重要視するのか
                double alpha = 3.0;

                // 距離
                double distance = Vector3.Distance(roadGraph[n].posi, roadGraph[neighbourID].posi);
                
                // 建物の倒壊度を取得
                int brokenness = getBrokenness(roadGraph[neighbourID].posi);

                // 0〜1に変換
                float dangerFactor = brokenness / 100f;

                // コスト計算
                double brokennessCost = 1.0 + dangerFactor * alpha;
                double G = gScore[n] + distance * brokennessCost;

                // L1に未登録 or より良い経路なら更新
                if (!L1.Contains(neighbourID) || G < gScore.GetValueOrDefault(neighbourID, double.MaxValue))
                {
                    gScore[neighbourID] = G;
                    parent[neighbourID] = n;

                    // 推定コスト H（ゴールまでの直線距離）
                    double H = Vector3.Distance(roadGraph[neighbourID].posi, roadGraph[goalID].posi);

                    // 優先度順に挿入
                    if (!L1.Contains(neighbourID))
                        L1.Add(neighbourID);

                    L1 = L1.OrderBy(id => gScore[id] + H).ToList();
                }
            }
        }
    }

    // 建物の倒壊度を取得する処理
    int getBrokenness(Vector3 road_posi)
    {
        int maxBrokenness = 0;

        foreach (var building in buildingGraph.Values)
        {
            // 頂点のどれかが半径内なら対象
            foreach (var apex in building.apexes)
            {
                float distance = Vector3.Distance(apex, road_posi);
                
                if (distance <= rad)
                {
                    if (building.brokenness > maxBrokenness)
                        maxBrokenness = building.brokenness;
                    break; // 建物は1回だけチェック
                }
            }
        }

        // Debug.Log($"倒壊度: {maxBrokenness}");
        return maxBrokenness;
    }

}