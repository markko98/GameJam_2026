using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class ModelPreviewPresenter : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private ModelPreviewSetOutlet previewSet;

    [Header("References")]
    [SerializeField] private RawImage rawImageTarget;
    [SerializeField, Tooltip("Null if ScreenSpace-Overlay - Can be left empty")] private Camera uiCamera;

    [FormerlySerializedAs("inputSettings")]
    [Header("Config")]
    [SerializeField] private ModelPreviewSettingsSO settings;
    [SerializeField] private LayerMask previewLayer;
    [SerializeField] private int rtWidth = 1024;
    [SerializeField] private int rtHeight = 768;
    [SerializeField] private int rtDepth = 24;
    [SerializeField] private RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;
    [SerializeField] private int rtAA = 2;

    // runtime
    private Camera previewCamera;
    private Transform modelAnchor;
    private RenderTexture rt;
    private GameObject preview3D;
    private ModelPreviewSetOutlet previewSetInstance;

    // runtime logic
    private ModelPreviewController previewController;
    private ModelPreviewInteraction interactionController;

    public void Setup()
    {
        preview3D = Instantiate(previewSet.gameObject);
        previewSetInstance = preview3D.GetComponent<ModelPreviewSetOutlet>();
        previewCamera = previewSetInstance.camera;
        modelAnchor = previewSetInstance.modelAnchor;

        SetupCamera();
        SetupControllers();
        GameTicker.SharedInstance.Update += CustomUpdate;
    }
    
    public void ShowModel(GameObject modelPrefab)
    {
        previewController.SetModel(modelPrefab);
    }

    private void CustomUpdate()
    {
        previewController.CustomUpdate();
    }

    private void SetupCamera()
    {
        // transparent BG over UI
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = new Color(0, 0, 0, 0);
        previewCamera.cullingMask = previewLayer;
    }

    private void SetupControllers()
    {
        previewController = new ModelPreviewController();
        previewController.Setup(settings, rawImageTarget, previewCamera, modelAnchor, Utils.LayerMaskToIndex(previewLayer));

        interactionController = new ModelPreviewInteraction(previewController, settings, rawImageTarget.rectTransform, uiCamera);
    }
    
    public void Cleanup()
    {
        GameTicker.SharedInstance.Update -= CustomUpdate;
        interactionController?.Dispose();
        previewController?.Dispose();

        previewSetInstance = null;
        if (preview3D)
        {
            Destroy(preview3D);
        }
    }
}
