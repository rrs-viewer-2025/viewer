using UnityEngine;

public class ParentGoalCheck : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Refuge")
        {
            GameData.ParentGoal = true;
        }
    }
}
