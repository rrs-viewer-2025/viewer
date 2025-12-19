using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class CanvasVideoSwitcher : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject titleUI;      // タイトルテキストやロゴ
    [SerializeField] private RawImage videoPanel;     // 動画を表示するRawImage
    [SerializeField] private VideoPlayer videoPlayer; // VideoPlayer

    [Header("Timing")]
    [SerializeField] private float titleDisplayTime = 3f; // タイトル表示時間
    [SerializeField] private float videoDisplayTime = 5f; // 動画再生時間

    private void Start()
    {
        // Canvas上でループ切り替え
        StartCoroutine(SwitchLoop());
    }

    private IEnumerator SwitchLoop()
    {
        while (true)
        {
            // --- タイトル表示 ---
            titleUI.SetActive(true);
            videoPanel.gameObject.SetActive(false);
            videoPlayer.Stop();
            yield return new WaitForSeconds(titleDisplayTime);

            // --- 動画再生 ---
            titleUI.SetActive(false);
            videoPanel.gameObject.SetActive(true);
            videoPlayer.Play();
            yield return new WaitForSeconds(videoDisplayTime);

            // --- 繰り返し ---
        }
    }
}
