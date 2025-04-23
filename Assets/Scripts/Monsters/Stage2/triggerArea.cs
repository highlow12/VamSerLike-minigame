using Unity.VisualScripting;
using UnityEngine;

public class triggerArea : MonoBehaviour
{
    public bool triggered = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.TryGetComponent<PlayerMove>(out var p))
        {
            triggered = true;
            GameManager.Instance.player.SpeedDebuff = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.TryGetComponent<PlayerMove>(out var p))
        {
            triggered = false;
            GameManager.Instance.player.SpeedDebuff = false;
        }
    }

}
