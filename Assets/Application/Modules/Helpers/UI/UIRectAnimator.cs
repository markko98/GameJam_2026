using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UIRectAnimator : MonoBehaviour
{
    [Header("Target (auto)")] [SerializeField]
    private RectTransform rect;
    
    public bool autoPlay = true;
    
    [Header("Motion")] 
    public bool applyMotion = true;
    [ShowIf("applyMotion")] [Min(0f)] public float amplitude = 14f;
    [ShowIf("applyMotion")] [Min(0.05f)] public float period = 0.8f;
    [ShowIf("applyMotion")] public Ease ease = Ease.InOutSine;

    [Header("Fade")] 
    public bool applyFade = true;
    [ShowIf("applyFade")] [Range(0f, 1f)] public float minAlpha = 0.45f;
    [ShowIf("applyFade")] [Range(0f, 1f)] public float maxAlpha = 1f;
    
    [Header("Scale")]
    public bool applyPulseScale = true;
    [ShowIf("applyPulseScale")] [Min(1f)] public float pulseMaxScale = 1.06f;

    // cached
    private bool isUI;
    private Vector2 startAnchoredPos;
    private Vector3 startLocalPos;
    private Transform myTransform;
    private CanvasGroup canvasGroup;
    private Sequence sequence;

    void Awake()
    {
        rect = rect ? rect : GetComponent<RectTransform>();
        isUI = rect != null;
        myTransform = transform;

        if (isUI) startAnchoredPos = rect.anchoredPosition;
        else startLocalPos = myTransform.localPosition;

        if (applyFade)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = Mathf.Clamp01(maxAlpha);
        }
    }

    void OnEnable()
    {
        if (autoPlay) Play();
    }

    void OnDisable()
    {
        Cleanup();
    }

    public void Play()
    {
        if (sequence != null && sequence.IsActive()) return;

        var half = Mathf.Max(0.01f, period * 0.5f);
        sequence = DOTween.Sequence().SetUpdate(true);

        if (applyMotion)
        {
            if (isUI)
            {
                sequence.Append(rect.DOAnchorPosY(startAnchoredPos.y + amplitude, half).SetEase(ease));
                sequence.Append(rect.DOAnchorPosY(startAnchoredPos.y, half).SetEase(ease));
            }
            else
            {
                sequence.Append(myTransform.DOLocalMoveY(startLocalPos.y + amplitude, half).SetEase(ease));
                sequence.Append(myTransform.DOLocalMoveY(startLocalPos.y, half).SetEase(ease));
            }
        }

        if (applyFade && canvasGroup)
        {
            sequence.Join(canvasGroup.DOFade(minAlpha, half).SetEase(ease));
            sequence.Append(canvasGroup.DOFade(maxAlpha, half).SetEase(ease));
        }

        if (applyPulseScale)
        {
            sequence.Join(myTransform.DOScale(pulseMaxScale, half).SetEase(ease));
            sequence.Append(myTransform.DOScale(1f, half).SetEase(ease));
        }

        sequence.SetLoops(-1, LoopType.Restart).Play();
    }

    public void Stop()
    {
        Cleanup();
    }

    private void KillTween()
    {
        if (sequence == null) return;

        sequence.Kill();
        sequence = null;
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public void Cleanup()
    {
        KillTween();
        if (applyMotion)
        {
            if (isUI) rect.anchoredPosition = startAnchoredPos;
            else myTransform.localPosition = startLocalPos;
        }

        if (applyPulseScale) myTransform.localScale = Vector3.one;
        if (applyFade && canvasGroup) canvasGroup.alpha = Mathf.Clamp01(maxAlpha);
    }
}
