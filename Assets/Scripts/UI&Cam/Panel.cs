using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ★追加
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    // シーン内のカーソルオブジェクトを取得
    private GameObject cursorY;
    private GameObject cursorB;
    private GameObject cursorA;
    private GameObject cursorX;
    
    // パネルのImageコンポーネント
    private Image panelImage;
    
    // 現在選択されているカーソル
    private GameObject currentSelectedCursor = null;
    
    // 最後に押されたボタンを記録（2回押し判定用）
    private GameObject lastPressedCursor = null;
    
    // 遷移先シーン名
    private string targetSceneName = "main";

    // Manager.cs
    Manager mg;

    int i = -1;

    void Awake()
    {
        mg = FindObjectOfType<Manager>();
    }

    void Start()
    {
        panelImage = GetComponent<Image>();
        
        // シーン内のカーソルオブジェクトを名前で取得
        cursorY = GameObject.Find("cursorY");
        cursorB = GameObject.Find("cursorB");
        cursorA = GameObject.Find("cursorA");
        cursorX = GameObject.Find("cursorX");
    }

    void Update()
    {
        // Yボタン（Joystick1Button3）でcursorYを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            HandleCursorSelection(cursorY, targetSceneName);
            i = 1;
            Globaldata.path = mg.getPath(i);
        }
        
        // Bボタン（Joystick1Button1）でcursorBを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            HandleCursorSelection(cursorB, targetSceneName);
        }
        
        // Aボタン（Joystick1Button0）でcursorAを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            HandleCursorSelection(cursorA, targetSceneName);
            i = 2;
            Globaldata.path = mg.getPath(i);
        }
        
        // Xボタン（Joystick1Button2）でcursorXを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            HandleCursorSelection(cursorX, targetSceneName);
            i = 3;
            Globaldata.path = mg.getPath(i);
        }
    }
    
    // カーソル選択処理
    private void HandleCursorSelection(GameObject targetCursor, string sceneName)
    {
        // 2回同じものが押された場合は画面遷移
        if (lastPressedCursor == targetCursor)
        {
            TransitionToScene(sceneName);
            return;
        }
        
        // 他のカーソルの不透明度を0にリセット
        ResetAllCursorsOpacity();
        
        // 選択されたカーソルの不透明度を1に設定
        SetPanelOpacity(targetCursor, 1.0f);
        
        // 現在選択されているカーソルを更新
        currentSelectedCursor = targetCursor;
        lastPressedCursor = targetCursor;
    }
    
    // 全てのカーソルの不透明度を0にリセット
    private void ResetAllCursorsOpacity()
    {
        SetPanelOpacity(cursorY, 0.0f);
        SetPanelOpacity(cursorB, 0.0f);
        SetPanelOpacity(cursorA, 0.0f);
        SetPanelOpacity(cursorX, 0.0f);
    }
    
    // 指定したオブジェクトのImageコンポーネントの不透明度を設定するメソッド
    private void SetPanelOpacity(GameObject targetObject, float alpha)
    {
        if (targetObject != null)
        {
            Image targetImage = targetObject.GetComponent<Image>();
            if (targetImage != null)
            {
                Color currentColor = targetImage.color;
                targetImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            }
        }
    }
    
    // シーン遷移処理
    private void TransitionToScene(string sceneName)
    {
        Debug.Log($"シーン遷移: {sceneName}");
        // シーンが存在する場合のみ遷移
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"シーン '{sceneName}' の読み込みに失敗しました: {e.Message}");
        }
    }
}
