using UnityEngine;
using System.Collections.Generic;

public class TrainCaptain : BossMonster
{
    // 이동 속도 및 공격 관련 변수
    public float walkSpeed = 2f;
    public float dashSpeed = 10f;
    public float attackRange = 3f;
    public GameObject flameTrailPrefab;
    public GameObject flameProjectilePrefab;

    public Animator animator{get; private set;}

    // 상태 열거형 (걷기, 공격)
    enum State {
        Walk = 0,
        Attack = 1
    }

    protected override void initState()
    {
        // Animator 초기화
        animator = GetComponent<Animator>();
        // 상태 초기값은 걷기(Walk)로 설정
        states = new Dictionary<int, BaseState>
        {
            { (int)State.Walk, new TrainCaptainWalk(this) },
            { (int)State.Attack, new TrainCaptainAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)State.Walk]);
    }

    protected override void checkeState()
    {
        // 플레이어와의 거리 기반 상태 전환
        float distance = Vector3.Distance(transform.position, GameManager.Instance.player.transform.position);
        if(distance > attackRange)
        {
            if(!(monsterFSM.currentState is TrainCaptainWalk))
            {
                monsterFSM.ChangeState(states[(int)State.Walk]);
            }
        }
        else
        {
            if(!(monsterFSM.currentState is TrainCaptainAttack))
            {
                monsterFSM.ChangeState(states[(int)State.Attack]);
            }
        }
    }
}

// 걷기 상태: 플레이어를 향해 천천히 이동하며 걷기 애니메이션 재생
class TrainCaptainWalk : BaseState
{
    public TrainCaptainWalk(Monster m) : base(m) { }
    
    public override void OnStateEnter()
    {
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null && tc.animator != null)
            tc.animator.SetBool("Walking", true); // 걷기 애니메이션 시작
    }
    
    public override void OnStateExit()
    {
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null && tc.animator != null)
            tc.animator.SetBool("Walking", false); // 걷기 애니메이션 종료
    }
    
    public override void OnStateUpdate()
    {
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null)
        {
            // 플레이어를 향해 이동
            Vector3 playerPos = GameManager.Instance.player.transform.position;
            Vector3 dir = (playerPos - monster.transform.position).normalized;
            monster.transform.position += dir * tc.walkSpeed * Time.deltaTime;
        }
    }
}

// 공격 상태: 플레이어와 가까워지면 돌진 및 불꽃 효과 실행
class TrainCaptainAttack : BaseState
{
    float flameTrailTimer = 0f;
    float projectileTimer = 0f;
    
    public TrainCaptainAttack(Monster m) : base(m) { }
    
    public override void OnStateEnter()
    {
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null && tc.animator != null)
            tc.animator.SetTrigger("Dash"); // 돌진 애니메이션 트리거
        // 타이머 초기화
        flameTrailTimer = 0f;
        projectileTimer = 0f;
    }
    
    public override void OnStateExit()
    {
        // 공격 상태 종료 시 추가 처리 필요시 작성
    }
    
    public override void OnStateUpdate()
    {
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null)
        {
            // 플레이어를 향해 빠르게 이동
            Vector3 playerPos = GameManager.Instance.player.transform.position;
            monster.transform.position = Vector3.MoveTowards(monster.transform.position, playerPos, tc.dashSpeed * Time.deltaTime);
        }
        
        // 불꽃 자국 생성 타이머 업데이트 (5초마다)
        float dt = Time.deltaTime;
        flameTrailTimer += dt;
        if(flameTrailTimer >= 5f)
        {
            SpawnFlameTrail();
            flameTrailTimer = 0f;
        }
        
        // 불꽃 투사체 발사 타이머 업데이트 (2초마다)
        projectileTimer += dt;
        if(projectileTimer >= 2f)
        {
            ShootFlameProjectile();
            projectileTimer = 0f;
        }
    }
    
    void SpawnFlameTrail()
    {
        // 현재 위치에 불꽃 자국 생성, 3초 후 소멸
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null && tc.flameTrailPrefab != null)
        {
            GameObject flameTrail = Object.Instantiate(tc.flameTrailPrefab, monster.transform.position, Quaternion.identity);
            Object.Destroy(flameTrail, 3f);
        }
    }
    
    void ShootFlameProjectile()
    {
        // 플레이어를 향해 불꽃 투사체 발사 (투사체는 충돌 시 플레이어 체력 감소 로직 포함)
        TrainCaptain tc = monster as TrainCaptain;
        if(tc != null && tc.flameProjectilePrefab != null)
        {
            Vector3 playerPos = GameManager.Instance.player.transform.position;
            Vector3 direction = (playerPos - monster.transform.position).normalized;
            Object.Instantiate(tc.flameProjectilePrefab, monster.transform.position, Quaternion.LookRotation(direction));
        }
    }
}

