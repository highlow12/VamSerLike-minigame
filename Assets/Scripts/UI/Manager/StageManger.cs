using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageManager : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject stageSelectScreen;

    public Button enterButton;
    public Button stageSelectButton;
    public Button selectStageButton;
    public Button leftArrowButton;
    public Button rightArrowButton;

    public GameObject[] portalImages; // ���� �̹��� (�̹� �ִٰ� �ϼ����� Ȱ��)
    public TextMeshProUGUI stageTitle;
    public TextMeshProUGUI stageDescription;
    public TextMeshProUGUI stageDifficulty;

    private int currentStageIndex = 0;
    private int clearedStageIndex = 0; // ���� �ֱٿ� Ŭ������ �������� �ε���

    // �������� ������ �ϵ��ڵ�
    private string[] stageTitles = {
        "1. ������ ����ȸ��",
        "2. �ͽŵ鸰 �б�",
        "3. ������ ����",
        "4. ������ �׸���",
        "5. ������ ��ö",
        "6. ������ ȣ��",
        "7. ���� â��",
        "8. ���� ���� ��",
        "9. ������ �ڵ��� ����",
        "10. ���� �帣�� ���"
    };

    private string[] stageDescriptions = {
        "�Ѷ� ȭ���ߴ� ����ȸ�忡 ����Ը��� ���Ҵ�. ���� ���鸮�� �ֺ�, ���� �Ǹ� ������ ��������.",
        "�ͼ������� ���� ���� �ӿ��� �̻��ϰ� ��Ʋ�� ���� �б��� �����Ѵ�.",
        "���� ���밡 ������ ��⹰, �������� ����� ��� �� ������ �ʴ� �ü����� ���� ���Ѻ��� �ִ�.",
        "����ϰ� �� ������ ����, �Ʒ��� ����������� £�� ����� ���� ��Ų��.",
        "�޸��� ��ö ��, �������� ������ �ʴ´�. �������� �Ҿ���� ä ������ ���� �ӿ��� ���� ���ؾ� �� ���ΰ�.",
        "�߰��ϴ� ���͵��� ���� ������ ȣ���� �ǳʾ� �Ѵ�.",
        "ģ���� �Բ� ���ٲ����� �ϸ� ��Ҵ� ���. �׷��� �̰��� �� �̻� ��ſ� ������ ������ �ƴϴ�.",
        "�����ϰ� ���� ���� �� �̷ο��� �������� ���� Ż���ض�.",
        "�������� ���� �տ� ��Ÿ�� �Ҿ���� ����� ã�ƾ� �Ѵ�.",
        "��ħ�� ã�Ե� ģ��, �׸��� ���ſ� �����ߴ� ��� ������� �ٽ� ��Ÿ�� ���� ���´�. ���� �Ʒ� �������� ������ ����, ���ΰ��� �ڽ��� �Ѿƿ� ������ �޾Ƶ��� ���ΰ�, �ƴϸ� �ٽ� ���� ���� ���ΰ�?"
    };

    private string[] stageDifficulties = {
        "���̵�: �ڡ١١١�",
        "���̵�: �ڡ١١١�",
        "���̵�: �ڡڡ١١�",
        "���̵�: �ڡڡ١١�",
        "���̵�: �ڡڡڡ١�",
        "���̵�: �ڡڡڡ١�",
        "���̵�: �ڡڡڡڡ�",
        "���̵�: �ڡڡڡڡ�",
        "���̵�: �ڡڡڡڡ�",
        "���̵�: �ڡڡڡڡ�"
    };

    void Start()
    {
        // PlayerPrefs�κ��� Ŭ���� ���� �ε�
        clearedStageIndex = PlayerPrefs.GetInt("ClearedStageIndex", 0);

        // ��ư �̺�Ʈ ����
        enterButton.onClick.AddListener(EnterStage);
        stageSelectButton.onClick.AddListener(OpenStageSelect);
        selectStageButton.onClick.AddListener(ConfirmStageSelection);
        leftArrowButton.onClick.AddListener(NavigateToPreviousStage);
        rightArrowButton.onClick.AddListener(NavigateToNextStage);

        // �ʱ� ȭ�� ����
        homeScreen.SetActive(true);
        stageSelectScreen.SetActive(false);

        // ���� �������� ǥ�� ������Ʈ
        UpdateStageDisplay();
    }

    void EnterStage()
    {
        // ���� ���õ� ���������� ���� ����
        Debug.Log("�������� ����: " + stageTitles[currentStageIndex]);
        // ���⼭ ���� �������� ���� �ε��ϰų� ���� ������Ʈ�� Ȱ��ȭ
    }

    void OpenStageSelect()
    {
        homeScreen.SetActive(false);
        stageSelectScreen.SetActive(true);
        UpdateStageDisplay();
    }

    void ConfirmStageSelection()
    {
        homeScreen.SetActive(true);
        stageSelectScreen.SetActive(false);
    }

    void NavigateToPreviousStage()
    {
        if (currentStageIndex > 0)
        {
            currentStageIndex--;
            UpdateStageDisplay();
        }
    }

    void NavigateToNextStage()
    {
        // Ŭ������ �������������� �̵� ����
        if (currentStageIndex < clearedStageIndex && currentStageIndex < stageTitles.Length - 1)
        {
            currentStageIndex++;
            UpdateStageDisplay();
        }
    }

    void UpdateStageDisplay()
    {
        // �������� ���� ������Ʈ
        stageTitle.text = stageTitles[currentStageIndex];
        stageDescription.text = stageDescriptions[currentStageIndex];
        stageDifficulty.text = stageDifficulties[currentStageIndex];

        // ���� �̹��� ������Ʈ (��� ������ ���� ��������)
        for (int i = 0; i < portalImages.Length; i++)
        {
            if (i < stageTitles.Length) // �����ϴ� ���������� ó��
            {
                bool isVisible = (i == currentStageIndex) || // ���� ���õ� ��������
                                 (i == currentStageIndex - 1 && currentStageIndex > 0) || // ���� ��������
                                 (i == currentStageIndex + 1 && i <= clearedStageIndex); // ���� �������� (Ŭ������ ��츸)

                portalImages[i].SetActive(isVisible);

                // ���� ���õ� ���������� �� ũ�� ǥ�� (ũ�� ����)
                if (i == currentStageIndex)
                {
                    portalImages[i].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                }
                else
                {
                    portalImages[i].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
            }
        }

        // ȭ��ǥ ��ư Ȱ��ȭ/��Ȱ��ȭ
        leftArrowButton.interactable = (currentStageIndex > 0);
        rightArrowButton.interactable = (currentStageIndex < clearedStageIndex && currentStageIndex < stageTitles.Length - 1);

        // ���� ��ư Ȱ��ȭ
        selectStageButton.interactable = true;
    }

    // �������� Ŭ���� �� ȣ���� �Լ�
    public void StageClear()
    {
        // ���� ���������� ���� ���� Ŭ���� ����������� ������Ʈ
        if (currentStageIndex == clearedStageIndex)
        {
            clearedStageIndex++;
            PlayerPrefs.SetInt("ClearedStageIndex", clearedStageIndex);
            PlayerPrefs.Save();
        }

        // Ȩ ȭ������ ���ư��鼭 ���� ���������� ����
        if (currentStageIndex < stageTitles.Length - 1)
        {
            currentStageIndex++;
        }

        homeScreen.SetActive(true);
        stageSelectScreen.SetActive(false);
        UpdateStageDisplay();
    }
}