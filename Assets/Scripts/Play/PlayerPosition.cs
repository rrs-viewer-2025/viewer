using UnityEngine;

public class PlayerPosition : MonoBehaviour
{
    public void SetPosition()
    {
        Vector3 pos = Globaldata.roadGraph[Globaldata.path[0]].posi;
        transform.position = pos;
    }
}
