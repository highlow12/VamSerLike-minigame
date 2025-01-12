using System.Collections.Generic;
using UnityEngine;

public class Teacher : BossMonster
{
    public Transform Head;
    public Transform Neck;
    public float HeadSpeed = 1.0f;
    public int NeckMinimumLenght = 30;
    public int NeckMaximumLenght = 50;
    public int NeckRandomLenght{get; private set;}

    protected override void checkeState()
    {
        //throw new System.NotImplementedException();
    }

    protected override void initState()
    {
        NeckRandomLenght = NeckMaximumLenght - NeckMinimumLenght;
        states = new Dictionary<int, BaseState>
        {
            {1,new MoveHeadState(this)} // it has one state
        };
        monsterFSM = new MonsterFSM(states[1]);
    }
}
class MoveHeadState : BaseState
{
    new Teacher monster;
    int state = 0;
    float timer = 0.0f;
    float time = 0.0f;
    Transform newNeck;
    public MoveHeadState(Teacher monster) : base(monster)
    {
        this.monster = monster;
        newNeck = monster.Neck;
    }

    public override void OnStateEnter()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnStateExit()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnStateUpdate()
    {
        if (state == 0) // Ready to attack
        {
            time = (monster.NeckMinimumLenght + Random.Range(0, monster.NeckRandomLenght)) / monster.HeadSpeed;

            Vector3 targetDir = GameManager.Instance.player.transform.position - monster.Head.position;
            targetDir = new Vector3(0, 0, Vector3.SignedAngle(Vector3.right, targetDir, Vector3.forward));

            // Make the head look at the player
            monster.Head.localRotation = Quaternion.Euler(targetDir);

            // Make the neck look at the player
            newNeck.localScale = new(0.1f,0.1f,0.1f);
            newNeck.localRotation = Quaternion.Euler(targetDir);

            state = 1;
        }
        else // Move the head to the player
        {
            // HeadMovement
            monster.Head.position += monster.Head.right * monster.HeadSpeed * Time.deltaTime;

            // NeckMovement
            newNeck.position += monster.Head.right * monster.HeadSpeed/2 * Time.deltaTime;
            newNeck.localScale = new Vector3(monster.HeadSpeed * timer, newNeck.localScale.y, newNeck.localScale.z);


            timer += Time.deltaTime;
            if (timer >= time)
            {
                state = 0;
                timer = 0.0f;
                newNeck = Object.Instantiate(monster.Neck, monster.Head.position, monster.Neck.rotation, monster.transform);
            }
        }
        
    }
}
