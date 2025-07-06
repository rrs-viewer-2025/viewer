using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float LimitTime = 40f;
    public float timeRemaining;
    public TextMeshProUGUI timerText;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public GameObject Info_timeover;
    private bool timeover = false;
    private bool isStopped = false;

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

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // 10秒切ったら赤色に
            if (timeRemaining < 10f)
            {
                timerText.color = warningColor;
            }
            else
            {
                timerText.color = normalColor;
            }
        }
        else
        {
            timerText.text = "00:00";
            Info_timeover.SetActive(true);
            Invoke("LoadResultScene", 3f);
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

}
