using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : MonoBehaviour
{
    GameManager gameManager;

    float dropEggInterval = 60;
    float flowDropEggTime;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        DropEgg();
    }

    void DropEgg()
    {
        if(transform.localScale.x >= Setting.ANIMAL_MAXGLOW)
        {
            flowDropEggTime += Time.deltaTime;

            if(flowDropEggTime > dropEggInterval)
            {
                DropItem egg = gameManager.objectManager.GetObj("Egg").GetComponent<DropItem>();
                egg.transform.parent.position = transform.position;

                if (egg.animator == null) egg.SetInit();
                egg.StartAppear();

                flowDropEggTime = 0;
            }
        }
        }
}
