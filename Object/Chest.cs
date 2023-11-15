using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Chest : MonoBehaviour, IObjectEvent, IItemPanel
{
    // 내부 인스펙터
    Animator animator;

    // 외부 오브젝트
    GameManager gameManager;

    // UI
    public Transform[][] panel_items;

    // 게임 진행
    public List<ItemImg> items = new List<ItemImg>();

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SetInit()
    {
        animator = GetComponent<Animator>();

        AddClickClose();
        LoadItemPanel();
        LoadItem();
    }

    public void AddClickClose()
    {
        EventTrigger ev = gameManager.panel_chest.transform.Find("but_close").GetComponent<EventTrigger>();

        EventTrigger.Entry clickEventT = new EventTrigger.Entry();
        clickEventT.eventID = EventTriggerType.PointerDown;
        clickEventT.callback.AddListener((e) =>
        {
            for (int i = 0; i < panel_items.GetLength(0); i++)
            {
                for (int j = 0; j < panel_items[i].Length; j++)
                    gameManager.ResetItemImg(panel_items[i][j]);
            }

            gameManager.panel_inventory.SetActive(false);
            gameManager.panel_chest.SetActive(false);

            animator.SetBool("isOpen", false);
            animator.SetBool("isClose", true);
        });
        ev.triggers.Add(clickEventT);
    }

    public void LoadItemPanel()
    {
        Transform item_V = gameManager.panel_chest.transform.Find("Item_v");

        panel_items = new Transform[item_V.childCount][];

        for (int i = 0; i < item_V.childCount; i++)
        {
            Transform item_h = item_V.GetChild(i).transform;
            Transform[] temp = new Transform[item_h.childCount];

            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = item_h.GetChild(j);
            }

            panel_items[i] = temp;
        }
    }

    public void LoadItem()
    {
        int index = Array.BinarySearch(gameManager.chestLists, name, new ChestStringCompare());
        string[] chestInfors = gameManager.chestLists[index].item;

        foreach (string item in chestInfors)
        {
            string[] cutItem = item.Split(' ');

            ItemImg itemImg = gameManager.SetItem(items, $"{cutItem[0]}Img", "Chest", int.Parse(cutItem[1]), false);
            if(itemImg != null) itemImg.myChest = this;
        }

        gameManager.chests.Add(this);
    }

    private void Update()
    {
        SetAniState();
    }

    public void SetAniState()
    {
        if (animator != null)
        {
            if (GameManager.IsGetAniState(animator, "close", 1.0f))
            {
                animator.SetBool("isClose", false);
            }
        }
    }

    public void ChestEvent()
    {
        gameManager.panel_chest.SetActive(true);

        gameManager.nowOpenChest = this;
        gameManager.InstallItemOnPanel(items, panel_items);

        animator.SetBool("isOpen", true);
    }
}
