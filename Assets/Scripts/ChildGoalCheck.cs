using UnityEngine;

public class ChildGoalCheck : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Refuge")
        {
            GameData.ChildGoal = true;
        }
    }
}
