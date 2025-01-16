using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualJoystick : MonoBehaviour
{
    public bool _isReleased = false;
    Joystick _joystick;
    PlayerMove _playerMove;

    void Start()
    {
        _joystick = GetComponent<Joystick>();
        _playerMove = FindFirstObjectByType<PlayerMove>();
    }

    void Update()
    {
        if (_joystick.input == Vector2.zero)
        {
            if (!_isReleased)
            {
                _playerMove.OnMoveWithVirtualJoystick(Vector2.zero);
            }
            _isReleased = true;
        }
        else
        {
            _isReleased = false;
            _playerMove.OnMoveWithVirtualJoystick(_joystick.input);
        }
    }


}