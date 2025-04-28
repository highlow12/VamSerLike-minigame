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
        public bool showOnStart = false; // ���� �� ǥ�� ����
        public bool neverClose = false;  // Ư�� �˾��� ������ �ʵ��� ��ȣ
    }

    public List<PopupData> popupPanels = new List<PopupData>();
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Dictionary<string, bool> popupStates = new Dictionary<string, bool>();

    // StartPanel ������ ��������� �߰�
    [SerializeField] private GameObject startPanel;

    // ������� ���� �α� �ɼ�
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        // Awake���� �⺻ ������Ʈ �ʱ�ȭ
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        // StartPanel ������ ���ٸ� �ڵ����� ã��
        if (startPanel == null)
        {
            // ���� �˾� ��Ͽ��� StartPanel �̸����� �� �׸� ã��
            var startPanelData = popupPanels.Find(x => x.popupName == "StartPanel");
            if (startPanelData != null && startPanelData.popupPanel != null)
            {
                startPanel = startPanelData.popupPanel;
                // StartPanel�� ������ �ʵ��� ����
                startPanelData.neverClose = true;
                if (showDebugLogs) Debug.Log("StartPanel�� �˾� ��Ͽ��� ã�� �����߽��ϴ�.");
            }
            else
            {
                // ������ �ٷ� ã��
                startPanel = GameObject.Find("StartPanel");
                if (startPanel != null && showDebugLogs)
                    Debug.Log("StartPanel�� ������ ã�� �����߽��ϴ�.");
            }
        }
    }

    private void Start()
    {
        // ��� �˾� �ʱ�ȭ - StartPanel ����
        foreach (var popup in popupPanels)
        {
            if (popup != null && popup.popupPanel != null && !string.IsNullOrEmpty(popup.popupName))
            {
                // StartPanel�� �׻� Ȱ��ȭ, �ٸ� �˾��� ��Ȱ��ȭ
                bool isStartPanel = (popup.popupName == "StartPanel" || popup.popupPanel == startPanel);

                if (!isStartPanel)
                {
                    popup.popupPanel.SetActive(false);
                    popupStates[popup.popupName] = false;
                }
                else
                {
                    // StartPanel�� Ư�� ó��
                    popup.popupPanel.SetActive(true);
                    popupStates[popup.popupName] = true;
                    //popup.neverClose = true; // StartPanel�� ������ �ʵ��� ����

                    if (showDebugLogs) Debug.Log("StartPanel�� Ȱ��ȭ�Ǿ����ϴ�.");
                }
            }
            else if (popup != null)
            {
                Debug.LogWarning("��ȿ���� ���� �˾� ����: " + (popup.popupName ?? "�̸� ����"));
            }
        }
    }

    public void ShowPopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // �̹� Ȱ��ȭ�� ��� ����
            if (popupStates.ContainsKey(popupName) && popupStates[popupName])
                return;

            // StartPanel�� Ư�� ó���ϰ�, �ٸ� �˾��� ���� �� StartPanel�� ���� �ʽ��ϴ�
            if (popupName != "StartPanel")
            {
                // StartPanel�� ������ �ٸ� �˾��� �ݱ�
                CloseAllPopupsExcept("StartPanel");
            }

            popup.popupPanel.SetActive(true);
            popupStates[popupName] = true;

            if (showDebugLogs) Debug.Log($"�˾� ����: {popupName}");
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning($"ã�� �� ���� �˾�: {popupName}");
        }
    }

    public void ClosePopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // neverClose �÷��װ� true�� �˾��� ������ �ʵ��� ��ȣ
            if (popup.neverClose)
            {
                if (showDebugLogs) Debug.Log($"{popupName}��(��) neverClose �������� ���� ������ �ʽ��ϴ�.");
                return;
            }

            // ���� Ȯ��
            if (!popupStates.ContainsKey(popupName) || !popupStates[popupName])
                return;

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

                if (showDebugLogs) Debug.Log($"�˾� ����: {popupName}");
            }
        }
    }

    public void ForceClosePopup(string popupName)
    {
        PopupData popup = popupPanels.Find(x => x.popupName == popupName);
        if (popup != null && popup.popupPanel != null)
        {
            // neverClose �÷��װ� true�� �˾��� �����ε� ������ �ʵ��� ��ȣ
            if (popup.neverClose)
            {
                if (showDebugLogs) Debug.Log($"{popupName}��(��) neverClose �������� ���� �����ε� ������ �ʽ��ϴ�.");
                return;
            }

            popup.popupPanel.SetActive(false);
            popupStates[popupName] = false;

            if (showDebugLogs) Debug.Log($"�˾� ���� ����: {popupName}");
        }
    }

    public void CloseAllPopups()
    {
        foreach (var popup in popupPanels)
        {
            if (popup.popupPanel != null && !popup.neverClose)  // neverClose �˾��� ���� ����
            {
                popup.popupPanel.SetActive(false);
                popupStates[popup.popupName] = false;
            }
        }

        if (showDebugLogs) Debug.Log("��� �˾��� �������ϴ� (neverClose �˾� ����)");
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

        if (showDebugLogs) Debug.Log($"��� �˾��� �������ϴ� ({exceptPopupName} �� neverClose �˾� ����)");
    }

    // Ư�� �˾��� �����ִ��� Ȯ��
    public bool IsPopupOpen(string popupName)
    {
        return popupStates.ContainsKey(popupName) && popupStates[popupName];
    }

    // ��Ÿ�ӿ� �˾� �߰�
    public void AddPopup(string popupName, GameObject popupPanel, bool neverClose = false)
    {
        if (string.IsNullOrEmpty(popupName) || popupPanel == null)
            return;

        // �̹� �����ϴ� �˾� Ȯ��
        if (popupPanels.Exists(x => x.popupName == popupName))
        {
            if (showDebugLogs) Debug.LogWarning($"�̹� �����ϴ� �˾� �̸�: {popupName}");
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

        if (showDebugLogs) Debug.Log($"��Ÿ�ӿ� �˾� �߰���: {popupName}");
    }
}