using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class PlayerMove : MonoBehaviour
{
    public Animator animator;
    public Vector3 inputVec { get; private set; }
    public float moveSpeed;
    public float speed;
    public float speedDebuffReduce = 0.8f; //속도 느리게 하는 디버프 걸렸을때 줄어드는 속도의 양
    public bool SpeedDebuff = false;

    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }
    public void OnMoveWithVirtualJoystick(Vector2 value)
    {
        inputVec = value;
    }

    void Update()
    {
        transform.Translate(inputVec * Time.deltaTime * CheckDebuff());
        moveSpeed = inputVec.magnitude;
        if (moveSpeed > 0)
        {
            animator.SetInteger("WalkState", 1);
            animator.SetFloat("WalkAnimSpeed", Mathf.Min(1, moveSpeed));
        }
        else
        {
            animator.SetInteger("WalkState", 0);
        }
    }

    public float CheckDebuff()
    {
        Debug.Log(SpeedDebuff);
        float returnvar = 1;
        returnvar *= SpeedDebuff ? speedDebuffReduce : 1;
        SpeedDebuff = false; // reset check var
        return returnvar;
    }

}
