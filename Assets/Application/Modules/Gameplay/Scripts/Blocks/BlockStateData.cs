using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class BlockStateData
{
    public BlockState state;
    public GameObject blockVisual;

    private Material _material;
    private Collider _collider;
    private readonly string fadeProperty = "_FadeAmount";
    public readonly string fadeBurnColorProperty = "_FadeBurnColor";
        
    private float fadeDuration = 1f;

    public Collider GetCollider()
    {
        if (blockVisual == null) return null;

        if (_collider == null)
        {
            _collider = blockVisual?.GetComponent<Collider>();
        }
            
        return _collider;
    }

    public Material GetMaterial()
    {
        if (blockVisual == null) return null;
        
        if (_material == null)
        {
            _material = blockVisual?.GetComponent<Renderer>()?.material;
        }
            
        return _material;
    }

    private Tween Fade(float endValue, float duration)
    {
        return GetMaterial()?.DOFloat(endValue, fadeProperty, duration);
    }

    public Tween FadeIn(bool instant = false)
    {
        blockVisual.SetActive(true);
        return Fade(0, instant ? 0 :fadeDuration);
    }
        
    public Tween FadeOut(bool instant = false)
    {
        return Fade(1, instant ? 0 :fadeDuration).OnComplete(() => blockVisual.SetActive(false));
    }

    public void SetFadeColor(Color fadeColor)
    {
        GetMaterial()?.SetColor(fadeBurnColorProperty, fadeColor);
    }
}