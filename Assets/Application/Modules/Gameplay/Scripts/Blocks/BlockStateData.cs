using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class BlockStateData
{
    public MaskType mask;
    public GameObject blockVisual;

    private Material _material;
    private Collider _collider;
    private readonly string fadeProperty = "_FadeAmount";
    public readonly string fadeBurnColorProperty = "_FadeBurnColor";
        
    private float fadeDuration = 1f;

    public Collider GetCollider()
    {
        if (_collider == null)
        {
            _collider = blockVisual.GetComponent<Collider>();
        }
            
        return _collider;
    }

    public Material GetMaterial()
    {
        if (_material == null)
        {
            _material = blockVisual.GetComponent<Renderer>().material;
        }
            
        return _material;
    }

    private Tween Fade(float endValue, float duration)
    {
        return GetMaterial().DOFloat(endValue, fadeProperty, duration);
    }

    public Tween FadeIn()
    {
        return Fade(0, fadeDuration);
    }
        
    public Tween FadeOut()
    {
        return Fade(1, fadeDuration);
    }

    public void SetFadeColor(Color fadeColor)
    {
        GetMaterial().SetColor(fadeBurnColorProperty, fadeColor);
    }
}