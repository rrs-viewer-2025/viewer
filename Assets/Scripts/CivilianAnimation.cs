using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianAnimation : MonoBehaviour
{
    private Animator anim;
    private bool runFlag;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        runFlag = true;
        anim.SetBool("Run", runFlag);
    }
}
