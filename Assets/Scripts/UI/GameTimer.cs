using UnityEngine;
using TMPro;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timeText; // 시간을 표시할 TMP Text 컴포넌트
    [SerializeField] private bool countUp = true; // true: 시간 증가, false: 시간 감소
    [SerializeField] private float initialTime = 0f; // 초기 시간 (초 단위)
    [SerializeField] private float totalTime = 900f; // 15분(900초)

    private float currentTime; // 현재 시간 (초 단위)
    private bool isRunning; // 타이머 작동 여부

    private void Start()
    {
        // 게임 시작 시 자동으로 타이머 시작
        ResetTimer();
        StartTimer();
    }

    private void Update()
    {
        if (isRunning)
        {
            // 시간 업데이트
            if (countUp)
            {
                currentTime += Time.deltaTime;

                // 15분(900초)에 도달하면 타이머 정지
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

                // 카운트다운 모드에서 시간이 0 이하로 내려가면 타이머 정지
                if (currentTime <= 0f)
                {
                    currentTime = 0f;
                    StopTimer();
                    OnTimerComplete();
                }
            }

            // UI 업데이트
            UpdateTimeDisplay();
        }
    }

    // 타이머 시작
    public void StartTimer()
    {
        isRunning = true;
    }

    // 타이머 정지
    public void StopTimer()
    {
        isRunning = false;
    }

    // 타이머 리셋
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

    // 시간 표시 업데이트 (분:초 형식으로만 표시)
    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;

        // 분, 초 계산
        int minutes = (int)(currentTime / 60f);
        int seconds = (int)(currentTime % 60f);

        // 분:초 형식으로 표시
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        // UI 텍스트 업데이트
        timeText.text = timeString;
    }

    // 타이머 완료 시 호출되는 메서드 (카운트다운 모드)
    private void OnTimerComplete()
    {
        Debug.Log("Timer Complete!");
        // 여기에 타이머가 완료되었을 때 수행할 동작을 추가하세요
    }
}