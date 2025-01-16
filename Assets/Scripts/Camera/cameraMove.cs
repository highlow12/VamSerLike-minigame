using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class cameraMove : MonoBehaviour
{
    Vector2 pos;
    Vector2 offset = Vector2.zero;
    [SerializeField] float offsetMult = 1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        pos = GameManager.Instance.player.transform.position;
        offset = GameManager.Instance.player.inputVec.normalized * offsetMult;
        transform.position = new(pos.x + offset.x, pos.y + offset.y, transform.position.z);
    }
}
