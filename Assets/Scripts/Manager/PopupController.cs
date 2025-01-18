using UnityEngine;

public class PopupController : MonoBehaviour
{
    public GameObject popupPanel; // �˾� �г�

    // �˾� ����
    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    // �˾� �ݱ�
    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}
