using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class PlayerMove : MonoBehaviour
{
    public Vector3 inputVec { get; private set; }
    public float speed;
    public float speedDebuffReduce = 0.8f; //속도 느리게 하는 디버프 걸렸을때 줄어드는 속도의 양
    public bool SpeedDebuff = false;
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
    }

    public float CheckDebuff()
    {
        //Debug.Log(SpeedDebuff);
        float returnvar = 1;
        returnvar *= SpeedDebuff ? speedDebuffReduce : 1;
        SpeedDebuff = false; // reset check var
        return returnvar;
    }

}
