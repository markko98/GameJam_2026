using System;
using UnityEngine;


[CreateAssetMenu(menuName = "HyperPlanes/Color Catalog", fileName = "ColorCatalogue")]
public class ColorCatalogue : ScriptableObject
{
    [Header("Text")]
    public Color buttonTextColor;
    public Color highlightTextColor;
    
    [Header("Cost")]
    public Color cantAffordCostTextColor;
    public Color defaultCostTextColor;
    
    [Header("UI Views")]
    public Color tabActiveColor;
    public Color tabInactiveColor;
    public Color tabDisabledColor;
    public UIGradient grayscaledButtonColors;
}

[Serializable]
public struct UIGradient
{
    public Color color1;
    public Color color2;
}