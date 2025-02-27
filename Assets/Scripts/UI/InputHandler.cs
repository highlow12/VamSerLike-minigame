using UnityEngine;
using System;

public enum SwipeDirection
{
    Left,
    Right,
    None
}

public class InputHandler : MonoBehaviour
{
    private Vector2 touchStartPos;
    private Action<SwipeDirection> onSwipeDetected;
    private float minSwipeDistance = 50f;

    public void Initialize(Action<SwipeDirection> swipeCallback)
    {
        onSwipeDetected = swipeCallback;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                touchStartPos = touch.position;
            else if (touch.phase == TouchPhase.Ended)
                ProcessSwipe(touch.position);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ProcessSwipe(Input.mousePosition);
        }
    }

    private void ProcessSwipe(Vector2 endPos)
    {
        Vector2 swipeDelta = endPos - touchStartPos;

        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y) && Mathf.Abs(swipeDelta.x) > minSwipeDistance)
        {
            SwipeDirection direction = swipeDelta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            onSwipeDetected?.Invoke(direction);
        }
    }
}