using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;

public class Setting : MonoBehaviour
{
    public string LogfolderPath;   // エディタ時のみInspectorから
    public string InterfaceType;   // エディタ時のみInspectorから

    [System.Serializable]
    private class ConfigData
    {
        public string logFolderPath;

        [JsonProperty("interface")]
        public string interfaceType;
    }

    void Awake()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");

#if UNITY_EDITOR
        // エディタ時：Inspector優先（空なら config.json を使う）
        bool needLogPath = string.IsNullOrEmpty(LogfolderPath);
        bool needInterface = string.IsNullOrEmpty(InterfaceType);

        if (File.Exists(configPath) && (needLogPath || needInterface))
        {
            string jsonText = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<ConfigData>(jsonText);

            if (needLogPath)
            {
                LogfolderPath = config.logFolderPath;
                Debug.Log($"[Setting.cs] LogfolderPath loaded from config.json: {LogfolderPath}");
            }
            if (needInterface)
            {
                InterfaceType = config.interfaceType;
                Debug.Log($"[Setting.cs] InterfaceType loaded from config.json: {InterfaceType}");
            }
        }
#else
        // ビルド後：常に config.json 優先
        if (File.Exists(configPath))
        {
            string jsonText = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<ConfigData>(jsonText);

            LogfolderPath = config.logFolderPath;
            InterfaceType = config.interfaceType;

            Debug.Log($"[Setting.cs] Loaded from config.json (build): LogfolderPath = {LogfolderPath}, InterfaceType = {InterfaceType}");
        }
        else
        {
            Debug.LogError($"[Setting.cs] config.json not found at: {configPath}");
        }
#endif

        // 最終ログ
        Debug.Log($"[Setting.cs] Final: LogfolderPath = {LogfolderPath}, InterfaceType = {InterfaceType}");
    }

    public void ReloadConfig()
    {
        Debug.Log("[Setting.cs] Reloading config...");
        LoadConfig();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
