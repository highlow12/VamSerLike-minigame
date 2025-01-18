using UnityEngine;

public class PopupController : MonoBehaviour
{
    public GameObject popupPanel; // ÆË¾÷ ÆÐ³Î

    // ÆË¾÷ ¿­±â
    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    // ÆË¾÷ ´Ý±â
    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}
