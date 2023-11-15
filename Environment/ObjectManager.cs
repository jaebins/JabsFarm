using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    GameManager gameManager;

    GameObject[,] fruits;
    GameObject[] eggs;
    GameObject[] coins;
    GameObject[] treeSticks;
    public GameObject[] seeds;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SetInit()
    {
        fruits = new GameObject[gameManager.resourcesManager.dropItem_fruit.Count, 5];
        eggs = new GameObject[10];
        seeds = new GameObject[10];
        coins = new GameObject[10];
        treeSticks = new GameObject[10];

        int i = 0;
        foreach(GameObject item in gameManager.resourcesManager.dropItem_fruit.Values)
        {
            for(int j = 0; j < fruits.GetLength(1); j++)
            {
                GameObject fruit = Instantiate(item).transform.GetChild(0).gameObject;
                fruit.transform.parent.gameObject.SetActive(false);
                fruits[i, j] = fruit;
            }

            i++;
        }

        MakeGameObj("Egg", gameManager.resourcesManager.dropItem, eggs);
        MakeGameObj("Coin", gameManager.resourcesManager.dropItem, coins);
        MakeGameObj("TreeStick", gameManager.resourcesManager.dropItem, treeSticks);
        MakeGameObj("Seed", gameManager.resourcesManager.installItem, seeds);
    }

    public void MakeGameObj(string objName, Dictionary<string, GameObject> resources, GameObject[] objArr)
    {
        for (int j = 0; j < objArr.Length; j++)
        {
            GameObject obj = Instantiate(resources[objName]).transform.GetChild(0).gameObject;
            obj.transform.parent.gameObject.SetActive(false);
            objArr[j] = obj;
        }
    }

    public DropItem GetFruit(int index)
    {
        for(int i = 0; i < fruits.GetLength(1); i++)
        {
            if (!fruits[index, i].transform.parent.gameObject.activeSelf)
            {
                fruits[index, i].transform.parent.gameObject.SetActive(true);
                return fruits[index, i].GetComponent<DropItem>();
            }
        }

        return null;
    }

    public GameObject GetObj(string name)
    {
        GameObject[] tar = null;

        switch (name)
        {
            case "Seed":
                tar = seeds;
                break;
            case "Egg":
                tar = eggs;
                break;
            case "Coin":
                tar = coins;
                break;
            case "TreeStick":
                tar = treeSticks;
                break;
        }

        for (int i = 0; i < tar.Length; i++)
        {
            if (!tar[i].transform.parent.gameObject.activeSelf)
            {
                tar[i].transform.parent.gameObject.SetActive(true);
                return tar[i];
            }
        }

        return null;
    }
}
