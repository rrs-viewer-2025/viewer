using UnityEngine;

public class HideDispInfo : MonoBehaviour
{
    // 非表示にするまでの時間（秒）
    public float displayTime = 5f;

    void Start()
    {
        // アタッチされたオブジェクトの状態によって表示/非表示を切り替える
        if (gameObject.activeSelf)
        {
            Invoke(nameof(HideObject), displayTime);
        }
        else
        {
            Invoke(nameof(DispObject), displayTime);
        }
    }

    void HideObject()
    {
        gameObject.SetActive(false);
    }

    void DispObject()
    {
        gameObject.SetActive(true);
    }
}
