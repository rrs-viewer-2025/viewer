using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using TMPro;

public class MapNameLoader : MonoBehaviour
{
    public TextMeshProUGUI mapNameText; // マップ名を表示するTextMeshProオブジェクト
    public string configFolder; // CONFIG.jsonのフォルダパス

    void Start()
    {
        Setting setting = FindObjectOfType<Setting>();
        configFolder = setting.LogfolderPath; // 設定クラスからパスを取得
        LoadMapName();
    }

    void LoadMapName()
    {
        string filePath = configFolder + "/CONFIG.json";

        if (!File.Exists(filePath))
        {
            Debug.LogError("CONFIG.json が見つかりません: " + filePath);
            mapNameText.text = "Map: Unknown";
            return;
        }

        try
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            string mapDir = json["config"]["config"]["data"]["gis.map.dir"]?.ToString();

            if (!string.IsNullOrEmpty(mapDir))
            {
                string[] pathParts = mapDir.Split('/');
                string mapName = pathParts[pathParts.Length - 2]; // 最後から2番目のフォルダ名を取得

                if (mapNameText != null)
                {
                    if (mapName == "kobe") {
                        mapName = "こうべ";
                    }
                    mapNameText.text = "マップ: " + mapName;
                }
            }
            else
            {
                Debug.LogError("gis.map.dir の値が見つかりません。");
                mapNameText.text = "Map: Unknown";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("CONFIG.json の読み込みエラー: " + e.Message);
            mapNameText.text = "Map: Error";
        }
    }
}
