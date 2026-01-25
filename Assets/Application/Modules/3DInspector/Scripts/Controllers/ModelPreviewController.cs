using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class ModelPreviewController : IModelPreviewTarget
{
    // references from setup
    private RawImage previewImage;
    private Camera previewCamera;
    private Transform modelAnchor;
    private int previewLayer;

    // runtime
    private GameObject currentModel;
    private RenderTexture renderTexture;
    private Rect lastPreviewRect;
    private float yawDeg;
    private float pitchDeg;
    private float currentFOV;
    private Quaternion initialModelRotation = Quaternion.identity;

    // config
    private ModelPreviewSettingsSO settings;
    private float baseDistance = 1f;
    private float zoomFactor = 1f;
    private float zoomFactor01;
    private float baseOrthoSize = 1f;
    private readonly float padding = 1.3f;
    private readonly int antiAliasing = 2;

    public void Setup(ModelPreviewSettingsSO settings, RawImage image, Camera camera, Transform anchor, int layerIndex)
    {
        this.settings = settings;
        previewImage = image;
        previewCamera = camera;
        modelAnchor = anchor;
        previewLayer = layerIndex;

        ApplyProjectionMode(settings.useOrthographicCamera);
        EnsureRTMatchesImage();
    }

    public void SetModel(GameObject modelPrefab)
    {
        if (currentModel) Object.Destroy(currentModel);
        if (!modelPrefab || !modelAnchor) return;

        currentModel = Object.Instantiate(modelPrefab, modelAnchor, false);
        SetLayerRecursively(currentModel, previewLayer);

        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;

        Utils.CenterModelByBounds(currentModel.transform);

        initialModelRotation = modelAnchor.transform.rotation;
        yawDeg = 0f;
        pitchDeg = Mathf.Clamp(0f, settings.pitchClamp.x, settings.pitchClamp.y);

        FitCameraToModel();
        zoomFactor = 1f;
        zoomFactor01 = Mathf.InverseLerp(settings.minZoom, settings.maxZoom, zoomFactor);

        ApplyModelRotation();
    }

    public void ApplyRotationDelta(float yawDeltaGlobal, float pitchDeltaLocal)
    {
        yawDeg += yawDeltaGlobal;
        pitchDeg = Mathf.Clamp(pitchDeg + pitchDeltaLocal, settings.pitchClamp.x, settings.pitchClamp.y);

        ApplyModelRotation();
    }

    public void ApplyZoomMultiplier(float scale)
    {
        zoomFactor = Mathf.Clamp(zoomFactor * scale, settings.minZoom, settings.maxZoom);
        zoomFactor01 = Mathf.InverseLerp(settings.minZoom, settings.maxZoom, zoomFactor);
    }

    public void CustomUpdate()
    {
        if (previewImage && previewImage.rectTransform.rect != lastPreviewRect)
            EnsureRTMatchesImage();

        if (!previewCamera || !modelAnchor) return;

        ApplyProjectionMode(settings.useOrthographicCamera);
        UpdateCamera();
    }

    public void Dispose()
    {
        if (renderTexture != null)
        {
            if (previewCamera) previewCamera.targetTexture = null;
            renderTexture.Release();
            Object.Destroy(renderTexture);
            renderTexture = null;
        }

        if (currentModel) Object.Destroy(currentModel);
        currentModel = null;
    }

    // internal methods
    private void UpdateCamera()
    {
        var lookAt = modelAnchor.position;
        var camForward = previewCamera.transform.forward;

        if (settings.useOrthographicCamera)
        {
            var minSize = Mathf.Max(0.01f, settings.orthographicSize.x);
            var maxSize = Mathf.Max(minSize, settings.orthographicSize.y);
            var targetSize = Mathf.Lerp(minSize, maxSize, zoomFactor01);
            previewCamera.orthographicSize = targetSize * baseOrthoSize;
            previewCamera.transform.position = lookAt - camForward * Mathf.Max(0.1f, baseDistance * 0.5f);
        }
        else
        {
            SetupCameraDistanceInPerspective();
        }

        previewCamera.transform.LookAt(lookAt, Vector3.up);
    }

    private void SetupCameraDistanceInPerspective()
    {
        var lookAt = modelAnchor.position;
        var camForward = previewCamera.transform.forward;

        var dist = baseDistance * Mathf.Max(0.1f, settings.perspectiveDistanceMultiplier);

        switch (settings.perspectiveZoomMode)
        {
            case PerspectiveZoomMode.Distance:
                dist *= zoomFactor;
                break;

            case PerspectiveZoomMode.FOV:
                break;

            case PerspectiveZoomMode.Hybrid:
                dist *= Mathf.Lerp(1.0f, 1.8f, zoomFactor01);
                break;
            default:
                dist *= 1f;
                break;
        }

        var minFov = Mathf.Clamp(settings.telephotoMinFOV, 1f, 90f);
        var maxFov = Mathf.Clamp(settings.telephotoMaxFOV, 1f, 90f);
        if (maxFov < minFov) (minFov, maxFov) = (maxFov, minFov);

        if (settings.useTelephotoPerspective)
        {
            float fov;
            switch (settings.perspectiveZoomMode)
            {
                case PerspectiveZoomMode.Distance:
                    fov = maxFov;
                    break;

                case PerspectiveZoomMode.FOV:
                case PerspectiveZoomMode.Hybrid:
                    fov = Mathf.Lerp(maxFov, minFov, zoomFactor01);
                    break;

                default:
                    fov = previewCamera.fieldOfView;
                    break;
            }

            currentFOV = fov;
            previewCamera.fieldOfView = fov;
        }

        previewCamera.transform.position = lookAt - camForward * dist;
    }

    private void ApplyModelRotation()
    {
        if (!currentModel) return;
        var yaw = Quaternion.AngleAxis(yawDeg, Vector3.up);
        var pitch = Quaternion.AngleAxis(pitchDeg, Vector3.right);
        modelAnchor.transform.rotation = yaw * (modelAnchor.rotation * pitch) *
                                         Quaternion.Inverse(modelAnchor.rotation) * initialModelRotation;
    }

    private void FitCameraToModel()
    {
        if (!previewCamera || !currentModel) return;

        // encapsulate model to fit camera
        var meshRens = currentModel.GetComponentsInChildren<Renderer>()
            .Where(r => r is MeshRenderer || r is SkinnedMeshRenderer)
            .ToArray();

        Bounds b;
        if (meshRens.Length == 0)
            b = new Bounds(currentModel.transform.position, Vector3.one);
        else
        {
            b = new Bounds(meshRens[0].bounds.center, Vector3.zero);
            foreach (var r in meshRens) b.Encapsulate(r.bounds);
        }

        modelAnchor.position = b.center;

        SetupCamera(b);
    }

    private void SetupCamera(Bounds bounds)
    {
        if (!previewCamera) return;

        var aspect = previewCamera.aspect;
        if (previewCamera.targetTexture)
            aspect = Mathf.Max(0.0001f, (float)previewCamera.targetTexture.width / previewCamera.targetTexture.height);
        else if (previewImage)
        {
            var r = previewImage.rectTransform.rect;
            if (r.height > 0) aspect = Mathf.Max(0.0001f, r.width / r.height);
        }

        if (!float.IsFinite(aspect) || aspect <= 0f) aspect = 1f;

        var radius = bounds.extents.magnitude;
        var center = bounds.center;
        var lookDir = previewCamera.transform.rotation * Vector3.forward;

        if (!settings.useOrthographicCamera)
        {
            var fitFov = previewCamera.fieldOfView;
            if (settings.useTelephotoPerspective)
            {
                var minFov = Mathf.Clamp(settings.telephotoMinFOV, 1f, 90f);
                var maxFov = Mathf.Clamp(settings.telephotoMaxFOV, 1f, 90f);
                if (maxFov < minFov) (minFov, maxFov) = (maxFov, minFov);
                fitFov = maxFov;
            }

            var halfFov = 0.5f * fitFov * Mathf.Deg2Rad;
            var distanceVertical = bounds.extents.y / Mathf.Tan(halfFov);
            var distanceHorizontal = bounds.extents.x / (Mathf.Tan(halfFov) * aspect);
            baseDistance = Mathf.Max(distanceVertical, distanceHorizontal, radius * 1.1f) * padding;

            baseDistance *= Mathf.Max(0.1f, settings.perspectiveDistanceMultiplier);

            var pos = center - lookDir * baseDistance;
            if (!float.IsFinite(pos.x))
            {
                lookDir = Vector3.forward;
                pos = center - lookDir * baseDistance;
            }

            previewCamera.transform.position = pos;
            previewCamera.transform.LookAt(center, Vector3.up);

            previewCamera.nearClipPlane = Mathf.Max(0.01f, baseDistance - radius * 3f);
            previewCamera.farClipPlane = Mathf.Max(previewCamera.nearClipPlane + 1f, baseDistance + radius * 6f);
        }
        else
        {
            var boundsVertical = bounds.extents.y;
            var boundsHorizontal = bounds.extents.x / aspect;
            baseOrthoSize = Mathf.Max(boundsVertical, boundsHorizontal) * padding;

            // place camera at a good enough distance  to prevent clipping
            baseDistance = Mathf.Max(1f, radius * 2f);

            var pos = center - lookDir * baseDistance;
            if (!float.IsFinite(pos.x))
            {
                lookDir = Vector3.forward;
                pos = center - lookDir * baseDistance;
            }

            previewCamera.transform.position = pos;
            previewCamera.transform.LookAt(center, Vector3.up);

            previewCamera.orthographicSize = baseOrthoSize;

            previewCamera.nearClipPlane = Mathf.Max(0.01f, baseDistance - radius * 3f);
            previewCamera.farClipPlane = Mathf.Max(previewCamera.nearClipPlane + 1f, baseDistance + radius * 6f);
        }
    }

    private void EnsureRTMatchesImage()
    {
        if (!previewImage) return;

        var rect = previewImage.rectTransform.rect;
        var width = Mathf.Max(64, Mathf.RoundToInt(rect.width));
        var height = Mathf.Max(64, Mathf.RoundToInt(rect.height));
        lastPreviewRect = rect;

        if (renderTexture == null || renderTexture.width != width || renderTexture.height != height)
        {
            if (renderTexture != null)
            {
                previewCamera.targetTexture = null;
                renderTexture.Release();
                Object.Destroy(renderTexture);
            }

            renderTexture = PreviewRenderTextureFactory.CreateAndBind(previewCamera, previewImage,
                width,
                height);

            if (previewCamera && renderTexture)
            {
                previewCamera.ResetAspect();
                previewCamera.aspect = (float)renderTexture.width / renderTexture.height;
            }
        }
    }

    private void ApplyProjectionMode(bool useOrtho)
    {
        if (!previewCamera) return;
        if (previewCamera.orthographic != useOrtho)
        {
            previewCamera.orthographic = useOrtho;
            if (currentModel) FitCameraToModel();
        }
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform c in go.transform)
            SetLayerRecursively(c.gameObject, layer);
    }
}