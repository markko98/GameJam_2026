using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ET = UnityEngine.InputSystem.EnhancedTouch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public sealed class ModelPreviewInteraction
{
    private enum GestureMode { None, Drag, Pinch }
    
    private readonly IModelPreviewTarget target;
    private readonly RectTransform targetRect;
    private readonly Camera targetCamera;
    
    private GestureMode gesture = GestureMode.None;
    private int primaryId = -1;
    private int pinchId0 = -1;
    private int pinchId1 = -1;

    private bool requireNewTouchForDrag = false;
    private float lastPinchDistance = 0f;
    
    // config
    private ModelPreviewSettingsSO settings;

    // runtime
    private bool dragging;
    private Vector2 lastPos;
    private Vector2 angularVelocity;

    public ModelPreviewInteraction(IModelPreviewTarget target, ModelPreviewSettingsSO settings,
        RectTransform targetRect, Camera overrideUiCamera = null)
    {
        this.target = target;
        this.settings = settings;
        this.targetRect = targetRect;
        this.targetCamera = overrideUiCamera ? overrideUiCamera : ResolveCanvasCamera(targetRect);

        if (!ET.EnhancedTouchSupport.enabled)
            ET.EnhancedTouchSupport.Enable();

        GameTicker.SharedInstance.Update += CustomUpdate;
    }

    public void Dispose()
    {
        GameTicker.SharedInstance.Update -= CustomUpdate;
    }

    // update
    private void CustomUpdate()
    {
        var deltaTime = GameTicker.DeltaTime;

        if (settings.dragEnabled) UpdateDrag(deltaTime);
        if (settings.wheelEnabled) UpdateZoomWheel();
        if (settings.pinchEnabled) UpdatePinch();

        if (!dragging)
        {
            if (settings.autoSpin && angularVelocity.sqrMagnitude < 1e-4f)
            {
                target.ApplyRotationDelta(settings.autoSpinSpeed * deltaTime, 0f);
            }
            else if (angularVelocity.sqrMagnitude > 1e-4f)
            {
                var yaw = angularVelocity.x * deltaTime;
                var pitch = angularVelocity.y * deltaTime;
                target.ApplyRotationDelta(yaw, pitch);

                var d = Mathf.Exp(-settings.inertiaDamping * deltaTime);
                angularVelocity *= d;
            }
        }

        target.CustomUpdate();
    }

    // input
    private void UpdateDrag(float dt)
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            var pos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame && IsInside(pos))
            {
                gesture = GestureMode.Drag;
                dragging = true;
                lastPos = pos;
                angularVelocity = Vector2.zero;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && dragging)
            {
                dragging = false;
                gesture = GestureMode.None;
            }

            if (dragging && mouse.leftButton.isPressed)
            {
                var delta = (Vector2)pos - lastPos;
                DragPixels(delta, dt);
                lastPos = pos;
            }
        }

        // If we’re currently in a pinch, do not drag.
        if (gesture == GestureMode.Pinch) return;

        ET.Touch? chosen = null;
        foreach (var t in ET.Touch.activeTouches)
        {
            if (t.phase is TouchPhase.Began or TouchPhase.Moved or TouchPhase.Stationary)
            {
                if (IsInside(t.screenPosition))
                {
                    chosen = t;
                    break;
                }
            }
        }

        // If we just ended a pinch, we require a brand-new Began to start dragging.
        if (requireNewTouchForDrag)
        {
            bool anyBeganInside = false;
            foreach (var t in ET.Touch.activeTouches)
            {
                if (t.phase == TouchPhase.Began && IsInside(t.screenPosition))
                {
                    anyBeganInside = true;
                    chosen = t;
                    break;
                }
            }

            if (!anyBeganInside)
            {
                // still waiting for a fresh Began; ensure drag off
                dragging = false;
                return;
            }

            // got a fresh Began - allow drag again
            requireNewTouchForDrag = false;
        }

        if (chosen.HasValue)
        {
            var t = chosen.Value;

            // Start drag only on Began (prevents leftover finger from pinch becoming a drag)
            if (!dragging && t.phase == TouchPhase.Began)
            {
                gesture = GestureMode.Drag;
                dragging = true;
                primaryId = t.touchId;
                lastPos = t.screenPosition;
                angularVelocity = Vector2.zero;
            }
            else if (dragging && primaryId == t.touchId &&
                     (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
            {
                var pos = t.screenPosition;
                var delta = pos - lastPos;
                DragPixels(delta, dt);
                lastPos = pos;
            }
        }
        else
        {
            // No valid touch inside - end drag
            if (dragging)
            {
                dragging = false;
                gesture = GestureMode.None;
            }
        }

        // If the finger we were dragging with lifted/canceled, stop drag
        if (dragging)
        {
            bool primaryStillActive = false;
            foreach (var t in ET.Touch.activeTouches)
                if (t.touchId == primaryId && t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled)
                    primaryStillActive = true;

            if (!primaryStillActive)
            {
                dragging = false;
                gesture = GestureMode.None;
                primaryId = -1;
            }
        }
    }

    private void UpdateZoomWheel()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        var pos = mouse.position.ReadValue();
        if (!IsInside(pos)) return;

        var raw = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(raw) > 0.01f)
        {
            var steps = Mathf.Clamp(raw / 120f, -3f, 3f);
            var scale = Mathf.Pow(settings.mouseWheelStepMultiplier, steps);
            target.ApplyZoomMultiplier(scale);
        }
    }

    private void UpdatePinch()
    {
        var touches = ET.Touch.activeTouches;
        if (touches.Count < 2)
        {
            if (gesture == GestureMode.Pinch)
            {
                gesture = GestureMode.None;
                pinchId0 = pinchId1 = -1;
                lastPinchDistance = 0f;
                requireNewTouchForDrag = true;
                dragging = false;
                angularVelocity = Vector2.zero;
            }
            return;
        }

        ET.Touch? a = null, b = null;
        foreach (var t in touches)
        {
            if (IsInside(t.screenPosition))
            {
                if (!a.HasValue) a = t;
                else if (!b.HasValue)
                {
                    b = t;
                    break;
                }
            }
        }

        if (!a.HasValue || !b.HasValue)
            return;

        var t0 = a.Value;
        var t1 = b.Value;

        // Enter pinch state (and cancel any drag) when 2 valid touches are present
        if (gesture != GestureMode.Pinch)
        {
            gesture = GestureMode.Pinch;
            dragging = false;
            angularVelocity = Vector2.zero;
            primaryId = -1;

            pinchId0 = t0.touchId;
            pinchId1 = t1.touchId;
            lastPinchDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            return; // wait until next frame for stable delta
        }

        if (t0.touchId != pinchId0 && t0.touchId != pinchId1 &&
            t1.touchId != pinchId0 && t1.touchId != pinchId1)
        {
            pinchId0 = t0.touchId;
            pinchId1 = t1.touchId;
            lastPinchDistance = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            return;
        }

        var currDist = Vector2.Distance(t0.screenPosition, t1.screenPosition);
        if (lastPinchDistance <= 0.001f)
        {
            lastPinchDistance = currDist;
            return;
        }

        var delta = currDist / lastPinchDistance;
        lastPinchDistance = currDist;

        var scale = Mathf.Pow(settings.pinchSensitivity, (delta - 1f) * 100f);
        scale = 1f / scale;

        target.ApplyZoomMultiplier(scale);
    }

    // helper methods
    private Camera ResolveCanvasCamera(RectTransform targetRect)
    {
        var canvas = targetRect ? targetRect.GetComponentInParent<Canvas>(true) : null;
        if (!canvas) return null;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        // ScreenSpaceCamera or WorldSpace → use whatever the Canvas itself references
        var cam = canvas.worldCamera;
        if (cam && cam.enabled && cam.pixelWidth > 0 && cam.pixelHeight > 0)
            return cam;

        // Fallback: try root canvas
        var root = canvas.rootCanvas;
        return root && root.worldCamera && root.worldCamera.enabled ? root.worldCamera : null;
    }

    private void DragPixels(Vector2 deltaPx, float dt)
    {
        var yawDeg = -deltaPx.x * settings.yawSpeed;
        var pitchDeg = -deltaPx.y * settings.pitchSpeed;

        target.ApplyRotationDelta(yawDeg, pitchDeg);

        var safeDt = Mathf.Max(dt, 0.0001f);
        angularVelocity = new Vector2(yawDeg / safeDt, pitchDeg / safeDt);
    }

    private bool IsInside(Vector2 pos)
    {
        var cam = targetCamera;
        if (cam && (!cam.enabled || cam.pixelWidth <= 0 || cam.pixelHeight <= 0))
            cam = null;
        return RectTransformUtility.RectangleContainsScreenPoint(targetRect, pos, cam);
    }
}