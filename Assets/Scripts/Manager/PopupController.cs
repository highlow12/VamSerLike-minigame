using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class PopupController : MonoBehaviour
{
    [System.Serializable]
    public class PopupData
    {
        public string popupName;
        public GameObject popupPanel;
        public bool showOnStart = false; // 시작 시 열지 여부
        public bool neverClose = false;  // 특정 팝업이 닫히지 않도록 보호
    }

    public List<PopupData> popupPanels = new List<PopupData>();
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Dictionary<string, bool> popupStates = new Dictionary<string, bool>();

    // StartPanel 참조를 명시적으로 추가
    [SerializeField] private GameObject startPanel;

    // 디버그를 위한 로그 옵션
    [SerializeField] private bool showDebugLogs = true;

    // 다른 팝업을 열 때 현재 열린 팝업을 자동으로 닫을지 여부
    [SerializeField] private bool closeOthersWhenOpening = false;

    // 부모 Canvas 참조 추가
    private Canvas parentCanvas;

    private void Awake()
    {
        // Awake에서 기본 컴포넌트 초기화
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        // 부모 Canvas 찾기
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogWarning("PopupController에 부모 Canvas가 없습니다!");
        }

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
                    popup.popupPanel.SetActive(popup.showOnStart); // showOnStart 플래그에 따라 활성화
                    popupStates[popup.popupName] = popup.showOnStart;

                    if (popup.showOnStart && showDebugLogs)
                        Debug.Log($"팝업 '{popup.popupName}'이 showOnStart 설정으로 인해 시작 시 활성화되었습니다.");
                }
                else
                {
                    // StartPanel은 특별 처리
                    popup.popupPanel.SetActive(true);
                    popupStates[popup.popupName] = true;

                    if (showDebugLogs) Debug.Log("StartPanel이 활성화되었습니다.");
                }
            }
            else if (popup != null)
            {
                Debug.LogWarning("유효하지 않은 팝업 설정: " + (popup.popupName ?? "이름 없음"));
            }
        }

        // 초기화 완료 후 모든 팝업 상태 로깅
        if (showDebugLogs) LogAllPopupStates();
    }

    // 모든 팝업의 상태를 로그로 출력하는 디버그 함수
    private void LogAllPopupStates()
    {
        Debug.Log("===== 모든 팝업 상태 =====");
        foreach (var popup in popupPanels)
        {
            if (popup != null && popup.popupPanel != null)
            {
                string positionInfo = "";
                string sizeInfo = "";

                RectTransform rt = popup.popupPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    positionInfo = rt.anchoredPosition.ToString();
                    sizeInfo = rt.sizeDelta.ToString();
                }

                Debug.Log($"팝업: {popup.popupName}, 활성화: {popup.popupPanel.activeSelf}, " +
                          $"neverClose: {popup.neverClose}, showOnStart: {popup.showOnStart}, " +
                          $"위치: {positionInfo}, 크기: {sizeInfo}");

                // Canvas 컴포넌트가 있는지 확인
                Canvas panelCanvas = popup.popupPanel.GetComponent<Canvas>();
                if (panelCanvas != null)
                {
                    Debug.Log($"  - Canvas 정보: RenderMode={panelCanvas.renderMode}, SortOrder={panelCanvas.sortingOrder}");
                }
            }
        }
        Debug.Log("=========================");
    }

    // UI 계층 구조를 확인하는 디버그 함수
    private void CheckUIHierarchy(GameObject panel, string prefix = "")
    {
        if (!showDebugLogs) return;

        Debug.Log($"{prefix}Panel: {panel.name}, 활성화: {panel.activeSelf}");

        // Canvas 컴포넌트 확인
        Canvas canvas = panel.GetComponent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"{prefix}- Canvas: RenderMode={canvas.renderMode}, SortOrder={canvas.sortingOrder}");
        }

        // CanvasGroup 컴포넌트 확인
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            Debug.Log($"{prefix}- CanvasGroup: Alpha={canvasGroup.alpha}, Interactable={canvasGroup.interactable}, BlocksRaycasts={canvasGroup.blocksRaycasts}");
        }

        foreach (Transform child in panel.transform)
        {
            string positionInfo = "";
            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt != null)
            {
                positionInfo = rt.anchoredPosition.ToString();
            }

            Debug.Log($"{prefix}- Child: {child.name}, 활성화: {child.gameObject.activeSelf}, 위치: {positionInfo}");

            // 재귀적으로 자식들도 확인
            if (child.childCount > 0)
                CheckUIHierarchy(child.gameObject, prefix + "  ");
        }
    }

    public void ShowPopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // 이미 활성화된 경우 무시
            if (popupStates.ContainsKey(popupName) && popupStates[popupName])
            {
                if (showDebugLogs) Debug.Log($"팝업 '{popupName}'은 이미 활성화 상태입니다.");
                return;
            }

            // 다른 팝업을 닫을지 여부 체크
            if (closeOthersWhenOpening)
            {
                // StartPanel만 특별 처리하고, 다른 팝업이 열릴 때 StartPanel을 닫지 않습니다
                if (popupName != "StartPanel")
                {
                    // StartPanel을 제외한 다른 팝업만 닫기
                    CloseAllPopupsExcept("StartPanel");
                }
            }

            // 중요: 부모 Canvas가 비활성화되어 있다면 활성화
            if (parentCanvas != null && !parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
                Debug.Log("부모 Canvas가 비활성화 상태여서 활성화했습니다.");
            }

            // 팝업 활성화 전 Canvas 확인
            Canvas popupCanvas = popup.popupPanel.GetComponent<Canvas>();
            if (popupCanvas != null)
            {
                if (showDebugLogs) Debug.Log($"팝업 '{popupName}'에 Canvas 컴포넌트가 있습니다. RenderMode: {popupCanvas.renderMode}, SortOrder: {popupCanvas.sortingOrder}");

                // Canvas가 있다면 확인: StagePanel이 활성화되어도 Canvas가 꺼지지 않도록
                popupCanvas.enabled = true;
            }

            // 팝업 내부에 다른 Canvas가 있는지 확인
            Canvas[] childCanvases = popup.popupPanel.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas childCanvas in childCanvases)
            {
                if (childCanvas != popupCanvas)
                {
                    childCanvas.enabled = true;
                    if (showDebugLogs) Debug.Log($"팝업 '{popupName}'의 자식 Canvas를 활성화: {childCanvas.gameObject.name}");
                }
            }

            // 팝업 활성화
            popup.popupPanel.SetActive(true);
            popupStates[popupName] = true;

            // 팝업 활성화 후 다시 Canvas 상태 확인
            if (popupCanvas != null && !popupCanvas.enabled)
            {
                popupCanvas.enabled = true;
                Debug.Log($"팝업 '{popupName}'의 Canvas가 비활성화되어 다시 활성화했습니다.");
            }

            // 디버그 정보 추가 - 팝업 활성화 시 계층 구조 체크
            if (showDebugLogs)
            {
                Debug.Log($"팝업 열림: {popupName} - Panel: {popup.popupPanel.name}, 활성화 상태: {popup.popupPanel.activeSelf}");
                CheckUIHierarchy(popup.popupPanel);
            }
        }
        else
        {
            // 등록된 모든 팝업 이름 출력하여 디버깅 돕기
            if (showDebugLogs)
            {
                string registeredPopups = string.Join(", ", popupPanels.Select(p => p.popupName).ToArray());
                Debug.LogWarning($"찾을 수 없는 팝업: {popupName} - 등록된 팝업 목록: {registeredPopups}");
            }
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