using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockView : MonoBehaviour
{
    [Serializable]
    private class BlockStateData
    {
        public MaskType mask;
        public GameObject blockVisual;

        private Material _material;
        private readonly string fadeProperty = "_FadeAmount";
        public readonly string fadeBurnColorProperty = "_FadeBurnColor";
        
        private float fadeDuration = 1f;

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

        public void ChangeFadeColor(Color fadeColor)
        {
            GetMaterial().SetColor(fadeBurnColorProperty, fadeColor);
        }
    }

    public PlayerSide Side;
    public BlockType Type;
    
    [SerializeField] private List<BlockStateData> stateDatas = new ();
    private Renderer rend;

    public void Initialize(PlayerSide side, BlockType type)
    {
        Side = side;
        Type = type;
        
        foreach (var stateData in stateDatas)
        {
           stateData.ChangeFadeColor(GetColorForMaskType(stateData.mask));
        }
        ChangeVisuals(MaskType.None);
    }

    public void ChangeVisuals(MaskType maskType)
    {
        var sequence = DOTween.Sequence();
        foreach (var stateData in stateDatas)
        {
            sequence.Join(stateData.FadeOut());
        }

        var dataToActivate = stateDatas.Find(x => x.mask == maskType);
        sequence.Append(dataToActivate?.FadeIn());
    }

    private Color GetColorForMaskType(MaskType maskType)
    {
        return maskType switch
        {
            MaskType.None => Color.black,
            MaskType.Trap => Color.blue,
            MaskType.Obstacle => Color.yellow,
            MaskType.Nature => Color.green,
            MaskType.Lava => Color.red,
            _ => Color.white
        };
    }
}