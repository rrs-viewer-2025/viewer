using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextEffect : MonoBehaviour
{
    InputSceneChanger controller;

    // 表示対象の TextMeshPro コンポーネント
    [SerializeField] private TMP_Text _text; //本文表示用
    [SerializeField] private TMP_Text _question; //質問文表示用
    [SerializeField] private TMP_Text _choice; //選択肢表示用

    // 吹き出し
    [SerializeField] private Image _bubble;
    [SerializeField] private Image _question_bubble;

    // 選択肢の数によるUIの配置セット
    [SerializeField] private GameObject _2choices;
    [SerializeField] private GameObject _3choices;
    [SerializeField] private GameObject _4choices;

    // 各文字の表示間隔（秒）
    [SerializeField] private float _delayDuration = 0.1f;
    // フェードアウトにかける時間（秒）
    [SerializeField] private float _fadeDuration = 1f;
    // 音声再生用の AudioSource と AudioClip 配列
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _voiceClips;
    [SerializeField] private AudioClip[] _questionVoiceClips;
    [SerializeField] private AudioClip[] _feedbackVoiceClips;


    // 表示する複数のテキスト（順番に再生される）
    [TextArea(2, 5)]
    [SerializeField] private string[] _texts;

    [TextArea(2, 5)]
    [SerializeField] private string[] _questions;

    [TextArea(2, 5)]
    [SerializeField] private string[] _choices;

    [TextArea(2, 5)]
    [SerializeField] private string[] _feedback;

    // --- 質問処理用 ---
    private bool _isWaitingForAnswer = false; //回答待ちかどうかを管理
    private int _selectedAnswer = -1; //選ばれた回答を格納する

    //feedbackのクリップ管理用
    private int choicesum = -1;
    private int choices = 0;

    // 表示コルーチンを管理（重複防止）
    private Coroutine _showCoroutine;

    // シーン遷移用のフェードアウトパネル
    [SerializeField] private Image _fadePanel;
    // フェードアウト速度
    [SerializeField] private float _sceneTransitionSpeed = 0.1f;

    void Awake()
    {
        controller = FindFirstObjectByType<InputSceneChanger>();
    }

    // オブジェクトが有効化されたら自動で再生開始
    private void OnEnable()
    {
        ShowAll();
    }

    /// <summary>
    /// 全テキストを順番に表示（文字送り→フェードアウト）
    /// </summary>
    public void ShowAll()
    {
        if (_showCoroutine != null)
            StopCoroutine(_showCoroutine);

        _showCoroutine = StartCoroutine(ShowAllCoroutine());
    }

    /// <summary>
    /// テキスト配列を1つずつ表示していくメインの流れ
    /// </summary>
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
                        yield return StartCoroutine(ShowChoicesCoroutine(_choices[questionIndex], questionIndex));
                        yield return StartCoroutine(WaitForControllerAnswer(questionIndex));
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

        // シーン遷移の処理
        controller.ChangeScene();
    }

    /// <summary>
    /// 指定テキストを1文字ずつ表示していく
    /// </summary>
    private IEnumerator ShowTextCoroutine(string message, int index)
    {
        // 適切な吹き出しを表示する
        _question_bubble.gameObject.SetActive(false);
        _bubble.gameObject.SetActive(true);

        // 適切なテキストを表示
        _question.gameObject.SetActive(false);
        _choice.gameObject.SetActive(false);
        _text.gameObject.SetActive(true);

        _2choices.SetActive(false);
        _3choices.SetActive(false);
        _4choices.SetActive(false);

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

    // 質問文関係の関数
    private IEnumerator ShowQuestionCoroutine(string message, int index)
    {
        // 適切な吹き出しを表示する
        _question_bubble.gameObject.SetActive(true);
        _bubble.gameObject.SetActive(false);

        // 適切なテキストを表示
        _text.gameObject.SetActive(false); //本文用は非表示
        _choice.gameObject.SetActive(false); //選択肢用
        _question.gameObject.SetActive(true); //質問用を表示

        _2choices.SetActive(false);
        _3choices.SetActive(false);
        _4choices.SetActive(false);

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

    // 選択肢関係の関数
    private IEnumerator ShowChoicesCoroutine(string message, int questionIndex)
    {
        _choice.gameObject.SetActive(true); //選択肢用
        // 1. 選択肢テキストを行ごとに分割
        string[] choiceArray = _choices[questionIndex]
            .Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        // まず全部非表示
        _2choices.SetActive(false);
        _3choices.SetActive(false);
        _4choices.SetActive(false);

        // 選択肢数に応じて正しいUIセットを表示
        GameObject activeSet = null;
        if (choiceArray.Length == 2){
            activeSet = _2choices;
            choices = 2;
        }
        else if (choiceArray.Length == 3){
            activeSet = _3choices;
            choices = 3;
        }
        else if (choiceArray.Length == 4){
            activeSet = _4choices;
            choices = 4;
        }
        else{
            Debug.LogError($"未対応の選択肢数: {choiceArray.Length}");
            yield break;
        }
            
        activeSet.SetActive(true);

        _choice.text = message;

        // 0.5秒待ってから次の処理へ（今は見た目だけ出す）
        yield return new WaitForSeconds(0.5f);
    }

    // 回答の処理
    private IEnumerator WaitForControllerAnswer(int questionIndex)
    {
        _isWaitingForAnswer = true;
        _selectedAnswer = -1;

        Debug.Log("回答待ち状態に入りました");

        while (_isWaitingForAnswer)
        {
            // controller が存在するか確認
            if (controller != null && controller.ButtonPressed)
            {
                int pressed = controller.LastPressedButton; // 押されたボタン番号
                Debug.Log($"ボタン {pressed} が押されました");

                _selectedAnswer = pressed;
                _isWaitingForAnswer = false;
            }

            yield return null;
        }

        Debug.Log($"選ばれた選択肢番号: {_selectedAnswer}");

        // フィードバックを表示
        yield return StartCoroutine(ShowFeedbackCoroutine(questionIndex, _selectedAnswer));

        // 本文に戻る
        _isWaitingForAnswer = false;
    }

    // フィードバックの処理
    private IEnumerator ShowFeedbackCoroutine(int questionIndex, int answerIndex)
    {
        int index = choicesum + answerIndex + 1;
        string message = "";

        if (_feedback == null || index < 0 || index >= _feedback.Length)
        {
            Debug.LogWarning($"Feedback index out of range: {index}");
            yield break;
        }

        message = _feedback[index];

        _text.gameObject.SetActive(true);
        _question.gameObject.SetActive(false);
        _choice.gameObject.SetActive(false);

        _2choices.SetActive(false);
        _3choices.SetActive(false);
        _4choices.SetActive(false);

        _bubble.gameObject.SetActive(true);
        _question_bubble.gameObject.SetActive(false);

        _text.text = message;
        _text.maxVisibleCharacters = 0;

        Color resetColor = _text.color;
        resetColor.a = 1f;
        _text.color = resetColor;

        // 音声再生
        if (_audioSource != null && _feedbackVoiceClips != null && index >= 0 && index < _feedbackVoiceClips.Length && _feedbackVoiceClips[index] != null)
        {
            _audioSource.PlayOneShot(_feedbackVoiceClips[index]);
        }

        var delay = new WaitForSeconds(_delayDuration);
        for (int i = 0; i <= message.Length; i++)
        {
            _text.maxVisibleCharacters = i;
            yield return delay;
        }

        yield return new WaitForSeconds(1f);

        choicesum += choices;
    }




    /// <summary>
    /// テキストを徐々にフェードアウトさせる
    /// </summary>
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
}
