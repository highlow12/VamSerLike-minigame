using UnityEngine;

public class ScreenSizeHandler : MonoBehaviour
{
    private Vector2 lastScreenSize;
    private BackgroundController backgroundController;
    private bool isInitialized = false;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);

        // BackgroundController 찾기 시도
        backgroundController = GetComponent<BackgroundController>();

        // 같은 게임오브젝트에서 찾지 못한 경우 부모에서 찾기
        if (backgroundController == null)
        {
            backgroundController = GetComponentInParent<BackgroundController>();
        }

        // 여전히 찾지 못한 경우 씬에서 찾기
        if (backgroundController == null)
        {
            backgroundController = FindFirstObjectByType<BackgroundController>();  // 수정된 부분
        }

        if (backgroundController == null)
        {
            Debug.LogError("BackgroundController를 찾을 수 없습니다! ScreenSizeHandler가 제대로 작동하지 않을 수 있습니다.");
            return;
        }

        isInitialized = true;
    }

    private void Update()
    {
        if (isInitialized && backgroundController != null)
        {
            CheckScreenSizeChange();
        }
    }

    private void CheckScreenSizeChange()
    {
        if (backgroundController == null) return;

        Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);

        if (currentScreenSize != lastScreenSize)
        {
            backgroundController.UpdateBackgroundSizes(currentScreenSize);
            lastScreenSize = currentScreenSize;
        }
    }

    public void SetBackgroundController(BackgroundController controller)
    {
        backgroundController = controller;
        isInitialized = true;
    }
}