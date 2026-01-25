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
    [SerializeField] private bool applyGrayscalePreset = false;
    [SerializeField] private UIGradient gradientColor;

    private void OnValidate()
    {
        Setup(gradientColor);
    }

    public void Setup(UIGradient gradientColor)
    {
        this.gradientColor = gradientColor;
        UpdateView(this.gradientColor);
    }

    public void UpdateView(UIGradient gradient)
    {
        buttonEffect.gradationMode = GradationMode.DiagonalToRightBottom;
        buttonEffect.gradationColor1 = gradient.color1;
        buttonEffect.gradationColor2 = gradient.color2;
    }

    public void Toggle(bool active)
    {
        button.interactable = active;
        if (applyGrayscalePreset)
        {
            var preset = active ? UIEffectProvider.GetPresetForButton(true) : UIEffectProvider.GetPresetForButton(false);
            buttonEffect.LoadPreset(preset);
            UpdateView(gradientColor);
        }
        else
        {
            var targetGradient = active ? gradientColor : ColorProvider.GetGrayscaleGradientForButton(); 
            UpdateView(targetGradient);
        }
    }
}