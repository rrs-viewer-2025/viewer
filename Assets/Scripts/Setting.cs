using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;

public class Setting : MonoBehaviour
{
    public string LogfolderPath; // インスペクターから設定可能

    [System.Serializable]
    private class ConfigData
    {
        public string logFolderPath;
    }

    void Awake()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "config.json");

#if UNITY_EDITOR
        // 開発中：InspectorのLogfolderPathを優先し、ログファイル存在チェック
        if (!string.IsNullOrEmpty(LogfolderPath))
        {
            string testLogPath = Path.Combine(LogfolderPath, "INITIAL_CONDITIONS.json");
            if (File.Exists(testLogPath))
            {
                Debug.Log($"[Setting.cs] Using Inspector path: {LogfolderPath}");
                return;
            }
            else
            {
                Debug.LogWarning($"[Setting.cs] Log not found at Inspector path: {testLogPath}. Falling back to config.json.");
            }
        }
#endif

        // ビルド後、もしくは Inspectorパスが無効だったとき
        if (File.Exists(path))
        {
            string jsonText = File.ReadAllText(path);
            ConfigData config = JsonConvert.DeserializeObject<ConfigData>(jsonText);
            LogfolderPath = config.logFolderPath;
            Debug.Log($"[Setting.cs] LogfolderPath loaded from config.json: {LogfolderPath}");
        }
        else
        {
            Debug.LogError($"[Setting.cs] Config file not found at: {path}");
        }
    }

    public void ReloadConfig()
    {
        Debug.Log("[Setting.cs] Reloading config...");
        LoadConfig();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
