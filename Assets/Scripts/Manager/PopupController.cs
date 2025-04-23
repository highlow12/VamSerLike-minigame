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
        public bool showOnStart = false; // 시작 시 표시 여부
        public bool neverClose = false;  // 특정 팝업이 닫히지 않도록 보호
    }

    public List<PopupData> popupPanels = new List<PopupData>();
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Dictionary<string, bool> popupStates = new Dictionary<string, bool>();

    // StartPanel 참조를 명시적으로 추가
    [SerializeField] private GameObject startPanel;

    // 디버깅을 위한 로그 옵션
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        // Awake에서 기본 컴포넌트 초기화
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        // StartPanel 참조가 없다면 자동으로 찾기
        if (startPanel == null)
        {
            // 먼저 팝업 목록에서 StartPanel 이름으로 된 항목 찾기
            var startPanelData = popupPanels.Find(x => x.popupName == "StartPanel");
            if (startPanelData != null && startPanelData.popupPanel != null)
            {
                startPanel = startPanelData.popupPanel;
                // StartPanel은 닫히지 않도록 설정
                startPanelData.neverClose = true;
                if (showDebugLogs) Debug.Log("StartPanel을 팝업 목록에서 찾아 설정했습니다.");
            }
            else
            {
                // 씬에서 바로 찾기
                startPanel = GameObject.Find("StartPanel");
                if (startPanel != null && showDebugLogs)
                    Debug.Log("StartPanel을 씬에서 찾아 설정했습니다.");
            }
        }
    }

    private void Start()
    {
        // 모든 팝업 초기화 - StartPanel 제외
        foreach (var popup in popupPanels)
        {
            if (popup != null && popup.popupPanel != null && !string.IsNullOrEmpty(popup.popupName))
            {
                // StartPanel은 항상 활성화, 다른 팝업은 비활성화
                bool isStartPanel = (popup.popupName == "StartPanel" || popup.popupPanel == startPanel);

                if (!isStartPanel)
                {
                    popup.popupPanel.SetActive(false);
                    popupStates[popup.popupName] = false;
                }
                else
                {
                    // StartPanel은 특별 처리
                    popup.popupPanel.SetActive(true);
                    popupStates[popup.popupName] = true;
                    //popup.neverClose = true; // StartPanel은 닫히지 않도록 설정

                    if (showDebugLogs) Debug.Log("StartPanel이 활성화되었습니다.");
                }
            }
            else if (popup != null)
            {
                Debug.LogWarning("유효하지 않은 팝업 설정: " + (popup.popupName ?? "이름 없음"));
            }
        }
    }

    public void ShowPopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // 이미 활성화된 경우 무시
            if (popupStates.ContainsKey(popupName) && popupStates[popupName])
                return;

            // StartPanel만 특별 처리하고, 다른 팝업이 열릴 때 StartPanel을 닫지 않습니다
            if (popupName != "StartPanel")
            {
                // StartPanel을 제외한 다른 팝업만 닫기
                CloseAllPopupsExcept("StartPanel");
            }

            popup.popupPanel.SetActive(true);
            popupStates[popupName] = true;

            if (showDebugLogs) Debug.Log($"팝업 열림: {popupName}");
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning($"찾을 수 없는 팝업: {popupName}");
        }
    }

    public void ClosePopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // neverClose 플래그가 true인 팝업은 닫히지 않도록 보호
            if (popup.neverClose)
            {
                if (showDebugLogs) Debug.Log($"{popupName}은(는) neverClose 설정으로 인해 닫히지 않습니다.");
                return;
            }

            // 상태 확인
            if (!popupStates.ContainsKey(popupName) || !popupStates[popupName])
                return;

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

                if (showDebugLogs) Debug.Log($"팝업 닫힘: {popupName}");
            }
        }
    }

    public void ForceClosePopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // neverClose 플래그가 true인 팝업은 강제로도 닫히지 않도록 보호
            if (popup.neverClose)
            {
                if (showDebugLogs) Debug.Log($"{popupName}은(는) neverClose 설정으로 인해 강제로도 닫히지 않습니다.");
                return;
            }

            popup.popupPanel.SetActive(false);
            popupStates[popupName] = false;

            if (showDebugLogs) Debug.Log($"팝업 강제 닫힘: {popupName}");
        }
    }

    public void CloseAllPopups()
    {
        foreach (var popup in popupPanels)
        {
            if (popup.popupPanel != null && !popup.neverClose)  // neverClose 팝업은 닫지 않음
            {
                popup.popupPanel.SetActive(false);
                popupStates[popup.popupName] = false;
            }
        }

        if (showDebugLogs) Debug.Log("모든 팝업이 닫혔습니다 (neverClose 팝업 제외)");
    }

    public void CloseAllPopupsExcept(string exceptPopupName)
    {
        foreach (var popup in popupPanels)
        {
            if (popup.popupPanel != null && popup.popupName != exceptPopupName && !popup.neverClose)
            {
                popup.popupPanel.SetActive(false);
                popupStates[popup.popupName] = false;
            }
        }

        if (showDebugLogs) Debug.Log($"모든 팝업이 닫혔습니다 ({exceptPopupName} 및 neverClose 팝업 제외)");
    }

    // 특정 팝업이 열려있는지 확인
    public bool IsPopupOpen(string popupName)
    {
        return popupStates.ContainsKey(popupName) && popupStates[popupName];
    }

    // 런타임에 팝업 추가
    public void AddPopup(string popupName, GameObject popupPanel, bool neverClose = false)
    {
        if (string.IsNullOrEmpty(popupName) || popupPanel == null)
            return;

        // 이미 존재하는 팝업 확인
        if (popupPanels.Exists(x => x.popupName == popupName))
        {
            if (showDebugLogs) Debug.LogWarning($"이미 존재하는 팝업 이름: {popupName}");
            return;
        }

        PopupData newPopup = new PopupData
        {
            popupName = popupName,
            popupPanel = popupPanel,
            neverClose = neverClose
        };

        popupPanels.Add(newPopup);
        popupStates[popupName] = popupPanel.activeSelf;

        if (showDebugLogs) Debug.Log($"런타임에 팝업 추가됨: {popupName}");
    }
}