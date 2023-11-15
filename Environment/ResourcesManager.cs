using Firebase.Database;
using Google.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using UnityEngine.Windows;

public enum Seeds
{
    Corner,
    Peach,
    Pumkin,
    Strawberry
}

public class ResourcesManager : MonoBehaviour
{
    public LoginManager loginManager;

    // json 데이터 파일
    public Dictionary<string, string> datas_player = new Dictionary<string, string>();
    public Dictionary<string, string> datas_env = new Dictionary<string, string>();

    // 타일
    public Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();

    // 아이템
    public Dictionary<string, GameObject> itemsImg_inventory = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> itemsImg_chest = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> itemsImg_quickSlot = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> itemsImg_shop = new Dictionary<string, GameObject>();

    // 드롭 아이템
    public Dictionary<string, GameObject> dropItem = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> dropItem_fruit = new Dictionary<string, GameObject>();

     // 설치형 아이템 
    public Dictionary<string, GameObject> installItem = new Dictionary<string, GameObject>();
    public Dictionary<string, GameObject> animal = new Dictionary<string, GameObject>();

    // 스프라이트
    public Dictionary<string, Sprite[]> seedSprites = new Dictionary<string, Sprite[]>();
    public Dictionary<string, Sprite> npcDialogueIcon = new Dictionary<string, Sprite>();

    // UI
    public Dictionary<string, GameObject> otherUI = new Dictionary<string, GameObject>();

    // 사운드
    public Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();

    void Awake()
    {
        loginManager = GameObject.Find("LoginManager").GetComponent<LoginManager>();

        DataResource();
        GroundResource();
        ItemResource();
        DropItemResource();
        InstallItemReousrce();
        SpriteResource();
        UIResource();
        SoundResource();
    }

    async void DataResource()
    {
        Addressables.LoadAssetsAsync<TextAsset>("Dir_Data_Env", (obj) =>
        {
            datas_env.Add(obj.name, obj.text);
        });

        bool initSetting = false;
        string json = string.Empty;

        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        await db.Child(loginManager.userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log(task.Exception);
            }
            else
            {
                // 만약 데이터베이스에 데이터가 존재하지 않는다면
                if (task.Result.Value == null) initSetting = true;
                // 존재한다면
                else json = task.Result.Value.ToString();
            }
        });

        if (initSetting) // 첫 시작 세팅
        {
            Addressables.LoadAssetsAsync<TextAsset>("Dir_Data_Player", (obj) =>
            {
                datas_player.Add(obj.name, obj.text);
            });
        }
        else
        {
            Debug.Log(json);

            PlayerInfor playerInfor = JsonUtility.FromJson<PlayerInfor>(json);
            datas_player.Add("MyQuickSlot", JsonUtility.ToJson(playerInfor.myQuickSlot));
            datas_player.Add("MyItem", JsonUtility.ToJson(playerInfor.itemListParent));
            datas_player.Add("PlayerResource", JsonUtility.ToJson(playerInfor.playerResource));
            datas_player.Add("Ground", JsonUtility.ToJson(playerInfor.groundInforParent));
        }
    }

    void GroundResource()
    {
        Addressables.LoadAssetsAsync<Tile>("Dir_Tile", (obj) =>
        {
            tiles.Add(obj.name, obj);
        });
    }

    void ItemResource()
    {
        Addressables.LoadAssetsAsync<GameObject>("Dir_ItemsImg", (obj) =>
        {
            GameObject item_inventory = Instantiate(obj);
            item_inventory.name += "_Inventory";
            item_inventory.GetComponent<ItemImg>().originalName = obj.name;
            itemsImg_inventory.Add($"{item_inventory.name}", item_inventory);

            GameObject item_chest = Instantiate(obj);
            item_chest.name += "_Chest";
            item_chest.GetComponent<ItemImg>().originalName = obj.name;
            itemsImg_chest.Add($"{item_chest.name}", item_chest);

            GameObject item_quick = Instantiate(obj);
            item_quick.name += "_Quick";
            item_quick.GetComponent<ItemImg>().originalName = obj.name;
            itemsImg_quickSlot.Add($"{item_quick.name}", item_quick);

            GameObject item_shop = Instantiate(obj);
            item_shop.name += "_Shop";
            item_shop.GetComponent<ItemImg>().originalName = obj.name;
            itemsImg_shop.Add($"{item_shop.name}", item_shop);
        });
    }

    void DropItemResource()
    {
        Addressables.LoadAssetsAsync<GameObject>("Dir_DropItem", (obj) =>
        {
            dropItem.Add(obj.name, obj);
        });

        Addressables.LoadAssetsAsync<GameObject>("Dir_DropItem_Fruit", (obj) =>
        {
            dropItem_fruit.Add(obj.name, obj);
        });
    }

    void InstallItemReousrce()
    {
        Addressables.LoadAssetsAsync<GameObject>("Dir_InstallItem", (obj) =>
        {
            installItem.Add(obj.name, obj);
        });

        Addressables.LoadAssetsAsync<GameObject>("Dir_Animal", (obj) =>
        {
            animal.Add(obj.name, obj);
        });
    }

    async void SpriteResource()
    {
        for (int i = 0; i < Enum.GetValues(typeof(Seeds)).Length; i++)
        {
            List<Sprite> seedsArr = new List<Sprite>();

            await Addressables.LoadAssetsAsync<Sprite>($"Dir_Seed_{Enum.GetName(typeof(Seeds), i)}", (obj) =>
            {
                seedsArr.Add(obj);
            }).Task;

            seedSprites.Add(Enum.GetName(typeof(Seeds), i), seedsArr.ToArray());
        }

        Addressables.LoadAssetsAsync<GameObject>("Dir_OtherUI", (obj) =>
        {
            otherUI.Add(obj.name, obj);
        });
    }

    void UIResource()
    {
        Addressables.LoadAssetsAsync<Sprite>("Dir_NpcDialogue", (obj) =>
        {
            npcDialogueIcon.Add(obj.name, obj);
        });
    }

    void SoundResource()
    {
        Addressables.LoadAssetsAsync<AudioClip>("Dir_Sound", (obj) =>
        {
            sounds.Add(obj.name, obj);
        });

    }
}
