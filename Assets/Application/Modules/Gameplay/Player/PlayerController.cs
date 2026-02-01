using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private float gridSize = 2f;
    [SerializeField] private bool useXZPlane = true;
    [SerializeField] private float yHeight = 0.75f;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private float rotateDuration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    [Header("Animation")] [SerializeField] private Animator animator;
    
    [Header("Input")]
    [SerializeField] private float joystickDeadzone = 0.3f;

    [SerializeField] private PlayerType playerType;

    private string dash = "Dash";
    private string die = "Die";

    [Header("Fall")]
    [SerializeField] private float fallDistance = 2f;
    [SerializeField] private float fallDuration = 0.5f;
    [SerializeField] private float groundCheckDistance = 2f;

    private bool isMoving;
    private bool isFrozen;
    private bool isDead;
    private bool canMove;
    private DisposeBag disposeBag = new DisposeBag();
    private bool isPaused;
    private EventBinding<PauseEvent> pauseBinding;
    private string victoryParam = "Victory";

    private void OnEnable()
    {
        pauseBinding = new EventBinding<PauseEvent>(OnPauseChanged);
        UEventBus<PauseEvent>.Register(pauseBinding);
        GameTicker.SharedInstance.Update += CustomUpdate;
        canMove = true;
    }
    
    private void OnDisable()
    {
        UEventBus<PauseEvent>.Deregister(pauseBinding);
        GameTicker.SharedInstance.Update -= CustomUpdate;
    }
    private void OnPauseChanged(PauseEvent args)
    {
        isPaused = args.isPaused;
    }

    private void CustomUpdate()
    {
        if(isPaused) return;
        if(!canMove) return;
        if (isMoving || isDead) return;
        
        CheckGround();

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

    private void CheckGround()
    {
        int layerMask = ~LayerMask.GetMask("PlayerDetection", "PlayerDetectionCollider");
        if (!Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, groundCheckDistance, layerMask)) return;
        
        if (hit.transform.gameObject.layer == LayerMask.NameToLayer("FallTrap"))
        {
            Die(DeathReason.Fall);
        }
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Trap"))
        {
            Die(DeathReason.Trap);
        } 
        else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("SpikeTrap"))
        {
            Die(DeathReason.Spike);
        }
    }

    private void Move(Vector2 direction)
    {
        if (isMoving) return;

        Vector3 moveDir = new Vector3(direction.x, 0f, direction.y);
        if (moveDir.sqrMagnitude < 0.001f) return;

        // Wall check
        if (Physics.Raycast(transform.position + Vector3.up, moveDir.normalized, out RaycastHit hit, gridSize, ~LayerMask.GetMask("UI")))
        {
            if (hit.collider.CompareTag("Wall")) return;
        }

        animator.SetBool(dash, true);
        isMoving = true;

        Vector3 offset = useXZPlane
            ? new Vector3(direction.x, 0f, direction.y) * gridSize
            : new Vector3(direction.x, direction.y, 0f) * gridSize;

        Vector3 targetPos = transform.position + offset;

        Vector3 flatDir = new Vector3(offset.x, 0f, offset.z);
        float yaw = Mathf.Atan2(flatDir.x, flatDir.z) * Mathf.Rad2Deg; // Unity yaw
        Quaternion targetRot = Quaternion.Euler(0f, yaw, 0f);

        transform.DOKill();

        ServiceProvider.audioService.PlayOneShot(SoundIds.sfx_move);
        DOTween.Sequence()
            .Append(transform.DORotateQuaternion(targetRot, rotateDuration))
            .Append(transform.DOMove(targetPos, moveDuration).SetEase(moveEase))
            .OnComplete(() =>
            {
                isMoving = false;
                animator.SetBool(dash, false);
                CheckGround();
            });
    }
    
    public void Kill(DeathReason reason)
    {
        Die(reason);
    }

    private void Die(DeathReason reason)
    {
        
        ServiceProvider.audioService.PlayOneShot(SoundIds.sfx_death);
        isDead = true;
        animator.SetBool(die, true);
        ShowParticle(reason);
        
        UEventBus<PlayerDiedEvent>.Raise( new PlayerDiedEvent());
        
        if (reason == DeathReason.Trap || reason == DeathReason.Arrow) return;
        
        transform.DOMoveY(transform.position.y - fallDistance, fallDuration)
            .SetEase(Ease.InQuad);
    }

    private void ShowParticle(DeathReason reason)
    {
        switch (reason)
        {
            case DeathReason.Trap:
                PlayParticle(ParticleType.ObstacleDeathParticle, Vector3.one * 2);
                DecreaseSize(0.5f);
                break;
            case DeathReason.Fall:
                PlayParticle(ParticleType.FallDeathParticle, Vector3.one);
                DecreaseSize(0.5f);
                break;
            case DeathReason.Spike:
                PlayParticle(ParticleType.TrapDeathParticle, Vector3.one * 3);
                DecreaseSize(0.5f);
                break;
            case DeathReason.Arrow:
                PlayParticle(ParticleType.TrapDeathParticle, Vector3.one * 2);
                DecreaseSize(0.5f);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
        }
    }

    private void DecreaseSize(float duration)
    {
        transform.DOScale(0, duration);
    }

    public void TriggerHappyAnimation()
    {
        canMove = false;
        PlayParticle(ParticleType.ConfettiParticle, new Vector3(3, 3, 3));
        animator.SetTrigger(victoryParam);
    }
    private void PlayParticle(ParticleType particleType, Vector3 scale)
    {
        var particle = ParticleProvider.GetParticle(particleType);
        if (particle == null) return;
        var pc = particle.GetComponent<ParticleSystem>();
        
        particle.transform.position = transform.position;
        particle.transform.localScale = scale;
        pc.Play();
        if (!Mathf.Approximately(3000, 0))
        {
            DelayedExecutionManager.ExecuteActionAfterDelay(2000,
                () =>
                {
                    particle.GetComponent<PoolableObject>().ReturnToPool();
                }).disposeBy(disposeBag);
        }
    }
}
