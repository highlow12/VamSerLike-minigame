using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level-up �ɼ� �гο��� �� ������ ���� ������ ���� ���� ��ġ�� ǥ���ϴ�
/// ���� �ε������͸� �����ϰ� �����̴� ȿ���� �Ŵ�¡�մϴ�.
/// </summary>
public class SkillLevelIndicatorUI : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("ǥ���� �ִ� ���� ��")]
    [SerializeField] private int maxLevel = 5;
    [Tooltip("������ ���� (��)")]
    [SerializeField] private float blinkInterval = 0.5f;
    [Tooltip("�ε������� ũ�� (�ȼ�)")]
    [SerializeField] private Vector2 indicatorSize = new Vector2(5, 5);
    [Tooltip("�ε������� ���� (�ȼ�)")]
    [SerializeField] private float indicatorSpacing = 4f;

    [Header("Prefabs")]
    [Tooltip("���� �ε������ͷ� ����� Image ������(�� ����)")]
    [SerializeField] private GameObject indicatorPrefab;

    private List<Image> indicators = new List<Image>();
    private Coroutine blinkCoroutine;

    void Awake()
    {
        // maxLevel��ŭ �ε������� �ν��Ͻ�ȭ
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject go = Instantiate(indicatorPrefab, transform);
            go.name = $"Indicator_{i}";

            // ũ�� �� ��ġ ����
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
    /// ���� ������ ���� �ε������͸� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentLevel">ȹ��� ���� �� (0���� ����)</param>
    public void Setup(int currentLevel)
    {
        // ���� ������ �ߴ�
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        // ���� �� Ȱ��ȭ ����
        for (int i = 0; i < indicators.Count; i++)
        {
            var img = indicators[i];
            img.gameObject.SetActive(true);
            img.color = (i < currentLevel) ? Color.yellow : Color.gray;
        }

        // ���� ���� ��ġ �����̱�
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
