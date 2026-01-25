using System;
using UnityEngine;

public enum ModalResult { None, Confirmed, Closed, DismissedBackground }
public enum ModalPlacement { Center, Above, Below, Left, Right, Auto }

public enum ModalPositionMode
{
    AutoNearTarget, // (default) PlaceNearTarget
    Center, // center of viewport
    ViewportNormalized, // position.x/y in [0..1] of viewport rect
    ViewportPixels // position is viewport-local pixels (RectTransform space)
}

public sealed class ModalRequest
{
    public string title;
    public string progress;
    public string description;
    public Sprite sprite;
    public Vector2 spriteSize = new Vector2(256, 256);

    public bool showConfirm = true;
    public bool showClose = true;
    public string confirmLabel = "Confirm";
    public string closeLabel = "Close";

    public bool allowBackgroundDismiss = false;
    public bool showBackgroundBlur = true;
    
    public string correlationId;
    
    public ModalPlacement placement = ModalPlacement.Auto;
    public Vector2 placementOffset;
    public bool clampToViewport = true;
    public float viewportPadding = 16f;
    public Rect? targetRectInViewport;
    
    public ModalPositionMode positionMode = ModalPositionMode.AutoNearTarget;
    public Vector2 position; // Meaning depends on mode
    public Vector2? pivotOverride;

    public AnimationType? animationShowOverride;
    public AnimationType? animationHideOverride;
}

public sealed class ModalSequenceOptions
{
    public bool allowBack = true;
    public bool allowSkip = true;
    public Action<int> onStepShown;
    public Action onCompleted;
    public Action onCanceled;
}
