public interface IModelPreviewTarget
{ 
    void ApplyRotationDelta(float yawDeltaGlobal, float pitchDeltaLocal);
    void ApplyZoomMultiplier(float scale);
    void CustomUpdate();
}