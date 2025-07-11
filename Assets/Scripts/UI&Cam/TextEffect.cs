using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextEffect : MonoBehaviour
{
    // 表示対象の TextMeshPro コンポーネント
    [SerializeField] private TMP_Text _text;
    // 各文字の表示間隔（秒）
    [SerializeField] private float _delayDuration = 0.1f;
    // フェードアウトにかける時間（秒）
    [SerializeField] private float _fadeDuration = 1f;
    // 音声再生用の AudioSource と AudioClip 配列
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _voiceClips;


    // 表示する複数のテキスト（順番に再生される）
    [TextArea(2, 5)]
    [SerializeField] private string[] _texts;

    // 表示コルーチンを管理（重複防止）
    private Coroutine _showCoroutine;

    // シーン遷移用のフェードアウトパネル
    [SerializeField] private Image _fadePanel;
    // フェードアウト速度
    [SerializeField] private float _sceneTransitionSpeed = 0.1f;

    // ScenesManager
    public string targetSceneName = "main"; // インスペクタで設定可

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
            yield return StartCoroutine(ShowTextCoroutine(_texts[i], i));

            // 最後のテキスト以外はフェードアウト
            if (i < _texts.Length - 1)
            {
                yield return StartCoroutine(FadeOutCoroutine());
            }
        }

        // エンターが押されたら次のシーンへ遷移
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        // シーン遷移
        SceneManager.LoadScene(targetSceneName);

        _showCoroutine = null;
    }

    /// <summary>
    /// 指定テキストを1文字ずつ表示していく
    /// </summary>
    private IEnumerator ShowTextCoroutine(string message, int index)
    {
        _text.gameObject.SetActive(true);
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
