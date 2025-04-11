using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class Setting : MonoBehaviour
{
    public string LogfolderPath;

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
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "config.json");

        if (System.IO.File.Exists(path))
        {
            string jsonText = System.IO.File.ReadAllText(path);
            ConfigData config = JsonConvert.DeserializeObject<ConfigData>(jsonText);
            LogfolderPath = config.logFolderPath;
            Debug.Log($"[Setting.cs] LogfolderPath loaded: {LogfolderPath}");
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

        // ✅ シーンを再読み込み
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
