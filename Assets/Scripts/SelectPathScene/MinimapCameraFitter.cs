// カメラの描画範囲をマップに合わせるスクリプトです
using UnityEngine;

public class MinimapCameraFitter : MonoBehaviour
{
    public GameObject mapPlane; // マップの地面プレーン

    public void FitCamera()
    {
        // 子オブジェクトにあるカメラ全部を処理
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            FitToPlane(cam);
        }
    }

    void FitToPlane(Camera miniMapCamera)
    {
        // Planeの境界情報取得
        var bounds = mapPlane.GetComponent<Renderer>().bounds;

        float mapWidth = bounds.size.x;
        float mapHeight = bounds.size.z;

        // カメラのアスペクト比に合わせて orthographicSize を調整
        float screenRatio = miniMapCamera.aspect; // カメラ自身の比率
        float targetRatio = mapWidth / mapHeight;

        if (screenRatio >= targetRatio)
        {
            // 横に余裕がある → 高さ基準
            miniMapCamera.orthographicSize = mapHeight / 2f;
        }
        else
        {
            // 縦に余裕がある → 幅基準
            // float differenceInSize = targetRatio / screenRatio;
            // miniMapCamera.orthographicSize = mapHeight / 2f * differenceInSize;
            // 縦が狭すぎる → 幅基準
            miniMapCamera.orthographicSize = mapWidth / (2f * screenRatio);
        }

    }
}
