using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class PlayerTrail : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> positions = new List<Vector3>();

    [SerializeField] float minDistance = 0.1f; // 次の点を追加する最小移動距離

    void Start()
    {
    lineRenderer = GetComponent<LineRenderer>();
    // LineRendererを無効化（描画しない）
    lineRenderer.enabled = false;
    Vector3 startPos = transform.position + Vector3.up * 0.2f;
    positions.Add(startPos);
    }

    void Update()
    {
        Vector3 currentPos = transform.position + Vector3.up * 0.2f;
        // 前回の点と一定距離離れたら追加
        if (Vector3.Distance(positions[positions.Count - 1], currentPos) > minDistance)
        {
            positions.Add(currentPos);
        }

        // スペースキーが押されたら軌道を表示
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = positions.Count;
            for (int i = 0; i < positions.Count; i++)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
        }
    }

    public List<Vector3> getPlayerPosiList()
    {
        return positions;
    }

    public void Resetposilist()
    {
        positions.Clear();
        // 現在位置を新しい始点として登録
        Vector3 startPos = transform.position + Vector3.up * 0.2f;
        positions.Add(startPos);
    }
}
