using System;
using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    [SerializeField] private bool startOnAwake = true;
    
    [Header("Spin")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 180f; // degrees per second

    private Renderer[] renderers;
    private bool canSpin;

    private void Awake()
    {
        if (startOnAwake)
        {
            canSpin = true;
        }
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();
    }
    private void OnEnable()
    {
        GameTicker.SharedInstance.Update += CustomUpdate;
    }

    private void OnDisable()
    {
        GameTicker.SharedInstance.Update -= CustomUpdate;
    }

    private void CustomUpdate()
    {
        if (canSpin == false) return;
        if (!IsInView()) return;

        float delta = GameTicker.DeltaTime;
        transform.Rotate(rotationAxis.normalized, rotationSpeed * delta, Space.Self);
    }
    private bool IsInView()
    {
        foreach (var r in renderers)
        {
            if (CameraVisibilityUtility.IsVisibleFrom(r, PersistentReferences.Instance.GetMainCamera()))
                return true;
        }
        return false;
    }
}