using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    private string logfolder; // ログのパスを入れる変数
    OverviewCamera overviewCamera;
    Road road;
    MinimapCameraFitter minimapcamerafitter;
    AStar astar;
    PathFind_WidthBase pfwb;
    PathDrawer pathdrawer;

    Dictionary<int, RoadNode> roadGraph = new Dictionary<int, RoadNode>();
    List<int> Path1 = new List<int>(); //AStarで探索した経路
    List<int> Path2 = new List<int>();

    void Awake()
    {
        overviewCamera = FindObjectOfType<OverviewCamera>();
        road = FindObjectOfType<Road>();
        minimapcamerafitter = FindObjectOfType<MinimapCameraFitter>();
        astar = FindObjectOfType<AStar>();
        pfwb = FindObjectOfType<PathFind_WidthBase>();
        pathdrawer = FindObjectOfType<PathDrawer>();
    }

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;

        road.SetLogFolderPath(logfolder);
        overviewCamera.SetLogFolderPath(logfolder);

        simulation();
        pathfind();
        pathdraw();
    }

    void simulation()
    {
        road.LoadInitialConditions();
        road.CombineAllMeshes();
        roadGraph = road.getRoadGragh();

        Debug.Log("roadGraphの中身をチェック");
        foreach(int i in roadGraph.Keys)
        {
            Debug.Log(i);
        }

        overviewCamera.SetOverviewCamera();
        minimapcamerafitter.FitCamera();
    }

    void pathfind()
    {
        int start = 1782;
        int goal = 18733;

        RoadNode node = new RoadNode();
        node.EntityID = goal;
        node.posi = new Vector3(54142 / 1000f, 0, 284027 / 1000f);
        node.neighbours.Add(18749);
        roadGraph[goal] = node;

        astar.SetStartGoal(start,goal);
        astar.SetRoadGragh(roadGraph);
        pfwb.SetStartGoal(start,goal);
        pfwb.SetRoadGragh(roadGraph);

        Path1 = astar.FindPath();
        Path2 = pfwb.FindPath();

        Debug.Log("Pathの中身チェック");
        foreach (int i in Path1)
        {
            Debug.Log(i);
        }
    }

    void pathdraw()
    {
        pathdrawer.SetRoadGragh(roadGraph);
        pathdrawer.DrawPath(Path1, "path1");
        pathdrawer.DrawPath(Path2, "path2");
    }
}