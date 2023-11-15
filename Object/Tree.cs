using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour, IObjectEvent
{
    public Animator animator;
    BoxCollider2D boxCollider;
    SpriteRenderer spriteRenderer;
    
    GameManager gameManager;

    public int fallFoodCnt;
    public Dictionary<int, string> fruitsName = new Dictionary<int, string>();

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if (animator != null) SetAniState();
    }

    public void SetInit()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        fallFoodCnt = UnityEngine.Random.Range(0, 5);

        int i = 1;
        foreach(string key in gameManager.resourcesManager.dropItem_fruit.Keys)
        {
            fruitsName.Add(i++, key);
        }
    }

    public void SetAniState()
    {
        if (GameManager.IsGetAniState(animator, "hit", 1.0f))
            animator.SetBool("isHit", false);

        if (GameManager.IsGetAniState(animator, "disappear", 1.0f))
        {
            animator.SetBool("isDisappear", false);
            animator.enabled = false;
            boxCollider.enabled = false;
            spriteRenderer.color = new Color(1, 1, 1, 0);
        }
        if (GameManager.IsGetAniState(animator, "appear", 1.0f))
        {
            animator.SetBool("isAppear", false);
            boxCollider.enabled = true;
            spriteRenderer.color = new Color(1, 1, 1, 1);
        }
    }

    public int StartHitEvent()
    {
        if(fallFoodCnt == 0)
        {
            animator.SetBool("isDisappear", true);
            StartCoroutine(Respawn());

            int result = UnityEngine.Random.Range(0, 6);

            // 0 : 열매, 1 : 코인
            switch (result)
            {
                case 0:
                    return UnityEngine.Random.Range(0, gameManager.resourcesManager.dropItem_fruit.Count); // 열매 반환
                case int n when (n >= 1 && n <= 4):
                    return gameManager.resourcesManager.dropItem_fruit.Count;
                case 5:
                    return gameManager.resourcesManager.dropItem_fruit.Count + 1; // 마지막 열매 인덱스 다음 인덱스 반환
            }
        }

        animator.SetBool("isHit", true);
        fallFoodCnt--;

        return -1;
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(Setting.TREE_SPAWNINTERVAL);

        animator.enabled = true;
        animator.SetBool("isAppear", true);

        fallFoodCnt = UnityEngine.Random.Range(0, 5);
    }
}
