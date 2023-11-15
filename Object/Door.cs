using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IObjectEvent
{
    Animator animator;

    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        SetInit();
    }

    public void SetInit()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAniState()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // πÆ¿Ã ø≠∏± ∂ß
        if (collision.CompareTag("Player"))
        {
            GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["openDoor"], false);
            DoorEvent(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // πÆ¿Ã ¥›»˙ ∂ß
        if (collision.CompareTag("Player"))
        {
            GameManager.PlaySound(gameManager.audioSource, gameManager.resourcesManager.sounds["closeDoor"], false);
            DoorEvent(collision);
        }
    }

    void DoorEvent(Collider2D player)
    {
        animator.SetBool("isOpen", !animator.GetBool("isOpen"));
        animator.SetBool("isClose", !animator.GetBool("isClose"));
    }
}
