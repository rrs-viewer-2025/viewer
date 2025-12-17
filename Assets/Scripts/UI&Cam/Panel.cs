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

    [SerializeField] private GameObject choices;

    private GameObject currentSelectedCursor = null;
    private GameObject lastPressedCursor = null;
    private string targetSceneName = "main";

    // Audio
    [SerializeField] private AudioSource _audioSource;

    // ボタンごとの音声を設定（Inspector で割り当てる）
    public AudioClip yButtonClip;
    public AudioClip bButtonClip;
    public AudioClip aButtonClip;
    public AudioClip startClip;

    [SerializeField] private RawImage _map1;
    [SerializeField] private RawImage _map2;
    [SerializeField] private RawImage _map3;

    [SerializeField] private AudioClip[] _voiceClips;

    [SerializeField] private AudioClip[] _questionVoiceClips;

    public TMP_Text _text; //ずんだもんが喋る文字の表示
    [SerializeField] private TMP_Text _question; //質問文表示用

    [TextArea(2, 5)]
    [SerializeField] private string[] _texts;

    [TextArea(2, 5)]
    [SerializeField] private string[] _questions;

    [SerializeField] private TMP_Text _tokutyou;

    // 各文字の表示間隔（秒）
    [SerializeField] private float _delayDuration = 0.1f;
    // フェードアウトにかける時間（秒）
    [SerializeField] private float _fadeDuration = 1f;

    // セリフ表示関連
    // 表示するTextMeshProUGUI
    private TMP_Text displayText;
    // 各ボタンで表示する内容
    private string bButtonText = "細い道を避けて\n避難所に到達するよ！";
    private string aButtonText = "倒れやすい建物を避けて\n避難所に到達するよ！";
    private string yButtonText = "最短経路で\n避難所に到達するよ！";

    // Manager.cs
    Manager mg;

    int i = -1;

    [System.Obsolete]

    // 表示コルーチンを管理（重複防止）
    private Coroutine _showCoroutine;

    private bool _isWaitingForAnswer = false; //回答待ちかどうかを管理
    private bool flag = false;

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
        choices.SetActive(false);

        panelImage = GetComponent<Image>();

        // シーン内のカーソルオブジェクトを名前で取得
        cursorY = GameObject.Find("cursorY");
        cursorB = GameObject.Find("cursorB");
        cursorA = GameObject.Find("cursorA");
        cursorX = GameObject.Find("cursorX");

        
        if (_audioSource == null)
        {
            Debug.LogError("_audioSource がアタッチされていません！");
        }


        if (startClip != null && _audioSource != null)
        {
            // PlayButtonSound(startClip);
        }

        ShowAll();
    }

    public void ShowAll()
    {
        if (_showCoroutine != null)
            StopCoroutine(_showCoroutine);

        _showCoroutine = StartCoroutine(ShowAllCoroutine());
    }

    private IEnumerator ShowAllCoroutine()
    {
        for (int i = 0; i < _texts.Length; i++)
        {
            string line = _texts[i].Trim();

            // --- ★追加：<q>タグがあるか判定 ---
            if (line.StartsWith("<q>"))
            {
                _isWaitingForAnswer = true;

                // <q>のあとに数字がある想定
                int questionIndex;
                if (int.TryParse(line.Substring(3), out questionIndex))
                {
                    // インデックスが範囲内なら質問文を表示
                    if (questionIndex >= 0 && questionIndex < _questions.Length)
                    {
                        yield return StartCoroutine(ShowQuestionCoroutine(_questions[questionIndex], questionIndex));
                        yield return StartCoroutine(ShowChoicesCoroutine());
                        flag = true;
                    }
                    else
                    {
                        Debug.LogWarning($"質問番号 {questionIndex} は範囲外です");
                    }
                }
                else
                {
                    Debug.LogWarning($"<q>タグの番号を読み取れません: {line}");
                }
            }
            else if(!_isWaitingForAnswer)
            {
                // 通常テキスト表示
                yield return StartCoroutine(ShowTextCoroutine(line, i));
            }

            // フェードアウト（最後の行以外）
            if (i < _texts.Length - 1 && !_isWaitingForAnswer)
                yield return StartCoroutine(FadeOutCoroutine());
        }

        _showCoroutine = null;
    }

    private IEnumerator ShowTextCoroutine(string message, int index)
    {
        // 適切なテキストを表示
        _question.gameObject.SetActive(false);
        choices.gameObject.SetActive(false);
        _text.gameObject.SetActive(true);
        _tokutyou.gameObject.SetActive(false);

        _text.text = message;
        _text.maxVisibleCharacters = 0;

        // アルファを1に戻す（フェード後の再利用対策）
        Color resetColor = _text.color;
        resetColor.a = 1f;
        _text.color = resetColor;

        // 音声再生
        if (_audioSource != null && _voiceClips != null && index < _voiceClips.Length && _voiceClips[index] != null)
        {
            _audioSource.PlayOneShot(_voiceClips[index]);
        }

        var delay = new WaitForSeconds(_delayDuration);
        var length = message.Length;

        for (int i = 0; i <= length; i++)
        {
            _text.maxVisibleCharacters = i;
            yield return delay;
        }

        // 全部表示後に少し待つ（見せ時間の確保）
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ShowQuestionCoroutine(string message, int index)
    {
        // 適切なテキストを表示
        _text.gameObject.SetActive(false); //本文用は非表示
        _question.gameObject.SetActive(true); //質問用を表示
        _tokutyou.gameObject.SetActive(false);

        _question.text = message;
        _question.maxVisibleCharacters = 0;

        // アルファを1に戻す（フェード後の再利用対策）
        Color resetColor = _question.color;
        resetColor.a = 1f;
        _question.color = resetColor;

        // 音声再生
        if (_audioSource != null && _questionVoiceClips != null && index < _questionVoiceClips.Length && _questionVoiceClips[index] != null)
        {
            _audioSource.PlayOneShot(_questionVoiceClips[index]);
        }

        var delay = new WaitForSeconds(_delayDuration);
        var length = message.Length;

        for (int i = 0; i <= length; i++)
        {
            _question.maxVisibleCharacters = i;
            yield return delay;
        }

        // 全部表示後に少し待つ（見せ時間の確保）
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ShowChoicesCoroutine()
    {
        choices.SetActive(true);

        // 0.5秒待ってから次の処理へ（今は見た目だけ出す）
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator FadeOutCoroutine()
    {
        Color originalColor = _text.color;
        float elapsed = 0f;

        while (elapsed < _fadeDuration)
        {
            // 時間に応じてアルファ値を0へ近づける
            float alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            _text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最終的に完全に透明にしておく
        _text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    private IEnumerator ShowButtonMessageCoroutine(string message)
    {
        if (_text == null) yield break;

        _question.gameObject.SetActive(false);
        _text.gameObject.SetActive(false);
        _tokutyou.gameObject.SetActive(true);

        _tokutyou.text = message;
        _tokutyou.maxVisibleCharacters = 0;

        Color resetColor = _tokutyou.color;
        resetColor.a = 1f;
        _tokutyou.color = resetColor;

        var delay = new WaitForSeconds(_delayDuration);
        var length = message.Length;

        for (int i = 0; i <= length; i++)
        {
            _tokutyou.maxVisibleCharacters = i;
            yield return delay;
        }

        yield return new WaitForSeconds(0.5f);
    }

    void Update()
    {
        if(flag)
        {
            // Yボタン（Joystick1Button3）でcursorYを選択
            if (Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Alpha2))
            {
                StopAllCoroutines();
                HandleCursorSelection(cursorY, targetSceneName);

                _map1.gameObject.SetActive(true);
                _map2.gameObject.SetActive(false);
                _map3.gameObject.SetActive(false);

                // Yボタンの音を再生
                PlayButtonSound(yButtonClip);

                // セリフ再生（←ここ修正）
                StartCoroutine(ShowButtonMessageCoroutine(yButtonText));

                // パスの選択
                i = 1;
                Globaldata.path = mg.getPath(i);
            }

            // Bボタン（Joystick1Button1）でcursorBを選択
            if (Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Alpha3))
            {
                StopAllCoroutines();
                HandleCursorSelection(cursorB, targetSceneName);

                _map1.gameObject.SetActive(false);
                _map2.gameObject.SetActive(true);
                _map3.gameObject.SetActive(false);

                // Bボタンの音を再生
                PlayButtonSound(bButtonClip);

                // セリフ再生（←ここ修正）
                StartCoroutine(ShowButtonMessageCoroutine(bButtonText));

                // パスの選択
                i = 2;
                Globaldata.path = mg.getPath(i);
            }

            // Aボタン（Joystick1Button0）でcursorAを選択
            if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Alpha1))
            {
                StopAllCoroutines();
                HandleCursorSelection(cursorA, targetSceneName);

                _map1.gameObject.SetActive(false);
                _map2.gameObject.SetActive(false);
                _map3.gameObject.SetActive(true);

                // Aボタンの音を再生
                PlayButtonSound(aButtonClip);

                // セリフ再生（←ここ修正）
                StartCoroutine(ShowButtonMessageCoroutine(aButtonText));

                // パスの選択
                i = 3;
                Globaldata.path = mg.getPath(i);
            }
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

        // 現在選択されているカーソルを更新
        currentSelectedCursor = targetCursor;
        lastPressedCursor = targetCursor;
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
        if (_audioSource == null)
        {
            Debug.LogError("_audioSource がアタッチされていません！");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("AudioClip が設定されていません！");
            return;
        }

        // 既に再生中の音声を停止
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        // 新しい音声を再生
        _audioSource.clip = clip;
        _audioSource.Play();
    }
    
    void SetText(string text)
    {
        if (displayText != null)
        {
            displayText.text = text;
        }
    }
}
