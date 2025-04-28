using UnityEngine;

public class TimeStop : MonoBehaviour
{
    public void TogglePause()
    {
        // 현재 상태를 반전시킴 (일시정지 ↔ 재개)
        GameManager.IsGamePaused = !GameManager.IsGamePaused;

        // 시간 스케일 조정 (일시정지시 0, 재개시 1)
        Time.timeScale = GameManager.IsGamePaused ? 0f : 1f;

        Debug.Log("Game Paused: " + GameManager.IsGamePaused);
    }

    // UI 버튼에 연결하기 위한 별도의 메서드
    public void PauseButtonClicked()
    {
        // 버튼 클릭시 무조건 일시정지
        GameManager.IsGamePaused = true;
        Time.timeScale = 0f;
        Debug.Log("Game Paused by Button");
    }

    // UI 버튼에 연결하기 위한 별도의 메서드
    public void ResumeButtonClicked()
    {
        // 버튼 클릭시 무조건 재개
        GameManager.IsGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed by Button");
    }
}