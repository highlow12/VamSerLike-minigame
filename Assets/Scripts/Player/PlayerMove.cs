using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;


public class PlayerMove : MonoBehaviour
{
    public Vector3 inputVec { get; private set; }
    public float speed;
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
        transform.Translate(inputVec * Time.deltaTime);
    }

}
