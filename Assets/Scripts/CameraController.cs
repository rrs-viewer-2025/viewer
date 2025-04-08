using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera[] cameras;  // カメラの配列
    private int currentCameraIndex = 0; // 現在のカメラのインデックス
    public float zoomSpeed = 1f;
    public float panSpeed = 5.0f;

    private Vector3[] originalPositions;  // 各カメラの初期位置
    private Quaternion[] originalRotations;  // 各カメラの初期回転
    private Vector3 lastMousePosition;
    private bool isPanning = false;

    void Start()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }

        originalPositions = new Vector3[cameras.Length];
        originalRotations = new Quaternion[cameras.Length];

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
            {
                originalPositions[i] = cameras[i].transform.position;
                originalRotations[i] = cameras[i].transform.rotation;
            }
        }
    }

    void Update()
    {
        // camera3では使用しない
        if (currentCameraIndex == 2) return;
        
        HandleZoom();
        HandlePan();
    }

    public void SwitchCamera(int index)
    {
        if (index < 0 || index >= cameras.Length) return;

        if (index == currentCameraIndex)
        {
            ResetCamera();
            return;
        }

        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = index;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    private void ResetCamera()
    {
        // カメラ３の時はリセットしない
        if (currentCameraIndex == 2) return;

        // カメラの位置と回転を初期値に戻すß
        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].transform.position = originalPositions[currentCameraIndex];
            cameras[currentCameraIndex].transform.rotation = originalRotations[currentCameraIndex];
        }
    }

    void HandleZoom()
    {
        Camera activeCamera = cameras[currentCameraIndex];
        if (activeCamera == null) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            activeCamera.transform.position += -activeCamera.transform.forward * scroll * zoomSpeed;
        }
    }

    void HandlePan()
    {
        Camera activeCamera = cameras[currentCameraIndex];
        if (activeCamera == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
            activeCamera.transform.Translate(move, Space.Self);
            lastMousePosition = Input.mousePosition;
        }
    }
}
