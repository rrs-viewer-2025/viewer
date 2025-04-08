using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    [SerializeField] private Transform target; // 追跡対象
    [SerializeField] private Sprite markerSprite; // マーカーのスプライト
    [SerializeField] private RectTransform minimapPanel; // Minimapのパネル
    [SerializeField] private float mapScale = 0.1f; // 縮尺
    [SerializeField] private Vector2 markerSize = new Vector2(20f, 20f); // マーカーのサイズ
    [SerializeField] private Transform mapCenter; // Minimapの中心位置

    private RectTransform minimapMarkerInstance;

    void Start()
    {
        if (markerSprite != null && minimapPanel != null)
        {
            GameObject markerObject = new GameObject("MinimapMarker");
            markerObject.transform.SetParent(minimapPanel);
            markerObject.transform.localScale = Vector3.one;

            minimapMarkerInstance = markerObject.AddComponent<RectTransform>();
            minimapMarkerInstance.sizeDelta = markerSize;

            Image markerImage = markerObject.AddComponent<Image>();
            markerImage.sprite = markerSprite;
        }
    }

    void Update()
    {
        if (target == null || minimapMarkerInstance == null || minimapPanel == null || mapCenter == null) return;

        // ワールド座標をMinimap座標に変換
        Vector3 relativePosition = target.position - mapCenter.position;
        Vector3 offset = new Vector3(relativePosition.x, relativePosition.z, 0f) * mapScale;
        float halfWidth = minimapPanel.rect.width / 2;
        float halfHeight = minimapPanel.rect.height / 2;

        // マーカーの位置を設定
        minimapMarkerInstance.anchoredPosition = new Vector2(offset.x, offset.y);

        // 範囲外の制限
        minimapMarkerInstance.anchoredPosition = new Vector2(
            Mathf.Clamp(minimapMarkerInstance.anchoredPosition.x, -halfWidth, halfWidth),
            Mathf.Clamp(minimapMarkerInstance.anchoredPosition.y, -halfHeight, halfHeight)
        );
    }
} 
