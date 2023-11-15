using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    Animator houseRoof;

    private void Start()
    {
        houseRoof = GameObject.Find("Grid").transform.Find("HouseRoof").GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            houseRoof.SetBool("isOn", false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            houseRoof.SetBool("isOn", true);
        }
    }
}
