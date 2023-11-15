using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;

public class Inventory : MonoBehaviour, IItemPanel
{
    // 내부 인스펙터
    RectTransform rectTransform;
    EventTrigger ev;

    // 외부 오브젝트
    GameManager gameManager;
    Image[] img_categorys;
    public TextMeshProUGUI text_money;
    public List<List<ItemImg>> myItems = new List<List<ItemImg>>(); // 아이템들

    // UI
    public Transform[][] panel_items;

    // 게임 진행 관련 변수
    Vector2 startMovePos;
    Image img_nowCategory;

    public void SetInit()
    {
        rectTransform = GetComponent<RectTransform>();
        ev = GetComponent<EventTrigger>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // 닫기 이벤트
        AddClickClose();

        // 카테고리 선택 이벤트
        Transform categorys = transform.Find("Categorys").Find("Categorys");
        img_categorys = new Image[categorys.childCount];
        for (int i = 0; i < categorys.childCount; i++)
        {
            AddClickCategory(categorys.GetChild(i).GetComponent<EventTrigger>(), i);
            img_categorys[i] = categorys.GetChild(i).GetComponent<Image>();
        }
        AddDragPanelEvent();

        // 인벤토리 초기 설정
        LoadItemPanel();
        LoadItem();
    }

    public void AddClickClose()
    {
        EventTrigger ev = transform.Find("but_close").GetComponent<EventTrigger>();

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
        });
        ev.triggers.Add(clickEventT);
    }

    void AddClickCategory(EventTrigger ev, int i)
    {
        EventTrigger.Entry clickEventT = new EventTrigger.Entry();
        clickEventT.eventID = EventTriggerType.PointerDown;
        clickEventT.callback.AddListener((e) =>
        {
            ChangeCategory(i);
        });
        ev.triggers.Add(clickEventT);
    }
    
    void AddDragPanelEvent()
    {
        EventTrigger.Entry clickEventT = new EventTrigger.Entry();
        clickEventT.eventID = EventTriggerType.PointerDown;
        clickEventT.callback.AddListener((e) =>
        {
            startMovePos = new Vector2(Input.mousePosition.x - (Screen.width / 2) - rectTransform.anchoredPosition.x,
                Input.mousePosition.y - (Screen.height / 2) - rectTransform.anchoredPosition.y);
        });
        ev.triggers.Add(clickEventT);

        EventTrigger.Entry dragEventT = new EventTrigger.Entry();
        dragEventT.eventID = EventTriggerType.Drag;
        dragEventT.callback.AddListener((e) =>
        {
            rectTransform.anchoredPosition = new Vector2((Input.mousePosition.x - (Screen.width / 2)) - startMovePos.x,
                (Input.mousePosition.y - (Screen.height / 2)) - startMovePos.y);
        });
        ev.triggers.Add(dragEventT);
    }

    public void LoadItemPanel()
    {
        Transform item_V = transform.Find("Item_v");
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
        text_money = transform.Find("text_money").GetComponent<TextMeshProUGUI>();
        gameManager.SetMoney(0);

        string textAsset = gameManager.resourcesManager.datas_player["MyItem"];
        gameManager.myItem = JsonUtility.FromJson<ItemListParent>(textAsset);
        foreach(ItemList itemList in gameManager.myItem.items)
        {
            List<ItemImg> temp = new List<ItemImg>();

            foreach(string itemName in itemList.item)
            {
                string[] cutItem = itemName.Split(' '); // 0 : 아이템 이름, 1 : 아이템 갯수

                gameManager.SetItem(temp, $"{cutItem[0]}Img", "Inventory", int.Parse(cutItem[1]), false);
            }

            myItems.Add(temp);
        }
    }

    public void ChangeCategory(int index)
    {
        // 이미 선택된 카테고리라면
        if (img_categorys[index].color.g >= 1)
        {
            img_categorys[index].color = new Color(1, 0.2f, 0.2f, 1);

            if (img_nowCategory != null) img_nowCategory.color = new Color(1, 1, 1, 1);
            img_nowCategory = img_categorys[index];
        }

        if(myItems.Count > index)
            gameManager.InstallItemOnPanel(myItems[index], panel_items);
    }
}
