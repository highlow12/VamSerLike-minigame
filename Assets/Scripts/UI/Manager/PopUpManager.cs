using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    public GameObject popUpUi;

    void Start()
    {
        if (popUpUi == null)
        {
            Debug.LogError("PopUpUi is not assigned");
        }
        else
        {
            popUpUi.SetActive(true);
        }
    }

    void Update()
    {

    }
}
