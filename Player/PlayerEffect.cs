using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEvent : MonoBehaviour
{
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(GameManager.IsGetAniState(animator, "show", 1.0f))
        {
            animator.SetBool("isShow", false);
            gameObject.SetActive(false);
        }
    }
}
