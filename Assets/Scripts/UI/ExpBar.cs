using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExperienceBar : MonoBehaviour
{
    public Slider experienceSlider; // ����ġ �����̴�
    public Text levelText;          // ���� �ؽ�Ʈ
    public float slideSpeed = 5f;   // �����̵� �ӵ�

    private int playerLevel = 1;    // ���� �÷��̾� ����
    private int currentXP = 0;      // ���� ����ġ
    private int xpToNextLevel = 100; // ���� �������� �ʿ��� ����ġ

    void Start()
    {
        UpdateUI();
    }

    // ���� ����ġ�� ��ȯ
    public int GetCurrentXP()
    {
        return currentXP;
    }

    // ����ġ ���� �� UI ������Ʈ
    public void AddExperience(int amount)
    {
        StartCoroutine(SlideExperience(amount));
    }

    // �����̵� �ִϸ��̼� �ڷ�ƾ
    private IEnumerator SlideExperience(int amount)
    {
        int targetXP = currentXP + amount;

        while (currentXP < targetXP)
        {
            // �����̴� �� ����
            currentXP = Mathf.Min(currentXP + Mathf.CeilToInt(slideSpeed * Time.deltaTime), targetXP);
            experienceSlider.value = (float)currentXP / xpToNextLevel;

            // ���� �� üũ
            if (currentXP >= xpToNextLevel)
            {
                // �ʰ��� ����ġ ���
                int overflowXP = currentXP - xpToNextLevel;
                currentXP = 0;
                LevelUp();

                // �ʰ� ����ġ�� ���� �ܰ迡 �ݿ�
                targetXP = overflowXP;
            }

            yield return null;
        }

        UpdateUI();
    }

    // ���� �� ó��
    private void LevelUp()
    {
        playerLevel++;
        xpToNextLevel = Mathf.FloorToInt(xpToNextLevel * 1.2f); // ������ �� �ʿ� ����ġ ����
    }

    // UI ������Ʈ
    private void UpdateUI()
    {
        levelText.text = "Lv. " + playerLevel;
        experienceSlider.value = (float)currentXP / xpToNextLevel;
    }
}
