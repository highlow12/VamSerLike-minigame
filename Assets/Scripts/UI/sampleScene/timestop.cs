using UnityEngine;

public class TimeStop : MonoBehaviour
{
    public void TogglePause()
    {
        // ���� ���¸� ������Ŵ (�Ͻ����� �� �簳)
        GameManager.IsGamePaused = !GameManager.IsGamePaused;

        // �ð� ������ ���� (�Ͻ������� 0, �簳�� 1)
        Time.timeScale = GameManager.IsGamePaused ? 0f : 1f;

        Debug.Log("Game Paused: " + GameManager.IsGamePaused);
    }

    // UI ��ư�� �����ϱ� ���� ������ �޼���
    public void PauseButtonClicked()
    {
        // ��ư Ŭ���� ������ �Ͻ�����
        GameManager.IsGamePaused = true;
        Time.timeScale = 0f;
        Debug.Log("Game Paused by Button");
    }

    // UI ��ư�� �����ϱ� ���� ������ �޼���
    public void ResumeButtonClicked()
    {
        // ��ư Ŭ���� ������ �簳
        GameManager.IsGamePaused = false;
        Time.timeScale = 1f;
        Debug.Log("Game Resumed by Button");
    }
}