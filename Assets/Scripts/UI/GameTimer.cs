using UnityEngine;
using TMPro;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timeText; // �ð��� ǥ���� TMP Text ������Ʈ
    [SerializeField] private bool countUp = true; // true: �ð� ����, false: �ð� ����
    [SerializeField] private float initialTime = 0f; // �ʱ� �ð� (�� ����)
    [SerializeField] private float totalTime = 900f; // 15��(900��)

    private float currentTime; // ���� �ð� (�� ����)
    private bool isRunning; // Ÿ�̸� �۵� ����

    private void Start()
    {
        // ���� ���� �� �ڵ����� Ÿ�̸� ����
        ResetTimer();
        StartTimer();
    }

    private void Update()
    {
        if (isRunning)
        {
            // �ð� ������Ʈ
            if (countUp)
            {
                currentTime += Time.deltaTime;

                // 15��(900��)�� �����ϸ� Ÿ�̸� ����
                if (currentTime >= totalTime)
                {
                    currentTime = totalTime;
                    StopTimer();
                    OnTimerComplete();
                }
            }
            else
            {
                currentTime -= Time.deltaTime;

                // ī��Ʈ�ٿ� ��忡�� �ð��� 0 ���Ϸ� �������� Ÿ�̸� ����
                if (currentTime <= 0f)
                {
                    currentTime = 0f;
                    StopTimer();
                    OnTimerComplete();
                }
            }

            // UI ������Ʈ
            UpdateTimeDisplay();
        }
    }

    // Ÿ�̸� ����
    public void StartTimer()
    {
        isRunning = true;
    }

    // Ÿ�̸� ����
    public void StopTimer()
    {
        isRunning = false;
    }

    // Ÿ�̸� ����
    public void ResetTimer()
    {
        if (countUp)
        {
            currentTime = initialTime;
        }
        else
        {
            currentTime = totalTime;
        }
        UpdateTimeDisplay();
    }

    // �ð� ǥ�� ������Ʈ (��:�� �������θ� ǥ��)
    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;

        // ��, �� ���
        int minutes = (int)(currentTime / 60f);
        int seconds = (int)(currentTime % 60f);

        // ��:�� �������� ǥ��
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        // UI �ؽ�Ʈ ������Ʈ
        timeText.text = timeString;
    }

    // Ÿ�̸� �Ϸ� �� ȣ��Ǵ� �޼��� (ī��Ʈ�ٿ� ���)
    private void OnTimerComplete()
    {
        Debug.Log("Timer Complete!");
        // ���⿡ Ÿ�̸Ӱ� �Ϸ�Ǿ��� �� ������ ������ �߰��ϼ���
    }
}