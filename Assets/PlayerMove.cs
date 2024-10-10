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
        
    }
    
    void Update(){
        transform.Translate(inputVec * Time.deltaTime);
    }
    
}
