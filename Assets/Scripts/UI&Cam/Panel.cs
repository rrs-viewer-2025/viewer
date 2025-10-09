using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    private GameObject currentSelectedCursor = null;
    private GameObject lastPressedCursor = null;
    private string targetSceneName = "main";

    // Audio
    private AudioSource audioSource;

    // ボタンごとの音声を設定（Inspector で割り当てる）
    public AudioClip yButtonClip;
    public AudioClip bButtonClip;
    public AudioClip aButtonClip;
    public AudioClip xButtonClip;

    // セリフ表示関連
    // 表示するTextMeshProUGUI
    private TMP_Text displayText;
    // 各ボタンで表示する内容
    private string yButtonText = "細い道を避けて避難所に到達するよ！";
    private string bButtonText = "倒れやすい建物を避けて避難所に到達するよ！";
    private string aButtonText = "最短経路で避難所に到達するよ！";

    // Manager.cs
    Manager mg;

    int i = -1;

    void Awake()
    {
        mg = FindObjectOfType<Manager>();
        
        // シーン内の"text"オブジェクトを取得し、TextMeshProUGUIを参照
        GameObject textObj = GameObject.Find("text");
        if (textObj != null)
        {
            displayText = textObj.GetComponent<TMP_Text>();
            if (displayText == null)
            {
                Debug.LogError("'text'オブジェクトにTMP_Textコンポーネントがありません。");
            }
        }
        else
        {
            Debug.LogError("シーン内に'text'オブジェクトが見つかりません。");
        }
    }

    void Start()
    {
        panelImage = GetComponent<Image>();

        // シーン内のカーソルオブジェクトを名前で取得
        cursorY = GameObject.Find("cursorY");
        cursorB = GameObject.Find("cursorB");
        cursorA = GameObject.Find("cursorA");
        cursorX = GameObject.Find("cursorX");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource がアタッチされていません！");
        }
    }

    void Update()
    {
        // Yボタン（Joystick1Button3）でcursorYを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button3))
        {
            HandleCursorSelection(cursorY, targetSceneName);

            // Yボタンの音を再生
            PlayButtonSound(yButtonClip);

            // セリフの格納
            SetText(aButtonText);

            // パスの選択
            i = 1;
            Globaldata.path = mg.getPath(i);
        }

        // Bボタン（Joystick1Button1）でcursorBを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button2))
        {
            HandleCursorSelection(cursorB, targetSceneName);

            // Bボタンの音を再生
            PlayButtonSound(bButtonClip);

            // セリフの格納
            SetText(yButtonText);

            // パスの選択
            i = 2;
            Globaldata.path = mg.getPath(i);
        }

        // Aボタン（Joystick1Button0）でcursorAを選択
        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            HandleCursorSelection(cursorA, targetSceneName);

            // Aボタンの音を再生
            PlayButtonSound(aButtonClip);

            // セリフの格納
            SetText(bButtonText);

            // パスの選択
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

    // ボタンごとに別の音を鳴らす（再生中の音は停止）
    private void PlayButtonSound(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource がアタッチされていません！");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("AudioClip が設定されていません！");
            return;
        }

        // 既に再生中の音声を停止
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 新しい音声を再生
        audioSource.clip = clip;
        audioSource.Play();
    }
    
    void SetText(string text)
    {
        if (displayText != null)
        {
            displayText.text = text;
        }
    }
}
