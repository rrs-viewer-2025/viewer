using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianState : MonoBehaviour
{
    public int STAMINA;
    public int HP; 
    public int DAMAGE;
    public int BURIEDNESS;
    public int TRAVEL_DISTANCE;

    public void SetState(int stamina, int hp, int damage, int buriedness, int traveldistance)
    {
        STAMINA = stamina;
        HP = hp;
        DAMAGE = damage;
        BURIEDNESS = buriedness;
        TRAVEL_DISTANCE = traveldistance;
    }
}
