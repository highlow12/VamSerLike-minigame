using UnityEngine;

public class ExpBarTester : MonoBehaviour
{
    public ExperienceBar experienceBar;

    void Update()
    {
        // Ű �Է����� ����ġ �߰�
        if (Input.GetKeyDown(KeyCode.Space)) // Space Ű�� ���� ����ġ �߰�
        {
            experienceBar.AddExperience(25); // 25 ����ġ �߰�
            Debug.Log("Added 25 XP");
        }

        if (Input.GetKeyDown(KeyCode.R)) // R Ű�� ������ ����ġ �ʱ�ȭ
        {
            experienceBar.AddExperience(-experienceBar.GetCurrentXP());
            Debug.Log("Reset XP and Level");
        }
    }
}
