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
    Building building;
    Refuge refuge;
    MinimapCameraFitter minimapcamerafitter;

    AStar astar;
    PathFind_WidthBase pfwb;
    PathFind_BrokennessBase pfbb;
    PathDrawer pathdrawer;

    Dictionary<int, RoadNode> roadGraph = new Dictionary<int, RoadNode>();
    Dictionary<int, BuildingNode> buildingGraph = new Dictionary<int, BuildingNode>();
    Dictionary<int, RefugeNode> refugeGraph = new Dictionary<int, RefugeNode>();

    List<int> Path1 = new List<int>(); //AStarで探索した経路
    List<int> Path2 = new List<int>();
    List<int> Path3 = new List<int>();

    void Awake()
    {
        overviewCamera = Object.FindFirstObjectByType<OverviewCamera>();
        road = Object.FindFirstObjectByType<Road>();
        building = Object.FindFirstObjectByType<Building>();
        refuge = Object.FindFirstObjectByType<Refuge>();
        minimapcamerafitter = Object.FindFirstObjectByType<MinimapCameraFitter>();

        astar = Object.FindFirstObjectByType<AStar>();
        pfwb = Object.FindFirstObjectByType<PathFind_WidthBase>();
        pfbb = Object.FindFirstObjectByType<PathFind_BrokennessBase>();
        pathdrawer = Object.FindFirstObjectByType<PathDrawer>();
    }

    void Start()
    {
        Setting setting = Object.FindFirstObjectByType<Setting>();
        logfolder = setting.LogfolderPath;

        road.SetLogFolderPath(logfolder);
        overviewCamera.SetLogFolderPath(logfolder);
        building.SetLogFolderPath(logfolder);
        refuge.SetLogFolderPath(logfolder);

        simulation();
        pathfind();
        pathdraw();

        Globaldata.roadGraph = roadGraph;
    }

    void simulation()
    {
        road.LoadInitialConditions();
        road.CombineAllMeshes();
        roadGraph = road.getRoadGraph();

        building.LoadInitialConditions();
        building.SetBrokennes();
        buildingGraph = building.getBuildingGraph();

        refuge.LoadAndDrawBuilding();
        refugeGraph = refuge.getRefugeGraph();

        overviewCamera.SetOverviewCamera();
        minimapcamerafitter.FitCamera();
    }

    void pathfind()
    {
        int start = 1782;
        int goal = 18733;

        foreach(var refuge in refugeGraph)
        {
            RoadNode node = new RoadNode();
            node.EntityID = refuge.Value.EntityID;
            node.posi = refuge.Value.posi;
            roadGraph[node.EntityID] = node;
        }

        astar.SetStartGoal(start,goal);
        astar.SetRoadGraph(roadGraph);

        pfwb.SetStartGoal(start,goal);
        pfwb.SetRoadGraph(roadGraph);

        pfbb.SetStartGoal(start, goal);
        pfbb.SetRoadGraph(roadGraph);
        pfbb.SetBuildingGraph(buildingGraph);

        Path1 = astar.FindPath();
        Path2 = pfwb.FindPath();
        Path3 = pfbb.FindPath();
    }

    void pathdraw()
    {
        pathdrawer.SetRoadGragh(roadGraph);
        pathdrawer.DrawPath(Path1, "path1");
        pathdrawer.DrawPath(Path2, "path2");
        pathdrawer.DrawPath(Path3, "path3");
    }

    public List<int> getPath(int i)
    {
        if(i == 1)
        {
            Debug.Log("path1をセット");
            return Path1;
        } 
        if(i == 2)
        {
            Debug.Log("path2をセット");
            return Path2;
        }
        if(i == 3)
        {
            Debug.Log("path3をセット");
            return Path3;
        }
        return new List<int>();
    }
}