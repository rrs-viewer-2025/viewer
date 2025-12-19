using UnityEngine;

public class CameraRotationLock : MonoBehaviour
{
    private Quaternion fixedRotation;

    void Start()
    {
        // 現在の角度を保持（例: 真上なら x=90, y=0, z=0）
        fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 毎フレーム回転を固定（親の回転に影響されないように）
        transform.rotation = fixedRotation;
    }
}
