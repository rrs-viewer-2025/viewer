using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    public GameObject plane;
    public void setPlane(Vector3 center, float width, float height) //OverviewCameraスクリプトから値もらうよ
    {
        //※Planeはデフォルトで10x10
        float scaleX = width / 10f;   // Xのスケールを計算
        float scaleZ = height / 10f;  // Zのスケールを計算

        plane.transform.localScale = new Vector3(scaleX + 10, 1, scaleZ + 10); // Planeのスケール調整(余白を持たせる)
        plane.transform.position = center; //Planeを中心に配置
    }
}
