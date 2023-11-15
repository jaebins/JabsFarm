using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    // 외부 오브젝트
    public GameManager gameManager;

    // 내부 인스펙터
    SpriteRenderer spriteRenderer;

    // 설정
    public string seedName;
    public Sprite[] seedSprites;

    // 진행
    public bool startGrow;
    public int growCount;
    float flowTime;
    float[] sucTime;

    public void SetInit()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (startGrow)
        {
            flowTime += Time.deltaTime;
            if(flowTime > sucTime[growCount])
            {
                flowTime = 0;
                spriteRenderer.sprite = seedSprites[++growCount];

                if (growCount == sucTime.Length - 1) StopGrow();
            }
        }
    }
    
    public void StartGorw(string seedName, int growCount)
    {
        this.seedName = seedName;
        this.growCount = growCount;

        seedSprites = gameManager.resourcesManager.seedSprites[seedName];
        spriteRenderer.sprite = seedSprites[growCount];
        sucTime = gameManager.seedInfors[seedName];

        startGrow = true;
    }

    public void StopGrow()
    {
        growCount = 0;

        startGrow = false;
    }
}
