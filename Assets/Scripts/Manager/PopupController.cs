using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PopupController : MonoBehaviour
{
    [System.Serializable]
    public class PopupData
    {
        public string popupName;
        public GameObject popupPanel;
    }

    public List<PopupData> popupPanels = new List<PopupData>();
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Dictionary<string, bool> popupStates = new Dictionary<string, bool>();

    private void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        // ��� �˾� �ʱ�ȭ
        foreach (var popup in popupPanels)
        {
            if (popup.popupPanel != null)
            {
                popup.popupPanel.SetActive(false);
                popupStates[popup.popupName] = false;
            }
        }
    }

    public void ShowPopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null && !popupStates[popupName])
        {
            // �ٸ� ���� �˾��� ��� �ݱ�
            CloseAllPopups();

            popup.popupPanel.SetActive(true);
            popupStates[popupName] = true;
        }
    }

    public void ClosePopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null && popupStates[popupName])
        {
            // ���� ���콺 ������ �Ʒ��� �ִ� UI ��� Ȯ��
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();

            raycaster.Raycast(pointerEventData, results);

            bool clickedOnPopup = false;
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.transform.IsChildOf(popup.popupPanel.transform))
                {
                    clickedOnPopup = true;
                    break;
                }
            }

            if (!clickedOnPopup)
            {
                popup.popupPanel.SetActive(false);
                popupStates[popupName] = false;
            }
        }
    }

    public void ForceClosePopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            popup.popupPanel.SetActive(false);
            popupStates[popupName] = false;
        }
    }

    public void CloseAllPopups()
    {
        foreach (var popup in popupPanels)
        {
            if (popup.popupPanel != null)
            {
                popup.popupPanel.SetActive(false);
                popupStates[popup.popupName] = false;
            }
        }
    }

    // Ư�� �˾��� �����ִ��� Ȯ��
    public bool IsPopupOpen(string popupName)
    {
        return popupStates.ContainsKey(popupName) && popupStates[popupName];
    }
}