using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TutorialAnchor : MonoBehaviour
{
    [SerializeField] private string anchorId;
    [SerializeField] private Button anchorButton;
    [SerializeField] private bool registerOnAwake = true;
    public UnityEvent onAction = new();

    public string AnchorId => anchorId;
    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = transform as RectTransform;
        
        var btn = anchorButton ?? GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => onAction?.Invoke());
        }

        if (registerOnAwake)
            Register();
    }
    
    public void RegisterWithId(string anchorId)
    {
        this.anchorId = anchorId;
        Register();
    }

    private void OnDestroy()
    {
        Unregister();
    }
    public Button GetButton()
    {
        return anchorButton;
    }
    public void Register()
    {
        TutorialAnchorRegistry.Register(this);
    }
    
    public void Unregister()
    {
        TutorialAnchorRegistry.Unregister(this);
    }
}