using UnityEngine;

public class ExpBarTester : MonoBehaviour
{
    public ExperienceBar experienceBar;

    void Update()
    {
        // 키 입력으로 경험치 추가
        if (Input.GetKeyDown(KeyCode.Space)) // Space 키를 눌러 경험치 추가
        {
            experienceBar.AddExperience(25); // 25 경험치 추가
            Debug.Log("Added 25 XP");
        }

        if (Input.GetKeyDown(KeyCode.R)) // R 키로 레벨과 경험치 초기화
        {
            experienceBar.AddExperience(-experienceBar.GetCurrentXP());
            Debug.Log("Reset XP and Level");
        }
    }
}
