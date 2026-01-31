using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private float gridSize = 2f;
    [SerializeField] private bool useXZPlane = true;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    [Header("Input")]
    [SerializeField] private float joystickDeadzone = 0.3f;

    [SerializeField] private PlayerType playerType;

    private bool isMoving;

    private void Update()
    {
        if (isMoving) return;

        var direction = ReadDirection();
        if (direction != Vector2.zero)
            Move(direction);
    }

    private Vector2 ReadDirection()
    {
        var raw = Vector2.zero;

        switch (playerType)
        {
            case PlayerType.Player1:
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.wKey.isPressed || kb.upArrowKey.isPressed) raw.y += 1f;
                    if (kb.sKey.isPressed || kb.downArrowKey.isPressed) raw.y -= 1f;
                    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) raw.x += 1f;
                    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) raw.x -= 1f;
                }

                break;
            }
            case PlayerType.Player2:
            {
                var gp = Gamepad.current;
                if (gp == null) return SnapToCardinal(raw);
            
                var dpad = new Vector2(
                    gp.dpad.right.isPressed ? 1f : gp.dpad.left.isPressed ? -1f : 0f,
                    gp.dpad.up.isPressed ? 1f : gp.dpad.down.isPressed ? -1f : 0f);

                raw = dpad != Vector2.zero ? dpad : gp.leftStick.ReadValue();
                break;
            }
        }


        return SnapToCardinal(raw);
    }

    private Vector2 SnapToCardinal(Vector2 input)
    {
        if (input.sqrMagnitude < joystickDeadzone * joystickDeadzone)
            return Vector2.zero;

        return Mathf.Abs(input.x) >= Mathf.Abs(input.y)
            ? new Vector2(Mathf.Sign(input.x), 0f)
            : new Vector2(0f, Mathf.Sign(input.y));
    }

    private void Move(Vector2 direction)
    {
        var realDirection = new Vector3(direction.x, 0f, direction.y);
        if (Physics.Raycast(transform.position, realDirection, out RaycastHit hit, gridSize))
        {
            if(hit.collider.CompareTag("Wall")) return;
        }
        
        
        isMoving = true;

        var offset = useXZPlane
            ? new Vector3(direction.x, 0.5f, direction.y) * gridSize
            : new Vector3(direction.x, direction.y, 0.5f) * gridSize;

        var sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(transform.position + offset, moveDuration).SetEase(moveEase))
            .Append(transform.DOMoveY(0, moveDuration))
            .OnComplete(() => isMoving = false);
            
    }
}
