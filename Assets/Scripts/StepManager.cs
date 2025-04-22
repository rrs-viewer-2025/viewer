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
    private int maxStep; //最大のステップ数
    private string logfolder; // ログのパスを入れる変数
    AmbulanceteamLoader ambulanceteamLoader;
    PoliceforceLoader policeforceLoader;
    FirebrigadeLoader firebrigadeLoader;
    CivilianLoader civilianLoader;
    BlockadeLoader blockadeLoader;

    void Awake() //確実に準備させるもの
    {
        ambulanceteamLoader = FindObjectOfType<AmbulanceteamLoader>();
        policeforceLoader = FindObjectOfType<PoliceforceLoader>();
        firebrigadeLoader = FindObjectOfType<FirebrigadeLoader>();
        civilianLoader = FindObjectOfType<CivilianLoader>();
        blockadeLoader = FindObjectOfType<BlockadeLoader>();

        Setting setting = FindObjectOfType<Setting>();
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

    }

    void Start()
    {
        ambulanceteamLoader.SetLogFolderPath(logfolder);
        policeforceLoader.SetLogFolderPath(logfolder);
        firebrigadeLoader.SetLogFolderPath(logfolder);
        civilianLoader.SetLogFolderPath(logfolder);
        blockadeLoader.SetLogFolderPath(logfolder);

        ambulanceteamLoader.LoadInitialConditions();
        policeforceLoader.LoadInitialConditions();
        firebrigadeLoader.LoadInitialConditions();
        civilianLoader.LoadInitialConditions();
        
        StartStep(); // 一度だけ

        // リセットボタンの設定
        Button resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
        resetButton.onClick.AddListener(ResetSimulation); // リセットボタンをクリックしたときにResetSimulationを呼び出す
    }

    public int GetCurrentStep() //他のステップからこいつを呼び出す
    {
        return currentStep;
    }

    public void NotifyCompleted() //他のスクリプトの処理が完了したらこいつを実行
    {
        completedCount++; //完了した数のカウント
        Debug.Log($"Advancing to step {currentStep}"); // ここでcurrentStepが更新されているか確認
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

        if (currentStep <= maxStep)
        {
            Debug.Log($"Step advanced! Now at Step {currentStep}");
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
        // FindObjectOfType<Script1>().StartStep();
        // FindObjectOfType<Script2>().StartStep();

        ambulanceteamLoader.StartStep();
        policeforceLoader.StartStep();
        civilianLoader.StartStep();
        firebrigadeLoader.StartStep();
        blockadeLoader.StartStep();
    }

    // リセット処理
    void ResetSimulation()
    {
        currentStep = 1; // ステップを1に戻す
    }
}
