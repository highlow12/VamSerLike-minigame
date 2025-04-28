using UnityEngine;

public class EnableOnHover : MonoBehaviour
{
    public GameObject target;
    
    void Start()
    {
        if (target != null)
            target.SetActive(false);
    }
    
    void OnMouseEnter()
    {
        if (target != null)
            target.SetActive(true);
    }
    
    void OnMouseExit()
    {
        if (target != null)
            target.SetActive(false);
    }
}
