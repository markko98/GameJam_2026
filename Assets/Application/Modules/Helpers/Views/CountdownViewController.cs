using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class CountdownViewController : UIViewController
{
    private readonly Action onCountdownFinished;
    private readonly float duration;
    private CountdownTimer countdownTimer;
    private AudioClip countdownSound;
    
    // elements labels
    private const string BackgroundLabel = "Background";
    private const string CountdownTextLabel = "CountdownText";
    
    // cached elements
    private UIImageViewComponent backgroundImageView;
    private TextMeshProUGUI countdownText;
    
    // runtime
    private int lastDisplayNumber = -1;
    private Sequence countdownSequence;
    private readonly string startingText;

    public CountdownViewController(Transform viewport, Action onCountdownFinished, UIStackNavigationController controller, string startingText, float duration = 3f) : base(controller)
    {
        this.onCountdownFinished = onCountdownFinished;
        this.startingText = startingText;
        
        this.duration = duration;
        var obj = Resources.Load($"Modules/Countdown/CountdownView");
        view = Object.Instantiate(obj, viewport, false ) as GameObject;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        CacheElements();
        ToggleBackground(true);
        SetStartingText();
    }

    private void SetStartingText()
    {
        countdownText.SetText(startingText);
    }

    private void CacheElements()
    {
        backgroundImageView = GetUIImageViewComponentWithName(BackgroundLabel);
        countdownText = GetTextWithName(CountdownTextLabel);
    }

    private Sequence ToggleBackground(bool show)
    {
        var s = DOTween.Sequence();
        s.Append(backgroundImageView.image.DOFade(show ? 0f : 0.15f, 0));
        s.Append(backgroundImageView.image.DOFade(show ? 0.15f : 0f, 0.2f));
        return s;
    }

    public void StartCountdown()
    {
        lastDisplayNumber = -1;
        countdownTimer = new CountdownTimer(duration, CountdownFinished, CountdownUpdated);
    }

    private void CountdownUpdated(float time)
    {
        int displayNumber = Mathf.CeilToInt(time);
    
        // Avoid updating if number is same or zero
        if (displayNumber == lastDisplayNumber || displayNumber == 0) return;

        lastDisplayNumber = displayNumber;
        countdownText.SetText(displayNumber.ToString());

        AnimateCountdownText();
    }

    private void AnimateCountdownText()
    {
        var textRect = countdownText.rectTransform;

        countdownText.alpha = 1f;
        textRect.anchoredPosition = Vector2.zero;
        textRect.localScale = Vector3.one;

        countdownSequence?.Kill();
        countdownSequence = DOTween.Sequence();
        countdownSequence.Join(countdownText.DOFade(0f, 0.9f).SetEase(Ease.OutQuad));
        countdownSequence.Join(textRect.DOAnchorPosY(50f, 0.9f).SetEase(Ease.OutQuad));
        countdownSequence.Join(textRect.DOPunchScale(Vector3.one * 0.3f, 0.4f, vibrato: 2, elasticity: 0.8f));
    }

    private void CountdownFinished()
    {
        onCountdownFinished?.Invoke();
        ToggleBackground(false);
        RemoveView();
    }
}
