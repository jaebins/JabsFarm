using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shop : MonoBehaviour, IObjectEvent, IItemPanel
{
    Animator animator;
    EventTrigger ev;

    GameManager gameManager;

    Transform[][] panel_items;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SetInit()
    {
        animator = GetComponent<Animator>();
        ev = GetComponent<EventTrigger>();
        
        AddClickClose();
        LoadItem();
    }

    public void AddClickClose()
    {
        EventTrigger ev = gameManager.panel_shop.transform.Find("but_close").GetComponent<EventTrigger>();

        EventTrigger.Entry clickEventT = new EventTrigger.Entry();
        clickEventT.eventID = EventTriggerType.PointerDown;
        clickEventT.callback.AddListener((e) =>
        {
            gameManager.panel_shop.SetActive(false);
            gameManager.inventory.gameObject.SetActive(false);

            animator.SetBool("isOpen", false);
            animator.SetBool("isClose", true);
        });
        ev.triggers.Add(clickEventT);
    }

    public void LoadItem()
    {
        Transform item_V = gameManager.panel_shop.transform.Find("Scroll View").Find("Viewport").Find("Content");
        Transform nowItem_h = null;

        string[] shopItems = JsonUtility.FromJson<ShopInfor>(gameManager.resourcesManager.datas_env["ShopInfor"]).shopInfor;

        for (int i = 0; i < shopItems.Length; i++)
        {
            string[] splitShopItem = shopItems[i].Split(' ');

            string itemName = $"{splitShopItem[0]}Img"; 
            int itemPrice = int.Parse(splitShopItem[1]);

            if (i % 2 == 0 || i == 0)
            {
                nowItem_h = Instantiate(gameManager.resourcesManager.otherUI["ShopItem_h"]).transform;
                nowItem_h.transform.parent = item_V;
            }

            GameObject shopItem = Instantiate(gameManager.resourcesManager.otherUI["ShopItem"]);
            shopItem.transform.parent = nowItem_h;

            string objectName = GameManager.GetGameObjectName(itemName, "Shop");
            ItemImg itemImg = Instantiate(gameManager.resourcesManager.itemsImg_shop[objectName]).GetComponent<ItemImg>();
            int type = itemImg.type;
            itemImg.transform.parent = shopItem.transform;
            itemImg.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            itemImg.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
            itemImg.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            itemImg.GetComponent<RectTransform>().anchoredPosition += Vector2.right * 20;
            itemImg.GetComponent<RectTransform>().sizeDelta -= new Vector2(30, 30);
            itemImg.transform.SetSiblingIndex(0);

            //shopItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = shopItem.GetComponent<RectTransform>().sizeDelta.x.ToString(); // 이름
            //shopItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = gameManager.panel_shop.GetComponent<RectTransform>().sizeDelta.x.ToString(); // 가격

            shopItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = itemImg.koreanName; // 이름
            shopItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{splitShopItem[1]} $"; // 가격
            shopItem.transform.GetChild(3).GetComponent<EventTrigger>().triggers.Add(AddPriceEvent(itemName, itemPrice, type));
        }
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

    EventTrigger.Entry AddPriceEvent(string itemName, int price, int type)
    {
        EventTrigger.Entry clickEvent = new EventTrigger.Entry();
        clickEvent.eventID = EventTriggerType.PointerClick;
        clickEvent.callback.AddListener((e) =>
        {
            if(gameManager.playerResource.money >= price)
            {
                GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["buy"], false);
                gameManager.SetMoney(-price);

                gameManager.SetItem(gameManager.inventory.myItems[type], itemName, "Inventory", 1, false);
                gameManager.InstallItemOnPanel(gameManager.inventory.myItems[type], gameManager.inventory.panel_items);
            }
        });

        return clickEvent;
    }

    public void ShopEvent()
    {
        gameManager.panel_shop.SetActive(true);

        if (animator == null) SetInit();
        
        animator.SetBool("isOpen", true);
    }
}
