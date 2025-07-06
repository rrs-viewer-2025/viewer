using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// フェードイン・アウトとシーン遷移を管理する共通コントローラ（シングルトン）
/// </summary>
public class FadeSceneController : MonoBehaviour
{
    public static FadeSceneController Instance { get; private set; }

    [Header("フェード設定")]
    public Image fadeImage;            // フェード用Image
    public float fadeSpeed = 1.0f;     // フェード速度

    private bool isFading = false;

    void Awake()
    {
        // シングルトン化 & 永続化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // シーン読み込みイベントに登録
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 最初のシーンでフェードイン開始
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            SetAlpha(1f);
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// 他のシーンから戻ってきたときに自動でフェードイン
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ★ fadeImage が null の場合、シーン内の "FadePanel" を探して再設定
        if (fadeImage == null)
        {
            GameObject panelObj = GameObject.FindWithTag("FadePanel"); // ★ タグ検索
            if (panelObj != null)
            {
                fadeImage = panelObj.GetComponent<Image>(); // ★ Image を取得
            }
        }

        // ★ フェードイン開始
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            SetAlpha(1f);
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.LogWarning("次のシーンで fadeImage が見つかりませんでした"); // ★ エラーログ
        }
    }


    public void StartFadeOutToScene(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeOutAndLoad(sceneName));
        }
    }

    public IEnumerator FadeIn()
    {
        isFading = true;
        float alpha = 1f;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
        isFading = false;
    }

    public IEnumerator FadeOut()
    {
        isFading = true;
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f);
        isFading = false;
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }
    }

    void OnDestroy()
    {
        // メモリリーク防止のためイベント解除
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
