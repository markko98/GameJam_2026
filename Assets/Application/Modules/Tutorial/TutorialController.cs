using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialController
{
    [Serializable]
    public struct Step
    {
        public string stepId;
        public string anchorId;
        public bool dismissOnAnchorAction;
        [Tooltip("Optional delay before this step starts (in seconds)")]
        public float delayBeforeShow;
        public ModalRequest modalRequest;
    }
    
    private readonly float globalStepDelay = 0f;
    public event Action<string> StepShown;
    public event Action TutorialEnded;
    public event Action<string> StepCompleted;

    private readonly Dictionary<string, Step> stepsById = new();
    private readonly List<string> orderedIds = new();
    private readonly TutorialHighlighter highlighter;
    private readonly IModalService modalService;

    private int index = -1;
    private TutorialAnchor currentAnchor;
    private string currentModalId;
    private int showGeneration;
    private bool singleStepMode;

    public TutorialController(TutorialHighlighter highlighter, IEnumerable<Step> stepsInOrder,
        IModalService modalService)
    {
        this.highlighter = highlighter ?? throw new ArgumentNullException(nameof(highlighter));
        this.modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));

        foreach (var s in stepsInOrder)
        {
            if (string.IsNullOrEmpty(s.stepId))
                throw new ArgumentException("Each tutorial step must have a unique stepId.");
            stepsById[s.stepId] = s;
            orderedIds.Add(s.stepId);
        }
    }

    public bool IsRunning => index >= 0 && index < orderedIds.Count;

    public void Begin()
    {
        singleStepMode = false;
        ShowIndex(0);
    }

    public void BeginFrom(string stepId)
    {
        singleStepMode = false;
        var idx = orderedIds.IndexOf(stepId);
        ShowIndex(idx < 0 ? 0 : idx);
    }

    public void ShowStep(string stepId, bool singleStep = false)
    {
        var idx = orderedIds.IndexOf(stepId);
        if (idx >= 0)
        {
            singleStepMode = singleStep;
            ShowIndex(idx);
        }
    }

    public void Next()
    {
        if (!IsRunning) return;
        ShowIndex(index + 1);
    }

    public void End()
    {
        UnhookCurrent();
        highlighter.Hide();
        index = -1;
        TutorialEnded?.Invoke();
    }


    private async void ShowIndex(int newIndex)
    {
        UnhookCurrent();

        if (newIndex < 0 || newIndex >= orderedIds.Count)
        {
            End();
            return;
        }

        index = newIndex;
        var step = stepsById[orderedIds[index]];
        if (step.delayBeforeShow > 0f)
            await AwaitableWait(step.delayBeforeShow);

        var thisGen = ++showGeneration;

        var anchor = TutorialAnchorRegistry.Get(step.anchorId);
        if (anchor == null || anchor.RectTransform == null || anchor.RectTransform.gameObject.activeInHierarchy == false)
        {
            Debug.LogWarning($"[Tutorial] Anchor '{step.anchorId}' missing; skipping step '{step.stepId}'.");
            ShowIndex(index + 1);
            return;
        }

        currentAnchor = anchor;

        highlighter.Show(anchor.RectTransform);

        // Ensure this step has a fresh correlation id for its modal
        if (string.IsNullOrEmpty(step.modalRequest.correlationId))
            step.modalRequest.correlationId = System.Guid.NewGuid().ToString("N");
        currentModalId = step.modalRequest.correlationId;

        var req = BuildModalForStep(step);
        var task = modalService.ShowAsync(req);

        if (step.dismissOnAnchorAction)
        {
            var btn = currentAnchor.GetButton();
            if (btn != null)
                currentAnchor.onAction.AddListener(OnAnchorAction);
            else
                highlighter.EnableHoleTap(OnAnchorHoleTap);

        }
        else
        {
            var myModalId = req.correlationId;
            CoroutineExecutor.Run(task, result =>
            {
                if (showGeneration != thisGen) return;
                if (currentModalId != myModalId) return;

                if (result == ModalResult.Confirmed)
                {
                    StepCompleted?.Invoke(step.stepId);
                    
                    bool isLast = index >= orderedIds.Count - 1;
                    if (singleStepMode) { singleStepMode = false; End(); }
                    else if (isLast) { End(); }
                    else { Next(); }
                }
                else
                {
                    End();
                }
            });
        }

        StepShown?.Invoke(step.stepId);
    }
    
    private ModalRequest BuildModalForStep(Step step)
    {
        var viewport = PersistentReferences.Instance.tutorialModalViewport.GetComponent<RectTransform>();
        var canvas = viewport.GetComponentInParent<Canvas>();
        var uiCam = canvas && canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
        var anchor = TutorialAnchorRegistry.Get(step.anchorId);
        
        var req = step.modalRequest ?? new ModalRequest();
        req.allowBackgroundDismiss = false;
        req.showBackgroundBlur = false;
        req.targetRectInViewport = ToViewportRect(anchor.RectTransform, viewport, uiCam);

        if (step.dismissOnAnchorAction)
        {
            req.showConfirm = false;
            req.showClose = false;
            if (string.IsNullOrEmpty(req.title)) req.title = "Hint";
            if (string.IsNullOrEmpty(req.description)) req.description = "Tap the highlighted control to continue.";
        }
        else
        {
            req.showConfirm = true;
            if (string.IsNullOrEmpty(req.confirmLabel)) req.confirmLabel = "Continue";
        }
        return req;
    }
    
    private void OnAnchorAction()
    {
        if (!string.IsNullOrEmpty(currentModalId))
            modalService.DismissById(currentModalId, ModalResult.Confirmed);

        StepCompleted?.Invoke(stepsById[orderedIds[index]].stepId);
        CoroutineExecutor.Instance.StartCoroutine(NextOrEnd());
    }

    private void OnAnchorHoleTap()
    {
        if (!string.IsNullOrEmpty(currentModalId))
            modalService.DismissById(currentModalId, ModalResult.Confirmed);

        CoroutineExecutor.Instance.StartCoroutine(NextOrEnd());
    }

    private IEnumerator NextOrEnd()
    {
        yield return null;
        if (singleStepMode) { singleStepMode = false; End(); }
        else Next();
    }

    private void UnhookCurrent()
    {
        if (currentAnchor != null)
            currentAnchor.onAction.RemoveListener(OnAnchorAction);
        currentAnchor = null;
    }
    
    private Rect ToViewportRect(RectTransform target, RectTransform viewport, Camera uiCam)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector2[] local = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            var sp = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewport, sp, uiCam, out local[i]);
        }

        return Rect.MinMaxRect(
            Mathf.Min(local[0].x, local[2].x), Mathf.Min(local[0].y, local[2].y),
            Mathf.Max(local[0].x, local[2].x), Mathf.Max(local[0].y, local[2].y));
    }
    private async Task AwaitableWait(float seconds)
    {
        float target = Time.time + seconds + globalStepDelay;
        while (Time.time < target)
            await Task.Yield();
    }

}
