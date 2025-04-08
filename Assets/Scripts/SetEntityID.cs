// エンティティにIDを振り分けるためのスクリプトだよ
// dictionalyでオブジェクトとID結びつけて管理してるならいらないのでは...

using UnityEngine;

public class SetEntityID : MonoBehaviour
{
    public int entityID;  // 市民のエンティティID

    public void SetEntityIDtoEntity(int id)
    {
        entityID = id;
    }
}
