using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
public class StepManager : MonoBehaviour
{
    private int currentStep = 1;
    private int scriptsToWait = 5; //ステップごとの実行するスクリプトの数
    private int completedCount = 0; //完了したスクリプトの数
    private int resetcompletedCount = 0; //リセット処理が完了したスクリプトの数
    private int maxStep; //最大のステップ数
    private string logfolder; // ログのパスを入れる変数
    AmbulanceteamLoader ambulanceteamLoader;
    PoliceforceLoader policeforceLoader;
    FirebrigadeLoader firebrigadeLoader;
    CivilianLoader civilianLoader;
    BlockadeLoader blockadeLoader;
    OverviewCamera overviewCamera;
    RefugeCamera refugeCamera;
    RoadMesh roadMesh;
    MainPathDraw mpd;
    PlayerPosition pp;

    void Awake() //確実に準備させるもの
    {
        ambulanceteamLoader = FindFirstObjectByType<AmbulanceteamLoader>();
        policeforceLoader = FindFirstObjectByType<PoliceforceLoader>();
        firebrigadeLoader = FindFirstObjectByType<FirebrigadeLoader>();
        civilianLoader = FindFirstObjectByType<CivilianLoader>();
        blockadeLoader = FindFirstObjectByType<BlockadeLoader>();
        overviewCamera = FindFirstObjectByType<OverviewCamera>();
        refugeCamera = FindFirstObjectByType<RefugeCamera>();
        roadMesh = FindFirstObjectByType<RoadMesh>();
        mpd = FindFirstObjectByType<MainPathDraw>();
        pp = FindFirstObjectByType<PlayerPosition>();
    }

    void Start()
    {
        Setting setting = FindFirstObjectByType<Setting>();
        logfolder = setting.LogfolderPath;

        string filePath = logfolder + "/CONFIG.json";
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JObject data = (JObject)json["config"]?["config"]?["data"];

            if (data != null && data["kernel.timesteps"] != null)
            {
                maxStep = int.Parse(data["kernel.timesteps"].ToString());
            }
        }

        ambulanceteamLoader.SetLogFolderPath(logfolder);
        policeforceLoader.SetLogFolderPath(logfolder);
        firebrigadeLoader.SetLogFolderPath(logfolder);
        civilianLoader.SetLogFolderPath(logfolder);
        blockadeLoader.SetLogFolderPath(logfolder);
        overviewCamera.SetLogFolderPath(logfolder);
        refugeCamera.SetLogFolderPath(logfolder);
        roadMesh.SetLogFolderPath(logfolder);
        simulation();
    }

    void simulation()
    {
        pp.SetPosition();
        ambulanceteamLoader.LoadInitialConditions();
        policeforceLoader.LoadInitialConditions();
        firebrigadeLoader.LoadInitialConditions();
        civilianLoader.LoadInitialConditions();
        overviewCamera.SetOverviewCamera();
        refugeCamera.SetRefugeCamera();
        roadMesh.LoadInitialConditions();
        mpd.PathDraw();
        
        StartStep(); // 一度だけ
    }

    public int GetCurrentStep() //他のステップからこいつを呼び出す
    {
        return currentStep;
    }

    public int GetMaxStep()
    {
        return maxStep;
    }

    public void NotifyCompleted() //他のスクリプトの処理が完了したらこいつを実行
    {
        completedCount++; //完了した数のカウント
        if (completedCount >= scriptsToWait) //全部のスクリプトが完了したら
        {
            AdvanceStep(); //この関数を実行
        }
        else
        {
            Debug.Log("All steps completed!");
        }
    }

    private void AdvanceStep()
    {
        currentStep++; //Stepをカウント
        completedCount = 0;

        if (currentStep < maxStep)
        {
            StartStep();
        }
        else
        {
            Debug.Log("All steps completed!");
        }
    }

    public void StartStep()
    {
        // 各ステップの開始時に Script1 / Script2 に命令
        // FindFirstObjectByType<Script1>().StartStep();
        // FindFirstObjectByType<Script2>().StartStep();

        ambulanceteamLoader.StartStep();
        policeforceLoader.StartStep();
        civilianLoader.StartStep();
        firebrigadeLoader.StartStep();
        blockadeLoader.StartStep();
    }

    // public void ResetComplete() //各スクリプトがリセット処理完了したら実行
    // {
    //     resetcompletedCount++;
    //     if(resetcompletedCount >= scriptsToWait)
    //     {
    //         simulation();
    //     }
    // }

    // リセット処理
    void ResetSimulation()
    {
        currentStep = 1; // ステップを1に戻す
        completedCount = 0;
        resetcompletedCount = 0;
        ambulanceteamLoader.Reset();
        policeforceLoader.Reset();
        civilianLoader.Reset();
        firebrigadeLoader.Reset();
        blockadeLoader.Reset();
        simulation();
    }
}
