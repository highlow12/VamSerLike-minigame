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

        // 컴포넌트들이 없다면 자동으로 찾거나 추가
        if (backgroundController == null)
            backgroundController = GetComponentInChildren<BackgroundController>();
        if (inputHandler == null)
            inputHandler = GetComponentInChildren<InputHandler>();
        if (screenSizeHandler == null)
            screenSizeHandler = GetComponentInChildren<ScreenSizeHandler>();

        // 컴포넌트가 없다면 자동으로 추가
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