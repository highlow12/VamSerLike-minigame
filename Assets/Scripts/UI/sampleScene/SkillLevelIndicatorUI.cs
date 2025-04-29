using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level-up 옵션 패널에서 각 무기의 현재 레벨과 다음 레벨 위치를 표시하는
/// 원형 인디케이터를 생성하고 깜빡이는 효과를 매니징합니다.
/// </summary>
public class SkillLevelIndicatorUI : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("표시할 최대 레벨 수")]
    [SerializeField] private int maxLevel = 5;
    [Tooltip("깜빡임 간격 (초)")]
    [SerializeField] private float blinkInterval = 0.5f;
    [Tooltip("인디케이터 크기 (픽셀)")]
    [SerializeField] private Vector2 indicatorSize = new Vector2(5, 5);
    [Tooltip("인디케이터 간격 (픽셀)")]
    [SerializeField] private float indicatorSpacing = 4f;

    [Header("Prefabs")]
    [Tooltip("원형 인디케이터로 사용할 Image 프리팹(빈 원형)")]
    [SerializeField] private GameObject indicatorPrefab;

    private List<Image> indicators = new List<Image>();
    private Coroutine blinkCoroutine;

    void Awake()
    {
        // maxLevel만큼 인디케이터 인스턴스화
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject go = Instantiate(indicatorPrefab, transform);
            go.name = $"Indicator_{i}";

            // 크기 및 위치 조정
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = indicatorSize;
            float totalWidth = maxLevel * indicatorSize.x + (maxLevel - 1) * indicatorSpacing;
            float startX = -totalWidth * 0.5f + indicatorSize.x * 0.5f;
            rt.anchoredPosition = new Vector2(startX + i * (indicatorSize.x + indicatorSpacing), 0);

            Image img = go.GetComponent<Image>();
            if (img != null)
            {
                indicators.Add(img);
                img.color = Color.gray;
            }
        }
    }

    /// <summary>
    /// 현재 레벨에 맞춰 인디케이터를 업데이트합니다.
    /// </summary>
    /// <param name="currentLevel">획득된 레벨 수 (0부터 시작)</param>
    public void Setup(int currentLevel)
    {
        // 이전 깜빡임 중단
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        // 색상 및 활성화 설정
        for (int i = 0; i < indicators.Count; i++)
        {
            var img = indicators[i];
            img.gameObject.SetActive(true);
            img.color = (i < currentLevel) ? Color.yellow : Color.gray;
        }

        // 다음 레벨 위치 깜빡이기
        if (currentLevel < indicators.Count)
            blinkCoroutine = StartCoroutine(BlinkNext(currentLevel));
    }

    private IEnumerator BlinkNext(int index)
    {
        Image img = indicators[index];
        bool on = false;

        while (true)
        {
            img.color = on ? Color.white : Color.gray;
            on = !on;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void OnDestroy()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }
}
