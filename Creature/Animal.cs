using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Animal : MonoBehaviour
{
    // 내부 인스펙터
    public Animator animator;
    public BoxCollider2D boxCollider;

    // 외부 오브젝트
    GameManager gameManager;

    // 설정 값
    public int animalIndex;
    float idleInterval = 4;
    float moveInterval = 4;

    // 게임 진행 변수
    Vector3 dir;
    int meetingCnt;
    float flowIdleTime;
    float flowMoveTime;
    bool isMove;
    bool isExit;
    bool isFeed;
    bool canMating;
    Animal mating_1;
    Animal mating_2;

    public bool IsMove {
        get
        {
            return isMove;
        }
        set
        {
            isMove = value;
            animator.SetBool(nameof(isMove), value);
        }
    }

    public bool IsFeed
    {
        get
        {
            return isMove;
        }
        set
        {
            isFeed = value;
            animator.SetBool(nameof(isFeed), value);
        }
    }

    public bool CanMating
    {
        get
        {
            return canMating;
        }
        set
        {
            canMating = value;
            animator.SetBool("isMating", value);
        }
    }

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        SetInit();
    }

    void Update()
    {
        if (animator != null)
        {
            SetAniState();
            Move();
        }
    }

    public void SetInit()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void Move()
    {
        if (!canMating || !isFeed)
        {
            if (isMove) flowMoveTime += Time.deltaTime;
            else flowIdleTime += Time.deltaTime;

            if (flowIdleTime > idleInterval)
            {
                IsMove = true;

                dir = new Vector3(Random.Range(-1, 2), Random.Range(-1, 2));
                if (dir.x == 0) dir.x = 1;

                if (dir.x < 0) transform.eulerAngles = new Vector3(0, 180, 0);
                else transform.eulerAngles = new Vector3(0, 0, 0);

                flowIdleTime = 0;
            }

            if (isMove)
            {
                if (flowMoveTime > moveInterval)
                {
                    IsMove = false;

                    flowMoveTime = 0;
                }

                // 경계 밖으로 나간다면 반대로 튕겨냄
                if (transform.position.x < Setting.OUTSIDE_MIN.x || transform.position.x > Setting.OUTSIDE_MAX.x)
                {
                    dir.x *= -1;
                    isExit = true;
                }
                if (transform.position.y > Setting.OUTSIDE_MIN.y || transform.position.y < Setting.OUTSIDE_MAX.y)
                {
                    dir.y *= -1;
                    isExit = true;
                }

                if (transform.position.x >= Setting.OUTSIDE_MIN.x && transform.position.y <= Setting.OUTSIDE_MIN.y &&
                    transform.position.x <= Setting.OUTSIDE_MAX.x && transform.position.y >= Setting.OUTSIDE_MAX.y)
                {
                    isExit = false;
                }

                transform.position += new Vector3(1 * dir.x, 1 * dir.y, 0) * Time.deltaTime;
            }
        }
    }

    public void SetAniState()
    {
        if(GameManager.IsGetAniState(animator, "feed", 1.0f))
        {
            IsFeed = false;
        }
    }

    public void ReceiveFeed()
    {
        IsMove = false;

        // 성체일 때만 교배 카운트 증가
        if (transform.localScale.x >= Setting.ANIMAL_MAXGLOW) meetingCnt++;
        else transform.localScale += new Vector3(1, 1, 0);

        if (meetingCnt == 4)
        {
            CanMating = true;

            gameManager.canMeetingAnimal[animalIndex].Push(this);

            // 한쌍 이상의 교배 가능한 동물이 있다면 교배 시작
            if(gameManager.canMeetingAnimal[animalIndex].Count % 2 == 0)
            {
                mating_1 = gameManager.canMeetingAnimal[animalIndex].Pop();
                mating_2 = gameManager.canMeetingAnimal[animalIndex].Pop();

                mating_1.boxCollider.isTrigger = false;
                mating_2.boxCollider.isTrigger = false;

                StartMating();
            }
        }
        else
        {
            IsFeed = true;
        }
    }

    void StartMating()
    {
        // 선형보간으로 둘 사이 중간점 구하기
        Vector2 midPos = Vector2.Lerp(mating_1.transform.position, mating_2.transform.position, 0.5f);

        StartCoroutine(GoMidPos(midPos));
    }

    IEnumerator GoMidPos(Vector2 midPos)
    {
        yield return new WaitForSeconds(0.01f);

        mating_1.transform.position = Vector2.MoveTowards(mating_1.transform.position, midPos, 0.05f);
        mating_2.transform.position = Vector2.MoveTowards(mating_2.transform.position, midPos, 0.05f);

        if ((mating_1.transform.position.x >= mating_2.transform.position.x - 2 && mating_1.transform.position.x <= mating_2.transform.position.x + 2) && 
            (mating_1.transform.position.y >= mating_2.transform.position.y - 2 && mating_1.transform.position.y <= mating_2.transform.position.y + 2))
        {
            StartCoroutine(Breed());
        }
        else
        {
            StartCoroutine(GoMidPos(midPos));
        }
    }

    IEnumerator Breed()
    {
        yield return new WaitForSeconds(2f);
        mating_1.EndMating();
        mating_2.EndMating();

        GameObject babyAnimal = Instantiate(gameManager.resourcesManager.animal[$"{name.Substring(0, name.Length - 7)}"]);
        babyAnimal.transform.localScale = new Vector3(3, 3, 0);
        babyAnimal.transform.position = transform.position + (Vector3.down * 2);
    }

    public void EndMating()
    {
        CanMating = false;
        meetingCnt = 0;
        boxCollider.isTrigger = true;
    }
}
