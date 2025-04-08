using UnityEngine;
using TMPro;

public class CitizenCounterTMP : MonoBehaviour
{
    public TextMeshProUGUI citizenCountText; // TMPのTextコンポーネント
    public string citizenTag = "citizen"; // 市民オブジェクトのタグ

    void Update()
    {
        // 指定したタグのオブジェクト数を取得
        int citizenCount = GameObject.FindGameObjectsWithTag(citizenTag).Length;
        
        // TMPテキストを更新
        citizenCountText.text = $"しみんのかず: {citizenCount}";
    }
}