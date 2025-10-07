using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PathFind_WidthBase : MonoBehaviour
{
    Dictionary<int, RoadNode> roadGragh = new Dictionary<int, RoadNode>();
    List<int> PathList = new List<int>(); //経路のIDを格納
    List<int> L1 = new List<int>(); //これから探索するリスト
    List<int> L2 = new List<int>(); //探索済のリスト

    public int startID; //スタートの道路ID
    public int goalID; //ゴールのID

    public void SetStartGoal(int start, int goal)
    {
        startID = start;
        goalID = goal;
    }

    public void SetRoadGragh(Dictionary<int, RoadNode> dictionary)
    {
        roadGragh = dictionary;
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
            if(roadGragh[n].neighbours.Count == 0)
            {
                Debug.Log("展開不可");
                continue; //以降の処理をスキップ
            }

            //隣接道路の各コストの計算
            foreach(int neighbourID in roadGragh[n].neighbours)
            {
                //隣接道路が探索済みの場合は次
                if(L2.Contains(neighbourID)) continue;

                if (!roadGragh.ContainsKey(neighbourID))
                {
                    // 道路でない場合は無視
                    continue;
                }

                // --- 幅考慮付きコスト計算 ---
                // 道路の幅をどれくらい重要視するのか
                double alpha = 3.0;

                // 現在の道路から隣接道路までの距離
                double distance = Vector3.Distance(roadGragh[n].posi, roadGragh[neighbourID].posi);
                
                // 道路の幅
                double width = getroadwidth(neighbourID, n);
                width = Mathf.Max((float)width, 0.1f);

                // コスト計算
                double widthAdjustedCost = distance / Math.Pow(width, alpha);
                double G = gScore[n] + widthAdjustedCost;

                //L1にまだ格納してない場合，または格納されているがGがより小さい場合
                if(!L1.Contains(neighbourID) || G < gScore.GetValueOrDefault(neighbourID, double.MaxValue))
                {
                    gScore[neighbourID] = G;
                    parent[neighbourID] = n;
                    
                    //推定コストの計算
                    double H = Vector3.Distance(roadGragh[neighbourID].posi, roadGragh[goalID].posi);

                    //総コスト
                    double F = G + H;

                    if (!L1.Contains(neighbourID))
                    L1.Add(neighbourID);

                    // ソートして f が小さいノードを先頭に
                    L1 = L1.OrderBy(id => gScore[id] + Vector3.Distance(roadGragh[id].posi, roadGragh[goalID].posi)).ToList();
                }
            }
        }
    }

    double getroadwidth(int neighbourID, int n)
    {
        foreach(double neighbour_width in roadGragh[neighbourID].edgeLength)
        {
            foreach(double n_width in roadGragh[n].edgeLength)
            {
                // 隣接道路の共通の辺の長さを返す
                if(neighbour_width == n_width)
                {
                    return neighbour_width;
                }
            }
        }

        return 1.0; // 見つからなかった場合は1.0を返す
    }
}