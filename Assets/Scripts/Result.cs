using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Result : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI hyoukaText;
    public TextMeshProUGUI commentText;

    void Start()
    {
        Debug.Log("リザルトシーンに受け取ったクリア時間: " + GameData.clearTime.ToString("F2") + "秒");
        result();
    }

    void result() //リザルトを表示する関数
    {
        if(!GameData.hinan) //避難失敗時の処理
        {
            timeText.text = "--:--";
            hyoukaText.text = "F";
            commentText.text = "まにあわなかったね。\nげんじつはもっと\nあぶないよ";
            return;
        }

        int minutes = Mathf.FloorToInt(GameData.clearTime / 36f);
        int seconds = Mathf.FloorToInt(GameData.clearTime % 36f);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);//クリアタイムの表示

        commentText.text = "よくできたね！\nでもあんしんしないで。\nげんじつはあまくない。";

        if( minutes < 1 ) hyoukaText.text = "S"; //1分未満
        else if( minutes < 2) hyoukaText.text = "A"; //1分以上2分未満
        else if( minutes < 3) hyoukaText.text = "B"; //2分以上3分未満
        else if( minutes < 4) hyoukaText.text = "C"; //3分以上4分未満
        else if( minutes < 5) hyoukaText.text = "D"; //4分以上5分未満
    }
}
