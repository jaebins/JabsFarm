using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEvent : MonoBehaviour
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void EatFood(string name)
    {
        GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["eat_me"], false);

        int value = 0;

        switch (name)
        {
            case "AppleImg":
                value = 1;
                break;
            case "GrapeImg":
                value = 1;
                break;
            case "LimeImg":
                value = 1;
                break;
            case "OrangeImg":
                value = 1;
                break;

            case "CornerImg":
                value = 1;
                break;
            case "PeachImg":
                value = 1;
                break;
            case "PumkinImg":
                value = 1;
                break;
            case "StrawberryImg":
                value = 1;
                break;

            case "MilkImg":
                value = 1;
                break;
        }

        gameManager.SetFeed(value);
        gameManager.SetItem(gameManager.quickSlot.items, name, "Quick", -1, false);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);
    }
}
