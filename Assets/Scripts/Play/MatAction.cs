using UnityEngine;
using UnityEngine.InputSystem;

public class MatAction : MonoBehaviour
{
    // インスペクターでの設定を可能にするために、InputActionをSerializeField属性で公開
    [SerializeField] private InputAction upAction;
    [SerializeField] private InputAction downAction;
    [SerializeField] private InputAction leftAction;
    [SerializeField] private InputAction rightAction;
    [SerializeField] private InputAction stopAction;

    // 各アクションの値を格納するプロパティ
    public int Up { get; private set; }
    public int Down { get; private set; }
    public int Left { get; private set; }
    public int Right { get; private set; }
    public int Stop { get; private set; }

    private void OnEnable()
    {
        // 各アクションを有効化
        upAction?.Enable();
        downAction?.Enable();
        leftAction?.Enable();
        rightAction?.Enable();
        stopAction?.Enable();
    }

    private void OnDisable()
    {
        // 各アクションを無効化
        upAction?.Disable();
        downAction?.Disable();
        leftAction?.Disable();
        rightAction?.Disable();
        stopAction?.Disable();
    }

    private void Update()
    {
        // 各アクションの値を取得
        Up = (int)upAction.ReadValue<float>();
        Down = (int)downAction.ReadValue<float>();
        Left = (int)leftAction.ReadValue<float>();
        Right = (int)rightAction.ReadValue<float>();
        Stop = (int)stopAction.ReadValue<float>();
        
        // 各アクションの値に応じてログを出力
        // if (up > 0) Debug.Log("上移動");
        // if (down > 0) Debug.Log("下移動");
        // if (left > 0) Debug.Log("左移動");
        // if (right > 0) Debug.Log("右移動");
        // if (staticValue < 0) Debug.Log("静止");
    }
}