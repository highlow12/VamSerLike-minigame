using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class cameraMove : MonoBehaviour
{
    Vector2 pos;
    Vector2 offset = Vector2.zero;
    float dis = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        pos = GameManager.Instance.player.transform.position; 
        transform.position = new(pos.x + offset.x*dis,pos.y + offset.y*dis, transform.position.z);
    }

    public void OnLook(InputValue value){
        var v = value.Get<Vector2>();
        v = Camera.main.ScreenToWorldPoint(v);
        var dir = v -(Vector2)transform.position;
        offset = dir.normalized;
    }
}
