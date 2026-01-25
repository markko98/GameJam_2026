using NaughtyAttributes;
using UnityEngine;

public enum PerspectiveZoomMode { Distance, FOV, Hybrid }

[CreateAssetMenu(menuName = "ModelPreview/ModelPreviewSettings",fileName = "ModelPreviewSettings")]
public class ModelPreviewSettingsSO : ScriptableObject
{
    [Header("Features")]
    public bool dragEnabled = true;
    public bool pinchEnabled = true;
    public bool wheelEnabled = true;
    public bool autoSpin = true;
    
    [Header("View settings")]
    public Vector2 pitchClamp = new Vector2(-45f, 45f);
    [Range(0.1f, 1f)] public float minZoom = 0.6f;
    [Range(1f, 3f)] public float maxZoom = 2.0f;
    
    [Header("Perspective zoom")]
    public bool useTelephotoPerspective = true;
    [ShowIf("useTelephotoPerspective")] [Range(1f, 90f)] public float telephotoMinFOV = 25f;
    [ShowIf("useTelephotoPerspective")] [Range(1f, 90f)] public float telephotoMaxFOV = 35f;
    [ShowIf("useTelephotoPerspective")] [Min(0.1f)] public float perspectiveDistanceMultiplier = 1.2f;
    [ShowIf("useTelephotoPerspective")] public PerspectiveZoomMode perspectiveZoomMode = PerspectiveZoomMode.Hybrid;
    
    [Header("Camera settings")]
    public bool useOrthographicCamera = true;
    [ShowIf("useOrthographicCamera")]  public Vector2 orthographicSize = new Vector2(3, 8);
    
    [Header("Input")]
    [Tooltip("Multiplier for calculating dragged pixels to degrees on x axis"), Range(0.05f, 1f)] public float yawSpeed = 0.25f;
    [Tooltip("Multiplier for calculating dragged pixels to degrees on y axis"), Range(0.01f, 1f)] public float pitchSpeed = 0.20f;
    [Tooltip("Higher value = faster stop on input release"), Range(2f, 12f)] public float inertiaDamping = 8f;
    [Tooltip("Degrees/second for auto spin when idle")] public float autoSpinSpeed = 10f;
    [Tooltip("Mouse wheel zoom sensitivity")] public float mouseWheelStepMultiplier = 5f;
    [Tooltip("Pinch to zoom sensitivity")] public float pinchSensitivity = 1.02f;
}