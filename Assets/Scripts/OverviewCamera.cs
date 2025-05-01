using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class OverviewCamera : MonoBehaviour
{
    public float padding = 10f;   // 余白
    string logfolder;
    int? Max_X = null, Min_X = null;
    int? Max_Y = null, Min_Y = null;
    Camera cam;
    PlaneManager planeManager;

    void Awake()
    {
        cam = GetComponent<Camera>();
        planeManager = FindObjectOfType<PlaneManager>();
    }

    public void SetLogFolderPath(string path)
    {
        logfolder = path;
    }

    public void SetOverviewCamera()
    {
        string filePath = logfolder + "/INITIAL_CONDITIONS.json";
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JArray entities = (JArray)json["initialCondition"]["entities"];

            foreach (var entity in entities)
            {
                int urn = entity["urn"].ToObject<int>();
                if (urn == URN.Entity.BUILDING) // 建物の場合 (URN 4356)
                {
                    int x = 0, y = 0;
                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == 4614) x = prop["intValue"].ToObject<int>(); // X座標
                        if (propUrn == 4615) y = prop["intValue"].ToObject<int>(); // Y座標
                    }

                    MaxMinX(x);
                    MaxMinY(y);
                }
            }

            SetCameraView(Max_X, Min_X, Max_Y, Min_Y);
        }
    }

    void MaxMinX(int x)
    {
        if(Max_X == null && Min_X == null)
        {
            Max_X = x;
            Min_X = x;
        }
        else
        {
            if(Max_X < x)
            {
                Max_X = x;
            }
            if(Min_X > x)
            {
                Min_X = x;
            }
        }
    }
    void MaxMinY(int y)
    {
        if(Max_Y == null && Min_Y == null)
        {
            Max_Y = y;
            Min_Y = y;
        }
        else
        {
            if(Max_Y < y)
            {
                Max_Y = y;
            }
            if(Min_Y > y)
            {
                Min_Y = y;
            }
        }
    }

    void SetCameraView(int? maxX, int? minX, int? maxY, int? minY)
    {
        if (maxX.HasValue && minX.HasValue && maxY.HasValue && minY.HasValue)
        {
            // 地図の範囲を計算
            float width = (float)(maxX.Value - minX.Value) / 1000f;
            float height = (float)(maxY.Value - minY.Value) / 1000f;
            // int? を取り出し、float に変換して計算
            float centerX = ((float)(maxX.Value + minX.Value)) / 2f / 1000f;
            float centerZ = ((float)(maxY.Value + minY.Value)) / 2f / 1000f;

            //PlaneManagerに渡す用
            Vector3 center = new Vector3(centerX, -0.001f, centerZ); //中心座標

            //PlaneManagerスクリプトのsetPlane関数に中心座標，横幅，縦幅を渡して実行
            planeManager.setPlane(center, width, height);

            
            // Cameraの位置を設定
            Vector3 posi = new Vector3(centerX, CalculateCameraHeight(width, height), centerZ);
            cam.transform.position = posi;

            // カメラを真下に向ける (x軸回転で90度)
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // 視野角 (FOV) を調整して全体を映す
            cam.fieldOfView = CalculateFieldOfView(width, height);
        }
    }

    // 高さを計算する(地図のサイズに基づいて調整)
    float CalculateCameraHeight(float width, float height)
    {
        // 高さを地図の高さに基づいて決める (例えば、広い地図ならカメラを高く)
        // 基準値を調整して最適化可能
        float baseHeight = Mathf.Max(width, height) * 0.3f; // 高さは地図の幅や高さの10%に設定
        return baseHeight + 50f; // 少し余裕を持たせて調整
    }

    // 視野角 (FOV) を計算するメソッド
    float CalculateFieldOfView(float width, float height)
    {
        // 地図のサイズに基づいてFOVを調整 (サイズが大きいほどFOVを広く設定)
        float aspectRatio = (float)Screen.width / Screen.height;
        float fov = Mathf.Atan2(Mathf.Max(width, height), 2f * CalculateCameraHeight(width, height)) * Mathf.Rad2Deg;
        return Mathf.Min(fov * aspectRatio, 90f); // 最大で90度を設定
    }
}
