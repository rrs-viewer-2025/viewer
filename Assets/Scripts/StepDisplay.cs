using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class StepDisplay : MonoBehaviour
{
    public TextMeshProUGUI stepText; // ステップ数を表示するUIテキスト
    public Button resetButton; // リセットボタン
    private string logFolder; // ログフォルダのパス
    public float updateInterval = 1.0f; // ステップ更新の間隔（秒）

    private List<int> stepNumbers = new List<int>(); // ステップフォルダのリスト
    private int currentStepIndex = 0;
    private bool isRunning = true; // ステップ更新が実行中かどうか

    void Start()
    {
        // 設定クラスからログフォルダのパスを取得
        Setting setting = FindObjectOfType<Setting>();
        logFolder = setting.LogfolderPath;

        LoadStepNumbers(); // ログフォルダからステップ情報を取得

        if (stepText == null)
        {
            Debug.LogError("stepText がセットされていません！");
            enabled = false;
            return;
        }

        if (resetButton == null)
        {
            Debug.LogError("resetButton がセットされていません！");
            enabled = false;
            return;
        }

        if (stepNumbers.Count == 0)
        {
            Debug.LogError("有効なステップフォルダが見つかりません！");
            enabled = false;
            return;
        }

        // リセットボタンにリスナーを追加
        resetButton.onClick.AddListener(ResetSimulation);

        // ステップ更新ループ開始
        StartCoroutine(UpdateStepLoop());
    }

    /// <summary>
    /// ログフォルダ内のステップフォルダを取得
    /// </summary>
    void LoadStepNumbers()
    {
        if (!Directory.Exists(logFolder))
        {
            Debug.LogError($"ログフォルダが見つかりません: {logFolder}");
            return;
        }

        string[] directories = Directory.GetDirectories(logFolder);
        foreach (string dir in directories)
        {
            string folderName = new DirectoryInfo(dir).Name;
            if (int.TryParse(folderName, out int step))
            {
                stepNumbers.Add(step);
            }
        }

        stepNumbers.Sort(); // ステップを昇順にソート
    }

    /// <summary>
    /// ステップを一定間隔で更新するコルーチン
    /// </summary>
    IEnumerator UpdateStepLoop()
    {
        while (true)
        {
            if (!isRunning || currentStepIndex >= stepNumbers.Count)
                yield break;

            int currentStep = stepNumbers[currentStepIndex];
            stepText.text = $"ステップ: {currentStep} / {stepNumbers[stepNumbers.Count - 1]}";

            currentStepIndex++;

            yield return new WaitForSeconds(updateInterval);
        }
    }

    /// <summary>
    /// リセットボタンが押されたときにステップをリセット
    /// </summary>
    void ResetSimulation()
    {
        currentStepIndex = 0; // ステップを最初に戻す
        isRunning = true; // 更新を再開

        // すでに実行中のコルーチンがない場合、再開
        StopAllCoroutines();
        StartCoroutine(UpdateStepLoop());
    }
}
