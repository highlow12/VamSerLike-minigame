using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class buttonTest : MonoBehaviour
{
    [Header("�׽�Ʈ ��ư")]
    [SerializeField] private Button addXpButton;     // ����ġ �߰� ��ư
    [SerializeField] private Button levelUpButton;   // ���� ������ ��ư

    [Header("����")]
    [SerializeField] private float xpAmountToAdd = 50f;  // �߰��� ����ġ��

    void Start()
    {
        // ��ư �̺�Ʈ ����
        if (addXpButton != null)
        {
            addXpButton.onClick.AddListener(AddExperience);
        }

        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(ForcePlayerLevelUp);
        }
    }

    // ����ġ �߰� �Լ�
    public void AddExperience()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience(xpAmountToAdd);
            Debug.Log($"����ġ {xpAmountToAdd} �߰���. ���� ����ġ: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp}");
        }
        else
        {
            Debug.LogError("GameManager�� ã�� �� �����ϴ�!");
        }
    }

    // ���� ������ �Լ�
    public void ForcePlayerLevelUp()
    {
        if (GameManager.Instance != null)
        {
            // ���� ����ġ���� �ʿ� ����ġ������ ���� + 1��ŭ ����ġ �߰�
            float expNeeded = GameManager.Instance.experienceToLevelUp - GameManager.Instance.playerExperience + 1;
            Debug.Log($"�������� ���� {expNeeded} ����ġ �߰�");
            GameManager.Instance.AddExperience(expNeeded);
        }
        else
        {
            Debug.LogError("GameManager�� ã�� �� �����ϴ�!");
        }
    }
}