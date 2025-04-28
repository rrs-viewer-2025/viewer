using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera[] cameras;  // 切り替え対象のカメラの配列
    private int currentCameraIndex = 0; // 現在アクティブなカメラのインデックス
    public float zoomSpeed = 1f; // ズーム速度
    public float panSpeed = 5.0f; // パン（カメラ平行移動）速度

    private Vector3[] originalPositions;  // 各カメラの初期位置
    private Quaternion[] originalRotations;  // 各カメラの初期回転
    private Vector3 lastMousePosition;  // 直前のマウス位置
    private bool isPanning = false; // パン操作中かどうか

    void Start()
    {
        // 最初にアクティブにするカメラを決め、他のカメラは非アクティブにする
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == currentCameraIndex);
        }

        // 各カメラの初期位置と回転を保存する
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
        // カメラ3（index == 2）のときはズーム・パン操作を無効にする
        if (currentCameraIndex == 2) return;
        
        HandleZoom(); // マウスホイールによるズーム処理
        HandlePan();  // マウス右クリックによるパン処理
    }

    // カメラを切り替える処理
    public void SwitchCamera(int index)
    {
        // 不正なインデックスなら無視
        if (index < 0 || index >= cameras.Length) return;

        // 同じカメラを選んだ場合はリセットだけ行う
        if (index == currentCameraIndex)
        {
            ResetCamera();
            return;
        }

        // 現在のカメラを非アクティブにし、新しいカメラをアクティブにする
        cameras[currentCameraIndex].gameObject.SetActive(false);
        currentCameraIndex = index;
        cameras[currentCameraIndex].gameObject.SetActive(true);
    }

    // カメラの位置と回転を初期状態に戻す処理
    private void ResetCamera()
    {
        // カメラ3（index == 2）のときはリセットしない
        if (currentCameraIndex == 2) return;

        if (cameras[currentCameraIndex] != null)
        {
            cameras[currentCameraIndex].transform.position = originalPositions[currentCameraIndex];
            cameras[currentCameraIndex].transform.rotation = originalRotations[currentCameraIndex];
        }
    }

    // ズーム処理（マウスホイールで前後移動）
    void HandleZoom()
    {
        Camera activeCamera = cameras[currentCameraIndex];
        if (activeCamera == null) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            // カメラの前方向（forward）に沿って前後に移動
            activeCamera.transform.position += -activeCamera.transform.forward * scroll * zoomSpeed;
        }
    }

    // パン処理（右クリックドラッグで平行移動）
    void HandlePan()
    {
        Camera activeCamera = cameras[currentCameraIndex];
        if (activeCamera == null) return;

        // 右クリック押したらパン開始
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }

        // 右クリック離したらパン終了
        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        // パン操作中
        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            // マウスの移動量に応じてカメラを平行移動
            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
            activeCamera.transform.Translate(move, Space.Self);
            lastMousePosition = Input.mousePosition;
        }
    }
}
