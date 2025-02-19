using UnityEngine;
using System.Collections.Generic;

public class Mermaid : BossMonster
{
    public float moveSpeed;
    public float ChargeSpeed;
    [SerializeField] float ChargeTime = 4f;
    float timer = 0;
    public Animator animator{get; private set;}
    enum MermaidAction
    {
        Chase = 0,
        Attack = 1
    }
    protected override void checkeState()
    {
        timer += Time.deltaTime;
        // 계속 플레이어를 따라다니다 공격 범위안에 들어오면 공격 또는 일정 시간이 지나면 공격
        if ( timer > ChargeTime)
        {
            monsterFSM.ChangeState(states[(int)MermaidAction.Attack]);
            timer = 0;
        }
        else
        {
            //monsterFSM.ChangeState(states[(int)MermaidAction.Chase]);
        }
    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();

        states = new Dictionary<int, BaseState>
        {
            { (int)MermaidAction.Chase, new MermaidChase(this) },
            { (int)MermaidAction.Attack, new MermaidAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)MermaidAction.Chase]);
    }

    public void ChageState_Chase(){
        monsterFSM.ChangeState(states[(int)MermaidAction.Chase]);
    }
}

class MermaidChase : BaseState
{
    // Change targetPosition to Vector2 for 2D movement on xy plane.
    Vector2 targetPosition;
    float radius = 5f;

    public MermaidChase(Monster m) : base(m) { }

    public override void OnStateEnter()
    {
        // Use only the xy components of the player's position
        targetPosition = Random.insideUnitCircle * radius + (Vector2)GameManager.Instance.player.transform.position;
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        if(Vector2.Distance(monster.transform.position, targetPosition) < 0.1f)
        {
            targetPosition = Random.insideUnitCircle * radius + (Vector2)GameManager.Instance.player.transform.position;
        }
        monster.transform.position += ((Vector3)(targetPosition - (Vector2)monster.transform.position)).normalized * ((Mermaid)monster).moveSpeed * Time.deltaTime;
    }
}

class MermaidAttack : BaseState
{
    Vector2 targetdir = Vector2.zero;
    public MermaidAttack(Monster m) : base(m) { }

    public override void OnStateEnter()
    {
        targetdir = GameManager.Instance.player.transform.position - monster.transform.position;
        ((Mermaid)monster).animator.SetTrigger("Attack");
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        monster.transform.position += (Vector3)targetdir.normalized * (((Mermaid)monster).ChargeSpeed * Time.deltaTime);
    }
}



