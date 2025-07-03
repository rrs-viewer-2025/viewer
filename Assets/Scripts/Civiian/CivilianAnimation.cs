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

    // 歩くアニメーションのメソッド
    public void OnFootstep()
    {
        // Debug.Log("Footstep!");
        // AudioSource.PlayClipAtPoint(footstepClip, transform.position); なども可
    }
}
