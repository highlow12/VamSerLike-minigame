using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private RectTransform[] backgrounds;
    public float transitionSpeed = 5f;
    private int currentBackgroundIndex = 1;  // 2번 화면(인덱스 1)을 시작 화면으로
    private bool isTransitioning = false;
    private readonly Vector2 BASE_RESOLUTION = new Vector2(1080f, 2340f);

    public void Initialize()
    {
        if (backgrounds == null || backgrounds.Length == 0)
        {
            Debug.LogError("Backgrounds not set!");
            return;
        }

        // 현재 인덱스를 1(2번 화면)로 설정
        currentBackgroundIndex = 1;
        SetupBackgrounds();
    }

    public void SetupBackgrounds()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;

            // 크기 설정
            backgrounds[i].sizeDelta = BASE_RESOLUTION;

            // 위치 설정 (2번 화면이 중앙에 오도록)
            float xPos = (i - currentBackgroundIndex) * BASE_RESOLUTION.x;
            backgrounds[i].anchoredPosition = new Vector2(xPos, 0);

            Debug.Log($"Setting background {i} position to: {xPos}, 0");

            // Image 컴포넌트 설정
            var image = backgrounds[i].GetComponent<Image>();
            if (image != null)
            {
                image.preserveAspect = true;
            }
        }
    }

    public void UpdateBackgroundSizes(Vector2 screenSize)
    {
        if (backgrounds == null || backgrounds.Length == 0 || isTransitioning)
            return;

        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;

            backgrounds[i].sizeDelta = BASE_RESOLUTION;

            // 현재 인덱스 기준으로 위치 업데이트
            float xPos = (i - currentBackgroundIndex) * BASE_RESOLUTION.x;
            backgrounds[i].anchoredPosition = new Vector2(xPos, 0);
        }
    }

    public void HandleSwipe(SwipeDirection direction)
    {
        if (isTransitioning) return;

        int newIndex = currentBackgroundIndex;
        if (direction == SwipeDirection.Left && currentBackgroundIndex < backgrounds.Length - 1)
        {
            newIndex++;
        }
        else if (direction == SwipeDirection.Right && currentBackgroundIndex > 0)
        {
            newIndex--;
        }
        else
        {
            return;
        }

        StartCoroutine(TransitionToBackground(newIndex));
    }

    private IEnumerator TransitionToBackground(int targetIndex)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        float startTime = Time.time;
        float duration = 0.5f;
        Vector2[] startPositions = new Vector2[backgrounds.Length];
        Vector2[] targetPositions = new Vector2[backgrounds.Length];

        // 시작 위치와 목표 위치 계산
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;
            startPositions[i] = backgrounds[i].anchoredPosition;
            targetPositions[i] = new Vector2((i - targetIndex) * BASE_RESOLUTION.x, 0);
        }

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Mathf.SmoothStep(0, 1, t);

            for (int i = 0; i < backgrounds.Length; i++)
            {
                if (backgrounds[i] == null) continue;
                backgrounds[i].anchoredPosition = Vector2.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }

        // 최종 위치 설정
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;
            backgrounds[i].anchoredPosition = targetPositions[i];
        }

        currentBackgroundIndex = targetIndex;
        isTransitioning = false;
    }
}