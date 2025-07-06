using UnityEngine;

public class Result : MonoBehaviour
{
    void Start()
    {
        Debug.Log("リザルトシーンに受け取ったクリア時間: " + TimeData.clearTime.ToString("F2") + "秒");
    }
}
