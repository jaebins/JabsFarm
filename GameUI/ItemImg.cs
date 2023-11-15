using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemImg : MonoBehaviour
{
    // 내부 인스펙터
    EventTrigger ev;

    // 외부 오브젝트
    GameManager gameManager;

    // 설정
    public int type;
    public int count;
    public int sellPrice;
    public string originalName;
    public string koreanName;
    public Chest myChest;
    public TextMeshProUGUI text_count;

    bool isClickLeft;
    bool isClickRight;

    void Start()
    {
        ev = GetComponent<EventTrigger>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((e) =>
        {
            if (isClickLeft) // 좌클릭
                LeftClickItem();
            else if (isClickRight) // 우클릭
                RightClickItem();
        });
        ev.triggers.Add(entry);
    }

    private void Update()
    {
        isClickLeft = Input.GetMouseButton(0);
        isClickRight = Input.GetMouseButton(1);
    }

    void LeftClickItem()
    {
        GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["itemClick"], false);

        // 퀵 슬롯은 계층 구조가 다르기 때문에 부모 존재 확인으로 판단
        string parentName = transform.parent.parent.parent.parent != null ? transform.parent.parent.parent.parent.name : "";

        bool inventoryToQuick = transform.parent.parent.parent.parent == null && gameManager.panel_inventory.activeSelf;

        if (inventoryToQuick) // 퀵 슬롯에서 인벤토리
        {
            ChangeItemState("Inventory", "Quick", gameManager.inventory.myItems[type], gameManager.quickSlot.items);
            gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items); // 상자 재설정
        }
        else if (parentName.Equals("Chest")) // 상자에서 인벤토리
        {
            ChangeItemState("Inventory", "Chest", gameManager.inventory.myItems[type], gameManager.nowOpenChest.items);
            gameManager.InstallItemOnPanel(gameManager.nowOpenChest.items, gameManager.nowOpenChest.panel_items); // 상자 재설정
        }
        else if(parentName.Equals("Inventory")) // 인벤토리에 있는 아이템
        {
            if (!gameManager.panel_chest.activeSelf) // 인벤토리에서 퀵 슬롯
            {
                ChangeItemState("Quick", "Inventory", gameManager.quickSlot.items, gameManager.inventory.myItems[type]);
                gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items); // 상자 재설정
            }
            else // 인벤토리에서 상자
            {
                ChangeItemState("Chest", "Inventory", gameManager.nowOpenChest.items, gameManager.inventory.myItems[type]);
                gameManager.InstallItemOnPanel(gameManager.nowOpenChest.items, gameManager.nowOpenChest.panel_items); // 상자 재설정
            }
        }
    }

    void RightClickItem()
    {
        // 아이템 되팔기
        if(gameManager.panel_shop.activeSelf)
        {
            GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["sell"], false);

            gameManager.SetItem(gameManager.inventory.myItems[type], originalName, "Inventory", -1, false);
            gameManager.InstallItemOnPanel(gameManager.inventory.myItems[type], gameManager.inventory.panel_items);
            gameManager.SetMoney(sellPrice);
        }
    }

    void ChangeItemState(string type, string type2, List<ItemImg> items, List<ItemImg> items2)
    {
        // 내구도가 있는 아이템인지 확인하기 위해 useDurability 참으로 둠 (내구도가 없다면 알아서 체크X)
        ItemImg plusItem = gameManager.SetItem(items, originalName, type, 1, false);

        // 이동 아이템 패널이 꽉차지 않았을 때
        if(plusItem != null)
        {
            Durability minusItem = gameManager.SetItem(items2, originalName, type2, -1, false).GetComponent<Durability>();
            gameManager.inventory.ChangeCategory(this.type); // 인벤토리 재설정

            //// 내구도 아이템 옮길 때 내구도 옮기기
            if (minusItem != null && minusItem.GetComponent<Durability>() != null)
            {
                float remainDura = minusItem.maxUseCount - minusItem.nowUseCount;
                plusItem.GetComponent<Durability>().SetDurability(-remainDura);
            }
        }

    }

    public void InstallOnPanel(Transform target, Transform panel)
    {
        target.transform.SetParent(panel);

        target.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        target.GetComponent<RectTransform>().sizeDelta = panel.GetComponent<RectTransform>().sizeDelta - new Vector2(30, 30);
        target.transform.SetSiblingIndex(0);

        text_count = panel.GetChild(1).GetComponent<TextMeshProUGUI>();
        text_count.gameObject.SetActive(true);
        text_count.GetComponent<TextMeshProUGUI>().text = count.ToString();

        text_count = panel.GetChild(1).GetComponent<TextMeshProUGUI>();
        text_count.gameObject.SetActive(true);
        text_count.GetComponent<TextMeshProUGUI>().text = count.ToString();
    }
}
