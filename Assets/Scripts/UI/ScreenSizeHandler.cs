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

        // BackgroundController ã�� �õ�
        backgroundController = GetComponent<BackgroundController>();

        // ���� ���ӿ�����Ʈ���� ã�� ���� ��� �θ𿡼� ã��
        if (backgroundController == null)
        {
            backgroundController = GetComponentInParent<BackgroundController>();
        }

        // ������ ã�� ���� ��� ������ ã��
        if (backgroundController == null)
        {
            backgroundController = FindFirstObjectByType<BackgroundController>();  // ������ �κ�
        }

        if (backgroundController == null)
        {
            Debug.LogError("BackgroundController�� ã�� �� �����ϴ�! ScreenSizeHandler�� ����� �۵����� ���� �� �ֽ��ϴ�.");
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