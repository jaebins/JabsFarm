using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlot : MonoBehaviour
{
    GameManager gameManager;

    public Image[] img_quickSlots;
    public Transform[][] panel_items;

    public List<ItemImg> items = new List<ItemImg>();

    public void SetInit()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // 퀵아이템 패널들 불러오기
        img_quickSlots = new Image[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            img_quickSlots[i] = transform.GetChild(i).GetComponent<Image>();

        LoadItemPanel();
        LoadItem();
    }

    void LoadItemPanel()
    {
        panel_items = new Transform[transform.childCount][];

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform item_h = transform.GetChild(i).transform;
            Transform[] temp = new Transform[1] { item_h };
            panel_items[i] = temp;
        }
    }

    void LoadItem()
    {
        MyQuickSlot myQuickSlot = JsonUtility.FromJson<MyQuickSlot>(gameManager.resourcesManager.datas_player["MyQuickSlot"]);
        for(int i = 0; i < myQuickSlot.items.Length; i++)
        {
            string[] cutItem = myQuickSlot.items[i].Split(' ');
            gameManager.SetItem(items, $"{cutItem[0]}Img", "Quick", int.Parse(cutItem[1]), false);
        }

        gameManager.InstallItemOnPanel(items, panel_items);
    }
}
