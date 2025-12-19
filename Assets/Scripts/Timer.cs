using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    float LimitTime = 240f;
    public float timeRemaining;
    public TextMeshProUGUI timerText;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public GameObject Info_timeover;
    private bool timeover = false;
    private bool isStopped = false;
    private bool damageWarning = false;
    PlayerTrail pt;

    void Awake()
    {
        pt = FindFirstObjectByType<PlayerTrail>();
    }

    void Start()
    {
        timeRemaining = LimitTime;
    }

    void Update()
    {
        if (isStopped) return;  // ゴールしたらタイマー止める
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            timeRemaining = Mathf.Max(0f, timeRemaining);

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            // ダメージ中でなければ残り時間で色制御
            if (!damageWarning)
            {
                timerText.color = (timeRemaining < 10f) ? warningColor : normalColor;
            }
        }
        else
        {
            if (!timeover)
            {
                timeover = true;

                Globaldata.playerposi = pt.getPlayerPosiList();
                timerText.text = "00:00";
                GameData.hinan = false; //避難失敗を格納

                Info_timeover.SetActive(true);
                Invoke(nameof(LoadResultScene), 3f);
            }
        }
    }

    void LoadResultScene()
    {
        SceneManager.LoadScene("result");
    }

    public void StopTimer()
    {
        isStopped = true;
    }

    public float GetTime()
    {
        return LimitTime - timeRemaining;
    }

    // Player から呼ばれる
    public void SetWarning(bool isDamage)
    {
        damageWarning = isDamage;

        timerText.color = isDamage ? warningColor : normalColor;
    }
}
