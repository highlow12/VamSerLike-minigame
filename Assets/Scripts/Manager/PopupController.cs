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

        // 모든 팝업 초기화
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
            // 다른 열린 팝업들 모두 닫기
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
            // 현재 마우스 포인터 아래에 있는 UI 요소 확인
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

    // 특정 팝업이 열려있는지 확인
    public bool IsPopupOpen(string popupName)
    {
        return popupStates.ContainsKey(popupName) && popupStates[popupName];
    }
}