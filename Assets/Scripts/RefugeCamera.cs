using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class RefugeCamera : MonoBehaviour
{
    public Camera cam;
    string logfolder;

    void Awake()
    {
        if(cam == null) cam = GetComponent<Camera>();
    }

    public void SetLogFolderPath(string path)
    {
        logfolder = path;
    }

    public void SetRefugeCamera()
    {
        string filePath = logfolder + "/INITIAL_CONDITIONS.json";
        if(File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            JObject json = JObject.Parse(jsonText);
            JArray entities = (JArray)json["initialCondition"]["entities"];

            foreach (var entity in entities)
            {
                int urn = entity["urn"].ToObject<int>();
                if (urn == URN.Entity.REFUGE) // 建物の場合 (URN 4356)
                {
                    int x = 0, y = 0;
                    int floor = 1;
                    foreach (var prop in entity["properties"])
                    {
                        int propUrn = prop["urn"].ToObject<int>();
                        if (propUrn == 4614) x = prop["intValue"].ToObject<int>(); // X座標
                        if (propUrn == 4615) y = prop["intValue"].ToObject<int>(); // Y座標
                        if (propUrn == 4618) floor = prop["intValue"].ToObject<int>(); //何階建てか
                    }
                    SetCameraView(x, y, floor);
                    // return; //1個目の避難所で処理を終わる
                }
            }
        }
    }

    void SetCameraView(int x, int y, int floor)
    {
        Vector3 posi = new Vector3(x / 1000f, (floor * 3) * 7f, (y / 1000f) - ((floor * 3) * 7f));
        cam.transform.position = posi;

        // 避難所を斜め上から見るために、やや下向きにカメラを回転させる
        cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f); // X軸に45度傾ける
    }

}
