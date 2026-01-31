using System;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonView : UIViewComponent
{
    public GameObject root;
    public TextMeshProUGUI text;
    public Button button;
    public UIEffect buttonEffect;

    private void OnValidate()
    {
        Setup();
    }

    public void Setup()
    {
    }

    public void UpdateView()
    {
    }

    public void Toggle(bool active)
    {
        button.interactable = active;
        var preset = active ? UIEffectProvider.GetPresetForButton(true) : UIEffectProvider.GetPresetForButton(false);
        buttonEffect.LoadPreset(preset);
        UpdateView();
    }
}