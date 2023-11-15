using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerEvent : MonoBehaviour
{
    GameManager gameManager;
    Player player;

    int npcIndex;
    int nowDialogueCnt;
    string[] nowDialogue;
    public TextMeshProUGUI text_npcContent;
    IEnumerator dialougeTask;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        player = gameManager.player;
    }

    // ----------------- Event ----------------- //

    public Seed CreateSeed(string seedName, int growCount, Vector3 pos)
    {
        Seed seed = gameManager.objectManager.GetObj("Seed").GetComponent<Seed>();
        seed.transform.parent = gameManager.seedsParent;
        seed.transform.parent.position = pos;

        if (seed.gameManager == null) seed.SetInit();
        seed.StartGorw(seedName, growCount);

        return seed;
    }

    public void PickupEvent(Collider2D collision)
    {
        if (collision.CompareTag("Fruit"))
        {
            PickupItem($"{collision.name}Img");
            collision.transform.parent.gameObject.SetActive(false);
        }
        else if (collision.CompareTag("Seed") && !collision.GetComponent<Seed>().startGrow)
        {
            PickupItem($"{collision.GetComponent<Seed>().seedName}Img");
            gameManager.SetMoney(100);
            collision.transform.parent.gameObject.SetActive(false);
        }
        else if (collision.CompareTag("Coin"))
        {
            gameManager.SetMoney(500);
            collision.transform.parent.gameObject.SetActive(false);
        }
        else if (collision.CompareTag("DropItem"))
        {
            PickupItem($"{collision.name}Img");
            collision.transform.parent.gameObject.SetActive(false);
        }

    }

    public void PickupItem(string itemName)
    {
        GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["pickupItem"], false);

        if (player.ProhibitPickUp())
        {
            gameManager.SetItem(gameManager.inventory.myItems[2], $"{itemName}", "Inventory", 1, false);
            gameManager.InstallItemOnPanel(gameManager.inventory.myItems[2], gameManager.inventory.panel_items);
        }
        else
        {
            gameManager.SetItem(gameManager.quickSlot.items, $"{itemName}", "Quick", 1, false);
            gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);
        }
    }

    // ----------------- Object Event ----------------- //

    public void OpenChest(Chest chest)
    {
        player.OpenInventory(new Vector2(570, 50));
        chest.ChestEvent();
    }

    public void OpenShop(Shop shop)
    {
        player.OpenInventory(new Vector2(570, 50));
        shop.ShopEvent();
    }

    public void OpenNpcDialogue(in Collider2D npc)
    {
        // 대화가 끝나면 다시 패널이 열리는 것을 방지
        if (nowDialogue != null && nowDialogueCnt == nowDialogue.Length)
        {
            nowDialogueCnt = 0;
            return;
        }

        npcIndex = int.Parse(npc.name[^1].ToString());
        gameManager.panel_npcDialogue.SetActive(true);

        AudioSource npcAudio = npc.GetComponent<AudioSource>();
        GameManager.PlaySound(npcAudio, gameManager.resourcesManager.sounds[$"npc{npcIndex}"], false);

        Sprite npcIcon = gameManager.resourcesManager.npcDialogueIcon[npc.name];
        gameManager.panel_npcDialogue.transform.GetChild(0).GetComponent<Image>().sprite = npcIcon;
        gameManager.panel_npcDialogue.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = gameManager.npcLists[npcIndex].name;
        text_npcContent = gameManager.panel_npcDialogue.transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        if (gameManager.npcLists[npcIndex].isAccept) // 퀘스트를 받은 상태라면
        {
            Quest quest = gameManager.npcLists[npcIndex].quest;
            int itemIndex = gameManager.CheckOverlap(gameManager.quickSlot.items, GameManager.GetGameObjectName($"{quest.needItem}Img", "Quick"));
            ItemImg needItem;

            // 나에게 아이템이 존재하면
            if (itemIndex >= 0) 
                needItem = GameObject.Find(GameManager.GetGameObjectName($"{quest.needItem}Img", "Quick")).GetComponent<ItemImg>();
            // 존재하지 않는다면
            else
                needItem = GameObject.Find(GameManager.GetGameObjectName($"{quest.needItem}Img", "Shop")).GetComponent<ItemImg>();

            // 퀘스트 아이템을 다 구하지 못했다면
            if (needItem.count < quest.count) {
                nowDialogue = new string[] { $"{quest.name}\n{needItem.koreanName} : {needItem.count} / {quest.count}" };
            }
            // 퀘스트 아이템을 구했다면
            else
            {
                nowDialogue = gameManager.npcLists[npcIndex].sucDialogue;
                quest.isSuccess = true;
            }
        }       
        else
        {
            nowDialogue = gameManager.npcLists[npcIndex].dialogue;
        }

        dialougeTask = DialogueEvent(0);
        StartCoroutine(dialougeTask);
    }

    public IEnumerator DialogueEvent(int charIndex)
    {
        yield return new WaitForSeconds(0.1f);

        if (charIndex == 0) GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds[$"npcDialogue"], false);

        text_npcContent.text += nowDialogue[nowDialogueCnt][charIndex++];

        // 대사가 끝났다면
        if(charIndex >= nowDialogue[nowDialogueCnt].Length)
        {
            gameManager.audioSource.Stop();
            yield break;
        }

        dialougeTask = DialogueEvent(charIndex);
        StartCoroutine(dialougeTask);
    }

    public bool StopDialogue()
    {
        gameManager.audioSource.Stop();

        StopCoroutine(dialougeTask);
        nowDialogueCnt++;
        text_npcContent.text = string.Empty;

        if (nowDialogueCnt == nowDialogue.Length)
        {
            if (gameManager.npcLists[npcIndex].quest.isSuccess) // 퀘스트 완료
            {
                GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds[$"successQuest"], false);

                gameManager.npcLists[npcIndex].isAccept = false;
                gameManager.npcLists[npcIndex].quest.isSuccess = false;

                gameManager.SetItem(gameManager.quickSlot.items, $"{gameManager.npcLists[npcIndex].quest.needItem}Img", "Quick", -gameManager.npcLists[npcIndex].quest.count, false);
                gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);

                gameManager.SetMoney(gameManager.npcLists[npcIndex].quest.reward);
            }
            else if (!gameManager.npcLists[npcIndex].isAccept) // 퀘스트 받기
            {
                GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds[$"receiveQuest"], false);

                gameManager.npcLists[npcIndex].isAccept = true;

                gameManager.nowQuest.Add(gameManager.npcLists[npcIndex].quest);
            }

            gameManager.panel_npcDialogue.SetActive(false);

            return true;
        }

        return false;
    }

    public void CutTree(ItemImg nowQuickItem, Tree tree)
    {
        if (tree.animator == null) tree.SetInit();

        player.SetAnimation("isAxe", true);

        int fruitIndex = tree.StartHitEvent();

        if(fruitIndex > -1)
        {
            DropItem dropItem = null;

            if (fruitIndex < gameManager.resourcesManager.dropItem_fruit.Count) // 과일
            {
                dropItem = gameManager.objectManager.GetFruit(fruitIndex);
            }
            else if (fruitIndex == gameManager.resourcesManager.dropItem_fruit.Count) // 나뭇가지
            {
                dropItem = gameManager.objectManager.GetObj("TreeStick").GetComponent<DropItem>();
            }
            else if(fruitIndex == gameManager.resourcesManager.dropItem_fruit.Count + 1) // 돈
            {
                dropItem = gameManager.objectManager.GetObj("Coin").GetComponent<DropItem>();
            }

            Vector3 ranPos = new Vector2(UnityEngine.Random.Range(0, 2f), UnityEngine.Random.Range(-2f, -1f));
            dropItem.transform.parent.position = tree.transform.position + ranPos;

            if (dropItem.animator == null) dropItem.SetInit();
            dropItem.StartAppear();
        }

        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, true);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);
    }

    // ----------------- Groud Event ----------------- //

    public void InstallSoil(ItemImg nowQuickItem)
    {
        player.SetAnimation("isHoe", true);

        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);

        string originalName = nowQuickItem.originalName.Substring(0, nowQuickItem.originalName.Length - 9); // PrefebImg 빼기
        player.groundTile.SetTile(player.frontPos, gameManager.resourcesManager.tiles[originalName]);

        player.ShowAniEvent(player.installAni);
        player.installAni.transform.position = player.GetNowFrontPos();
    }

    public void InstallSeed(ItemImg nowQuickItem)
    {
        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);

        string originalName = nowQuickItem.originalName.Substring(0, nowQuickItem.originalName.Length - 7); // seedImg

        CreateSeed(originalName, 0, player.frontPos + new Vector3(0.5f, 0.5f));

        player.ShowAniEvent(player.installAni);
        player.installAni.transform.position = player.GetNowFrontPos();
    }

    public void FeedAnimal(ItemImg nowQuickItem, Animal animal)
    {
        Debug.Log("먹이줌");
        animal.ReceiveFeed();

        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);
    }

    public void InstallFence(ItemImg nowQuickItem)
    {
        string originalName = nowQuickItem.originalName.Substring(0, nowQuickItem.originalName.Length - 14); // BasicPrefebImg

        if (player.turnNum == 2 || player.turnNum == 3)
            originalName += "Side";
        else
            originalName += "Basic";

        player.obstacleTile.SetTile(player.frontPos, gameManager.resourcesManager.tiles[originalName]);

        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);

        player.ShowAniEvent(player.installAni);
        player.installAni.transform.position = player.GetNowFrontPos();
    }

    public void WithDrawTile(ItemImg nowQuickItem, Tilemap tilemap, string tileName)
    {
        player.SetAnimation("isHoe", true);

        // 땅
        if (tilemap.name.Equals("Ground"))
        {
            tilemap.SetTile(player.frontPos, gameManager.resourcesManager.tiles["Grass_Basic"]);
        }
        // 장애물
        else if (tilemap.name.Equals("Obstacle"))
        {
            tilemap.SetTile(player.frontPos, null);
        }

        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, true);
        PickupItem(tileName);
    }

    public void FillWatering(ItemImg nowQuickItem)
    {
        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);

        PickupItem("WateringcanFillImg");
    }

    public void PourWatering(ItemImg nowQuickItem)
    {
        player.SetAnimation("isWater", true);

        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, true);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);

        player.groundTile.SetTile(player.frontPos, gameManager.resourcesManager.tiles["Soil_Water"]);
    }

    public void GetMilk(ItemImg nowQuickItem)
    {
        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);

        PickupItem("MilkImg");
    }

    public void SpawnAniaml(ItemImg nowQuickItem)
    {
        gameManager.SetItem(gameManager.quickSlot.items, nowQuickItem.originalName, "Quick", -1, false);
        gameManager.InstallItemOnPanel(gameManager.quickSlot.items, gameManager.quickSlot.panel_items);

        string originalName = nowQuickItem.originalName.Substring(0, nowQuickItem.originalName.Length - 3); // img

        GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds[originalName.ToLower()], false);

        GameObject animal = Instantiate(gameManager.resourcesManager.animal[originalName]);
        animal.transform.position = player.frontPos;
    }

    public bool PreviewInstallBlock(ItemImg nowQuickItem, Vector3Int newFrontPos, bool newInstall)
    {
        // 아이템 바꿀 때는 무조건 초기화 해줌
        if (newInstall) player.previewTile.SetTile(player.frontPos, null);

        // 위치가 달라질 때만 미리보기 갱신
        if ((player.frontPos.x == newFrontPos.x && player.frontPos.y == newFrontPos.y) || nowQuickItem == null) {
            return false;
        }

        if (player.CanShowInstallPreview())
        {
            string originalName = nowQuickItem.originalName.Substring(0, nowQuickItem.originalName.Length - 9); //PrefebImg

            player.previewTile.SetTile(player.frontPos, null);
            player.previewTile.SetTile(newFrontPos, gameManager.resourcesManager.tiles[originalName]);
        }

        player.frontPos = newFrontPos;


        return false;
    }

}
