using Coffee.UIEffects;
using UnityEngine;

[CreateAssetMenu(menuName = "HyperPlanes/UIEffect Catalog", fileName = "UIEffectCatalogue")]
public class UIEffectCatalogue : ScriptableObject
{
    [Header("Buttons")] 
    public UIEffectPreset buttonActive;
    public UIEffectPreset buttonInactive;
    public UIEffectPreset buttonPressed;
    public UIEffectPreset grayscale;
    public UIEffectPreset normal;
}