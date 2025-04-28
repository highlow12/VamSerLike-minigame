using UnityEngine;

public class BasicMirrorEffect : MonoBehaviour
{
    public GameObject targetImage;
    
    void Start()
    {
        if (targetImage)
            targetImage.SetActive(false);
    }
    
    void OnMouseEnter()
    {
        if (targetImage)
            targetImage.SetActive(true);
    }
    
    void OnMouseExit()
    {
        if (targetImage)
            targetImage.SetActive(false);
    }
}
