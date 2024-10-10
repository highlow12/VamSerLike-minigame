using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class PlayerMove : MonoBehaviour
{
    public Vector3 inputVec {get; private set;}
    public float speed;
    public void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
        //Move();
    }
    void Move(Vector2 speed){
        transform.Translate(speed);
        inputVec = speed.normalized;
    }
    public void Update(){
        Move(Vector2.left * Time.deltaTime * speed);
    }
    
}
