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
    List<int> Path4 = new List<int>();

    void Awake()
    {
        overviewCamera = FindObjectOfType<OverviewCamera>();
        road = FindObjectOfType<Road>();
        building = FindObjectOfType<Building>();
        refuge = FindObjectOfType<Refuge>();
        minimapcamerafitter = FindObjectOfType<MinimapCameraFitter>();

        astar = FindObjectOfType<AStar>();
        pfwb = FindObjectOfType<PathFind_WidthBase>();
        pfbb = FindObjectOfType<PathFind_BrokennessBase>();
        pathdrawer = FindObjectOfType<PathDrawer>();
    }

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        logfolder = setting.LogfolderPath;

        road.SetLogFolderPath(logfolder);
        overviewCamera.SetLogFolderPath(logfolder);
        building.SetLogFolderPath(logfolder);
        refuge.SetLogFolderPath(logfolder);

        simulation();
        pathfind();
        pathdraw();
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
}