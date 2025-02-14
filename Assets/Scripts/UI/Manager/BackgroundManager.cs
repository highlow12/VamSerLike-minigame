using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance { get; private set; }

    [SerializeField] private BackgroundController backgroundController;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private ScreenSizeHandler screenSizeHandler;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // ������Ʈ���� ���ٸ� �ڵ����� ã�ų� �߰�
        if (backgroundController == null)
            backgroundController = GetComponentInChildren<BackgroundController>();
        if (inputHandler == null)
            inputHandler = GetComponentInChildren<InputHandler>();
        if (screenSizeHandler == null)
            screenSizeHandler = GetComponentInChildren<ScreenSizeHandler>();

        // ������Ʈ�� ���ٸ� �ڵ����� �߰�
        if (backgroundController == null)
            backgroundController = gameObject.AddComponent<BackgroundController>();
        if (inputHandler == null)
            inputHandler = gameObject.AddComponent<InputHandler>();
        if (screenSizeHandler == null)
            screenSizeHandler = gameObject.AddComponent<ScreenSizeHandler>();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (backgroundController != null)
            backgroundController.Initialize();
        if (screenSizeHandler != null)
            screenSizeHandler.Initialize();
        if (inputHandler != null)
            inputHandler.Initialize(OnSwipeDetected);
    }

    private void OnSwipeDetected(SwipeDirection direction)
    {
        if (backgroundController != null)
            backgroundController.HandleSwipe(direction);
    }
}