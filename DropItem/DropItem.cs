using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour, IDropItemEvent
{
    public Animator animator;

    GameManager gameManager;

    public void SetInit()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        SetAniState();
    }

    public void SetAniState()
    {
        if(GameManager.IsGetAniState(animator, "appear", 1.0f))
        {
            animator.SetBool("isAppear", false);
            transform.position = Vector3.zero;
        }
    }

    public void StartAppear()
    {
        animator.SetBool("isAppear", true);
    }
}
