using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    Player player;

    Vector3 playerPos = new Vector3(0, 0, -10);

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Update()
    {
        // 경계선 카메라 관리
        // x : 9, 22 y: -5, -14
        if (player.transform.position.x < Setting.OUTSIDE_MIN.x)
            playerPos.x = Setting.OUTSIDE_MIN.x;
        else if (player.transform.position.x > Setting.OUTSIDE_MAX.x)
            playerPos.x = Setting.OUTSIDE_MAX.x;
        else
            playerPos.x = player.transform.position.x;

        if (player.transform.position.y > Setting.OUTSIDE_MIN.y)
            playerPos.y = Setting.OUTSIDE_MIN.y;
        else if (player.transform.position.y < Setting.OUTSIDE_MAX.y)
            playerPos.y = Setting.OUTSIDE_MAX.y;
        else
            playerPos.y = player.transform.position.y;

        transform.position = playerPos;
    }
}
