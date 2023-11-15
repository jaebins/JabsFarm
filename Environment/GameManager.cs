using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 내부 인스펙터
    public AudioSource audioSource;

    // 외부 오브젝트
    public Transform canvas;
    public ResourcesManager resourcesManager;
    public ObjectManager objectManager;
    public Player player;
    public ItemEvent itemEvent;
    public Transform seedsParent;

    // UI
    public GameObject panel_loading;
    public GameObject panel_inventory;
    public GameObject panel_chest;
    public GameObject panel_quickSlots;
    public GameObject panel_shop;
    public GameObject panel_npcDialogue;
    public Scrollbar healthbar;
    public Scrollbar feedbar;

    // 데이터 인스턴스
    public ItemListParent myItem;
    public QuickSlot quickSlot;
    public Inventory inventory;
    public PlayerResource playerResource;
    public List<Chest> chests = new List<Chest>();
    public List<Quest> nowQuest = new List<Quest>();

    // 게임 진행 변수
    public bool isStart;
    public int nowQuickIndex;
    public Chest nowOpenChest;
    public Image img_nowQuickSlot;
    public ItemImg nowQuickItem;
    public List<Stack<Animal>> canMeetingAnimal = new List<Stack<Animal>>();

    // 설정 값
    public ChestList[] chestLists;
    public NpcList[] npcLists;
    public Dictionary<string, float[]> seedInfors = new Dictionary<string, float[]>();
    public Shop shop;
    public GroundInfor groundInfor;
    public int[] groundSize;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        canvas = GameObject.Find("Canvas").transform;
        resourcesManager = GameObject.Find("ResourcesManager").GetComponent<ResourcesManager>();
        objectManager = GameObject.Find("ObjectManager").GetComponent<ObjectManager>();
        player = GameObject.Find("Player").GetComponent<Player>();
        itemEvent = GameObject.Find("ItemEvent").GetComponent<ItemEvent>();
        seedsParent = GameObject.Find("Seeds").transform;

        panel_loading = canvas.Find("Loading").gameObject;
        panel_inventory = canvas.Find("Inventory").gameObject;
        panel_chest = canvas.Find("Chest").gameObject;
        panel_quickSlots = canvas.Find("Quick").gameObject;
        panel_shop = canvas.Find("Shop").gameObject;
        panel_npcDialogue = canvas.Find("NpcDialogue").gameObject;
        healthbar = canvas.Find("PlayerPanel").Find("Healthbar").GetComponent<Scrollbar>();
        feedbar = canvas.Find("PlayerPanel").Find("Feedbar").GetComponent<Scrollbar>();

        inventory = panel_inventory.GetComponent<Inventory>();
        quickSlot = panel_quickSlots.GetComponent<QuickSlot>();

        panel_loading.SetActive(true);

        StartCoroutine(SetInit());
    }

    IEnumerator SetInit()
    {
        yield return new WaitForSeconds(1.0f);

        // 씨앗 정보 불러오기
        SeedInfor seedInforsClass = JsonUtility.FromJson<SeedInfor>(resourcesManager.datas_env["SeedInfor"]);
        foreach(string seedInfor in seedInforsClass.seedInfor)
        {
            string[] cut1 = seedInfor.Split(" "); // 0 : 과일 이름, 1 : 시간 정보
            float[] cut2 = Array.ConvertAll(cut1[1].Split(","), (x) => float.Parse(x));
            seedInfors.Add(cut1[0], cut2);
        }

        // 플레이어 정보 불러오기
        playerResource = JsonUtility.FromJson<PlayerResource>(resourcesManager.datas_player["PlayerResource"]);

        // 상자 정보 불러오기
        ChestListParent chestListParent = JsonUtility.FromJson<ChestListParent>(resourcesManager.datas_env["ChestInfor"]);
        chestLists = chestListParent.chest;
        foreach (ChestList chestList in chestLists)
        {
            Chest chest = GameObject.Find("Objects").transform.Find(chestList.name).GetComponent<Chest>();
            nowOpenChest = chest;
            chest.SetInit();
        }
        nowOpenChest = null;

        // Npc 정보 불러오기
        NpcListParent npcListParent = JsonUtility.FromJson<NpcListParent>(resourcesManager.datas_env["NpcInfor"]);
        npcLists = npcListParent.npc;

        // 동물 기본설정
        for (int i = 0; i < 2; i++)
            canMeetingAnimal.Add(new Stack<Animal>());

        inventory.SetInit();
        quickSlot.SetInit();
        objectManager.SetInit();
        player.SetInit();
        
        LoadGround();

        // 로딩 패널 끄기
        panel_loading.SetActive(false);

        isStart = true;
    }
    
    void LoadGround()
    {
        // y축은 음수부터 시작하기 때문에 json 파싱 때 변환해줘야함
        GroundInforParent groundInforParent = JsonUtility.FromJson<GroundInforParent>(resourcesManager.datas_player["Ground"]);
        groundInfor = groundInforParent.groundInfor;
        Debug.Log(groundInfor.size);

        groundSize = Array.ConvertAll(groundInfor.size.Split(" "), (a) => int.Parse(a)); // x, y

        // 저장된 obstacle 타일 설정
        for (int i = 0; i < groundInfor.obstaclePos.Length; i++)
        {
            string[] tileInfor = groundInfor.obstaclePos[i].Split(' '); // x, y, tileName
            Vector3Int pos = new Vector3Int(int.Parse(tileInfor[0]), -int.Parse(tileInfor[1]));

            player.obstacleTile.SetTile(pos, resourcesManager.tiles[tileInfor[2]]);
        }

        // 저장된 ground 타일 설정
        for (int i = 0; i < groundInfor.groundPos.Length; i++)
        {
            string[] tileInfor = groundInfor.groundPos[i].Split(' '); // x, y, tileName
            Vector3Int pos = new Vector3Int(int.Parse(tileInfor[0]), -int.Parse(tileInfor[1]));

            player.groundTile.SetTile(pos, resourcesManager.tiles[tileInfor[2]]);
        }

        // 설치 금지된 타일 설정
        groundInfor.prohibitList = new bool[groundSize[0], groundSize[1]];
        for (int i = 0; i < groundInfor.prohibitPos.Length; i++)
        {
            int[] prohibitInfor = Array.ConvertAll(groundInfor.prohibitPos[i].Split(' '), (a) => int.Parse(a)); // x, y

            groundInfor.prohibitList[prohibitInfor[0], prohibitInfor[1]] = true;
        }

        // 기본 타일 설정
        for (int i = 0; i < groundSize[0]; i++)
        {
            for(int j = 0; j < groundSize[1]; j++)
            {
                if (player.groundTile.GetTile(new Vector3Int(i, -j)) == null && !groundInfor.prohibitList[i, j])
                {
                    player.groundTile.SetTile(new Vector3Int(i, -j), resourcesManager.tiles["Grass_Basic"]);
                }
            }
        }

        // 심어진 씨앗 설정
        for(int i = 0; i < groundInfor.seedPos.Length; i++)
        {
            string[] seedInfor = groundInfor.seedPos[i].Split(" "); // seedName, seedGrowIndex, x, y

            Vector3 pos = new Vector3(int.Parse(seedInfor[2]), int.Parse(seedInfor[3]));
            Seed seed = player.playerEvent.CreateSeed(seedInfor[0], int.Parse(seedInfor[1]), pos);
        }
    }

    public void QuickSlotEvent(int index)
    {
        if (quickSlot.img_quickSlots[index].color.g < 1) // 아이템 사용
        {
            if(nowQuickItem != null)
            {
                ItemImg item = img_nowQuickSlot.transform.GetChild(0).GetComponent<ItemImg>();

                if (item.CompareTag("Fruit") || item.CompareTag("Drink"))
                {
                    itemEvent.EatFood(item.originalName);
                }
            }
        }
        else // 아니라면 변경
        {
            quickSlot.img_quickSlots[index].color = new Color(1, 0.2f, 0.2f, 1);
            
            if (img_nowQuickSlot != null) img_nowQuickSlot.color = new Color(1, 1, 1, 1);
            img_nowQuickSlot = quickSlot.img_quickSlots[index];

            ItemImg item = img_nowQuickSlot.transform.GetChild(0).GetComponent<ItemImg>();
            nowQuickItem = item;

            player.playerEvent.PreviewInstallBlock(nowQuickItem, player.frontPos, true);
        }

    }

    // 패널 새로고침
    public void InstallItemOnPanel(List<ItemImg> items, Transform[][] panel_items)
    {
        int index = 0;

        for (int i = 0; i < panel_items.GetLength(0); i++)
        {
            for (int j = 0; j < panel_items[i].Length; j++)
            {
                ResetItemImg(panel_items[i][j]);

                // 아이템이 있다면
                if (items.Count > index)
                {
                    ItemImg myItem = items[index++];
                    myItem.InstallOnPanel(myItem.transform, panel_items[i][j]);
                }
            }
        }

        //
        bool isRefreshQuick = isStart && panel_items[0][0].parent.name.Equals("Quick");
        if (isRefreshQuick)
        {
            if (img_nowQuickSlot.transform.childCount > 1)// 현재 퀵슬롯에 아이템이 있다면
                nowQuickItem = img_nowQuickSlot.transform.GetChild(0).GetComponent<ItemImg>();
            else if(img_nowQuickSlot.transform.childCount <= 1) // 현재 퀵슬롯에 아이템이 없다면
                nowQuickItem = null;

            player.playerEvent.PreviewInstallBlock(nowQuickItem, player.GetNowFrontPos(), true); // 미리보기 갱신
        }
    }

    public void ResetItemImg(Transform paenl_item)
    {
        if (paenl_item.childCount > 1)
        {
            paenl_item.GetChild(0).parent = null;
        }
        paenl_item.Find("text_Count").gameObject.SetActive(false);
    }

    // 아이템 획득 시 사용
    public ItemImg SetItem(List<ItemImg> items, string name, string type, int count, bool useDurability)
    {
        Transform[][] items_Panel = null;

        switch (type)
        {
            case "Chest":
                items_Panel = nowOpenChest.panel_items;
                break;
            case "Inventory":
                items_Panel = inventory.panel_items;
                break;
            case "Quick":
                items_Panel = quickSlot.panel_items;
                break;
        }

        int itemIndex = CheckOverlap(items, GetGameObjectName(name, type));
        ItemImg item = null;
        
        if(itemIndex < 0)
        {
            // 아이템 패널이 꽉 찼을 때
            if (items.Count == items_Panel.Length * items_Panel[0].Length) return null;

            item = GameObject.Find(GetGameObjectName(name, type)).GetComponent<ItemImg>();

            // 내구도
            if (item.GetComponent<Durability>() != null)
                item.GetComponent<Durability>().SetInit();

            item = item.GetComponent<ItemImg>();
            item.count = count;
            items.Add(item);
        }
        else
        {
            item = items[itemIndex];

            bool activeDurability = false;

            // useDurability : 내구도를 사용하는지, 바로 개수를 조정할 지 체크
            if (items[itemIndex].GetComponent<Durability>() != null && useDurability)
            {
                activeDurability = items[itemIndex].GetComponent<Durability>().SetDurability(-1);
            }

            // 내구도가 아예 없거나 소멸됬다면
            if (!activeDurability)
            {
                items[itemIndex].count += count;
                if (items[itemIndex].count <= 0)
                {
                    //if (type.Equals("Quick")) nowQuickItem = null;

                    items[itemIndex].transform.parent = null;
                    items.RemoveAt(itemIndex);
                }
            }
        }

        return item;
    }

    public int CheckOverlap(List<ItemImg> items, string originalItemName)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name.Equals(originalItemName)) return i;
        }
        return -1;
    }

    public void SetHealth(float value)
    {
        float healValue = value;

        // 회복될 체력이 최대 체력보다 크면 남은 체력 모두 회복
        if (player.nowHealth + value > player.maxHealth)
            healValue = player.maxHealth - player.nowHealth;

        player.nowHealth += healValue;
        healthbar.size = player.nowHealth / player.maxHealth;
    }

    public void SetFeed(float value)
    {
        float feedValue = value;

        // 회복될 허기량이 최대 허기량보다 크면 남은 허기량 모두 회복
        if (player.nowFeed + value > player.maxFeed)
            feedValue = player.maxFeed - player.nowFeed;

        player.nowFeed += feedValue;
        feedbar.size = player.nowFeed / player.maxFeed;
    }

    public void SetMoney(int value)
    {
        playerResource.money += value;
        inventory.text_money.text = $"Money : {playerResource.money} $";

        // 코인 획득 애니메이션
        player.ShowAniEvent(player.coinAni);
    }

    public static bool IsGetAniState(Animator animator, string name, float time)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= time;
    }

    public static void PlaySound(AudioSource audioSource, AudioClip clip, bool isLoop)
    {
        audioSource.clip = clip;
        audioSource.loop = isLoop;
        audioSource.Play();
    }

    public static string GetGameObjectName(string name, string type)
    {
        return $"{name}(Clone)_{type}";
    }

    public static string GetPrefebName(string name)
    {
        return $"{name}(Clone)";
    }

    public void Save()
    {
        // 경로가 없다면 (후에 저장경로 수정)
        if (!Directory.Exists(Path.Combine(Application.dataPath, "Datas_Save")))
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Datas_Save"));

        // 퀵 슬롯
        MyQuickSlot myQuickSlot = new MyQuickSlot();
        myQuickSlot.items = GetStringData(quickSlot.items);

        // 인벤토리
        ItemListParent itemListParent = new ItemListParent();
        itemListParent.items = new ItemList[inventory.myItems.Count];
        for (int i = 0; i < inventory.myItems.Count; i++)
        {
            ItemList itemList = new ItemList();
            itemList.item = GetStringData(inventory.myItems[i]);

            itemListParent.items[i] = itemList;
        }

        // 플레이어 정보

        // 상자 정보
        ChestListParent chestListParent = new ChestListParent();
        chestListParent.chest = new ChestList[chests.Count];
        for (int i = 0; i < chests.Count; i++)
        {
            ChestList chestList = new ChestList();
            chestList.name = chests[i].name;
            chestList.item = GetStringData(chests[i].items);

            chestListParent.chest[i] = chestList;
        }

        // 타일 정보
        List<string> obstaclePos = new List<string>();
        List<string> groundPos = new List<string>();
        List<string> seedPos = new List<string>();

        for (int i = 0; i < groundSize[0]; i++)
        {
            for(int j = 0; j < groundSize[1]; j++)
            {
                if(player.obstacleTile.GetTile(new Vector3Int(i, -j)) != null)
                {
                    string tileName = player.obstacleTile.GetTile(new Vector3Int(i, -j)).name;
                    obstaclePos.Add($"{i} {j} {tileName}");
                }
            }
        }
        for (int i = 0; i < groundSize[0]; i++)
        {
            for (int j = 0; j < groundSize[1]; j++)
            {
                if (player.groundTile.GetTile(new Vector3Int(i, -j)) != null && 
                    !player.groundTile.GetTile(new Vector3Int(i, -j)).name.Equals("Grass_Basic"))
                {
                    string tileName = player.groundTile.GetTile(new Vector3Int(i, -j)).name;
                    groundPos.Add($"{i} {j} {tileName}");
                }
            }
        }
        for(int i = 0; i < seedsParent.childCount; i++)
        {
            Seed seed = seedsParent.GetChild(i).GetComponent<Seed>();
            seedPos.Add($"{seed.seedName} {seed.growCount} {seed.transform.position.x} {seed.transform.position.y}");
        }

        groundInfor.obstaclePos = obstaclePos.ToArray();
        groundInfor.groundPos = groundPos.ToArray();
        groundInfor.seedPos = seedPos.ToArray();

        GroundInforParent groundInforParent = new GroundInforParent
        {
            groundInfor = groundInfor
        };

        PlayerInfor playerInfor = new PlayerInfor
        {
            itemListParent = itemListParent,
            myQuickSlot = myQuickSlot,
            playerResource = playerResource,
            groundInforParent = groundInforParent
        };
        string text = JsonUtility.ToJson(playerInfor);

        DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;
        db.Child(resourcesManager.loginManager.userId).SetValueAsync(text);

        //SaveFile("MyQuickSlot.json", quickSlotText);
        //SaveFile("MyItem.json", inventoryText);
        //SaveFile("Player.json", playerInforText);
        //SaveFile("Chest.json", chestText);
        //SaveFile("Ground.json", groundText);
    }

    //public static void SaveFile(string jsonName, string tarText)
    //{
    //    if (!File.Exists(Path.Combine(Application.dataPath, "Datas_Save", jsonName)))
    //    {
    //        using (FileStream fs = File.Create(jsonName))
    //            fs.Dispose();
    //    }

    //    File.WriteAllText(Path.Combine(Application.dataPath, "Datas_Save", jsonName), string.Empty);
    //    File.WriteAllText(Path.Combine(Application.dataPath, "Datas_Save", jsonName), tarText);
    //}

    public static string[] GetStringData(List<ItemImg> items)
    {
        string[] textArr = new string[items.Count];

        for (int i = 0; i < textArr.Length; i++)
        {
            string text = $"{items[i].originalName.Substring(0, items[i].originalName.Length - 3)} {items[i].count}";
            textArr[i] = text;
        }

        return textArr;
    }

    void OnApplicationQuit()
    {
        if (isStart)
        {
            Save();
        }
    }
}
