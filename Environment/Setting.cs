using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChestStringCompare : IComparer
{
    public int Compare(object x, object y)
    {
        ChestList cX = (ChestList)x;

        return string.Compare(cX.name, (string)y);
    }
}

public class Setting
{
    public static float ANIMAL_MAXGLOW = 7;
    public static float PLAYER_FEEDINTERVAL = 30;
    public static float TREE_SPAWNINTERVAL = 3;

    public static Vector2 OUTSIDE_MIN = new Vector2(9, -5);
    public static Vector2 OUTSIDE_MAX = new Vector2(21, -14);
}

public class MyKeySetting
{
    public string inventory
    {
        get { return inventory; } 
        set
        {
            inventory = value;
        }
    }

    public string pause
    {
        get { return pause; }
        set
        {
            pause = value;
        }
    }
}




// json 파일 로드할 때는 직렬화 필수

[System.Serializable]
public class PlayerInfor
{
    public ItemListParent itemListParent;
    public MyQuickSlot myQuickSlot;
    public PlayerResource playerResource;
    public GroundInforParent groundInforParent;
}

[System.Serializable]
public class ItemListParent
{
    /// <summary>
    /// 1번째 : 장비,
    /// 2번째 : 무기,
    /// 3번째 : 음식,
    /// 4번째 : 작물,
    /// 5번째 : 기타
    /// </summary>
    public ItemList[] items; 
}

[System.Serializable]
public class ItemList
{
    public string[] item;
}

[System.Serializable]
public class MyQuickSlot
{
    public string[] items;
}

[System.Serializable]
public class ChestListParent
{
    public ChestList[] chest;
}

[System.Serializable]
public class ChestList
{
    public string name;
    public string[] item;
}

[System.Serializable]
public class SeedInfor
{
    public string[] seedInfor;
}

[System.Serializable]
public class ShopInfor
{
    public string[] shopInfor;
}

[System.Serializable]
public class PlayerResource
{
    public int money;
}

[System.Serializable]
public class GroundInforParent
{
    public GroundInfor groundInfor;
}

[System.Serializable]
public class GroundInfor
{
    public string size;
    public string[] obstaclePos;
    public string[] groundPos;
    public string[] seedPos;

    public string[] prohibitPos;
    public bool[,] prohibitList;
}

[System.Serializable]
public class NpcListParent
{
    public NpcList[] npc;
}

[System.Serializable]
public class NpcList
{
    public string name;
    public string[] dialogue;
    public string[] sucDialogue;
    public Quest quest;

    public bool isAccept;
}

[System.Serializable]
public class Quest
{
    public string name;
    public string needItem;
    public int count;
    public int reward;

    public bool isSuccess;
}