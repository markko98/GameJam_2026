using DG.Tweening;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class UILoadElementAnimation
{
    private float animationDuration;
    private Sequence animation;
    private Sequence hideAnimation;
    private RectTransform target;
    private CanvasGroup targetCanvasGroup;
    private Vector3 startingLocalPosition;
    private Vector3 centerPosition;
    private readonly int offsetDistance;
    private readonly Direction direction;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="direction"></param>
    /// <param name="offsetDistance">Pass -1 to use targets width or height</param>
    public UILoadElementAnimation(RectTransform target, float duration, Direction direction, Vector3 customCenterPosition = default, int offsetDistance = -1)
    {
        this.target = target;
        this.offsetDistance = offsetDistance;
        this.animationDuration = duration;
        this.direction = direction;
        this.startingLocalPosition = target.localPosition;
        this.centerPosition = customCenterPosition;
        if (this.target.TryGetComponent(out targetCanvasGroup) == false)
        {
            targetCanvasGroup = this.target.gameObject.AddComponent<CanvasGroup>();
        }
    }
    public Sequence Animate()
    {
        var s = DOTween.Sequence();
        s.Append(targetCanvasGroup.DOFade(0, 0));
        s.Append(target.DOLocalMove(startingLocalPosition + StartingOffsetForDirection(direction), 0));
        s.Append(targetCanvasGroup.DOFade(1, animationDuration));
        s.Join(target.DOLocalMove(startingLocalPosition, animationDuration).SetEase(Ease.InOutSine));
        animation = s;
        return s;
    }

    public Sequence Hide()
    {
        var s = DOTween.Sequence();
        s.Append(target.DOLocalMove(startingLocalPosition, 0));
        s.Append(targetCanvasGroup.DOFade(0, animationDuration));
        s.Join(target.DOLocalMove(startingLocalPosition - StartingOffsetForDirection(direction), animationDuration).SetEase(Ease.InOutSine));
        hideAnimation = s;
        return s;
    }

    public Sequence AnimateContinuous(bool isShowing)
    {
        var startingPos =
            isShowing ? centerPosition + StartingOffsetForDirection(direction) : centerPosition;
        var endPosition =
            isShowing ? centerPosition : centerPosition - StartingOffsetForDirection(direction);
        var s = DOTween.Sequence();
        s.Append(targetCanvasGroup.DOFade(isShowing ? 0 : 1, 0));
        s.Append(target.DOLocalMove(startingPos, 0));
        s.Append(targetCanvasGroup.DOFade(isShowing ? 1 : 0, animationDuration));
        s.Join(target.DOLocalMove(endPosition, animationDuration).SetEase(Ease.InOutSine));
        animation = s;
        return s;
    }

    public void Cleanup(bool cleanup = true)
    {
        animation?.Kill(cleanup);
        animation = null;
        hideAnimation?.Kill(cleanup);
        hideAnimation = null;
    }

    private Vector3 StartingOffsetForDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector3.down * GetOffsetDistance(),
            Direction.Down => Vector3.up * GetOffsetDistance(),
            Direction.Left => Vector3.right * GetOffsetDistance(),
            Direction.Right => Vector3.left * GetOffsetDistance(),
            _ => Vector3.zero
        };
    }

    private float GetOffsetDistance()
    {
        if (!target) return 100f;
        
        if (offsetDistance == -1)
        {
            return direction is Direction.Down or Direction.Up ? target.rect.size.y : target.rect.size.x;
        }

        return offsetDistance;
    }

    ~UILoadElementAnimation()
    {
        Cleanup();
    }
}