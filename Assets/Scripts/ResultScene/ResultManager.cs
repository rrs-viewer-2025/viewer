using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
{
    private string logfolder;
    OverviewCamera overviewCamera;
    ResultRoad road;
    Building building;
    Refuge refuge;
    MinimapCameraFitter minimapcamerafitter;
    ResultPathDraw rpd;


    void Awake()
    {
        overviewCamera = Object.FindFirstObjectByType<OverviewCamera>();
        road = Object.FindFirstObjectByType<ResultRoad>();
        building = Object.FindFirstObjectByType<Building>();
        refuge = Object.FindFirstObjectByType<Refuge>();
        minimapcamerafitter = Object.FindFirstObjectByType<MinimapCameraFitter>();
        rpd = Object.FindFirstObjectByType<ResultPathDraw>();
    }

    void Start()
    {
        Setting setting = Object.FindFirstObjectByType<Setting>();
        logfolder = setting.LogfolderPath;

        road.SetLogFolderPath(logfolder);
        overviewCamera.SetLogFolderPath(logfolder);
        building.SetLogFolderPath(logfolder);
        refuge.SetLogFolderPath(logfolder);

        road.SetMesh();

        simulation();
        pathdraw();
    }

    void simulation()
    {
        road.LoadInitialConditions();

        building.LoadInitialConditions();
        building.SetBrokennes();

        refuge.LoadAndDrawBuilding();

        overviewCamera.SetOverviewCamera();
        minimapcamerafitter.FitCamera();
    }

    void pathdraw()
    {
        rpd.PathDraw();
    }
}
