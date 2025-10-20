using UnityEngine;

public class main_minimap : MonoBehaviour
{
    public Material lineMaterial; // インスペクタから設定
    
    public void DrawPath()
    {
        GameObject go = new GameObject("PathLine_path1");
        var lr = go.AddComponent<LineRenderer>();

        lr.positionCount = Globaldata.path.Count;
        lr.material = lineMaterial;
        lr.widthMultiplier = 5.0f;
        lr.useWorldSpace = true;

        for (int i = 0; i < Globaldata.path.Count; i++)
        {
            lr.SetPosition(i, Globaldata.roadGraph[Globaldata.path[i]].posi);
        }

        // レイヤー設定
        int layer = LayerMask.NameToLayer("path1");
        go.layer = layer;
    }
}
