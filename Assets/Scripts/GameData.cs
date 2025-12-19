using UnityEngine;

public static class GameData  //クリア時間と避難成功したかの情報を格納するスクリプトだよ
{
    public static float clearTime;
    public static bool hinan = false;

    public static bool ChildGoal = false;
    public static bool ParentGoal = false;
}
