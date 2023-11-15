using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Durability : MonoBehaviour
{
    GameManager gameManager;

    public Scrollbar scroll_durability;

    public float maxUseCount;
    public float nowUseCount;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SetInit()
    {
        if(scroll_durability == null)
        {
            scroll_durability = Instantiate(gameManager.resourcesManager.otherUI["Scroll_Durability"]).GetComponent<Scrollbar>();
            scroll_durability.transform.SetParent(transform);

            scroll_durability.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
            scroll_durability.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 20);
        }

        nowUseCount = maxUseCount;
        scroll_durability.size = nowUseCount / maxUseCount;
    }

    public bool SetDurability(float value)
    {
        nowUseCount += value;
        if(nowUseCount < 0)
        {
            nowUseCount = maxUseCount;
            scroll_durability.size = nowUseCount / maxUseCount;

            return false;
        }

        scroll_durability.size = nowUseCount / maxUseCount;
        return true;
    }
}
