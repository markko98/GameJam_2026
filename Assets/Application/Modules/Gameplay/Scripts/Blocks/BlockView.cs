using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockView : MonoBehaviour
{
    public PlayerSide Side;
    public BlockType Type;
    
    [SerializeField] private List<BlockStateData> stateDatas = new ();
    private Renderer rend;
    private Sequence _activeSequence;

    public void Initialize(PlayerSide side, BlockType type)
    {
        Side = side;
        Type = type;
        
        foreach (var stateData in stateDatas)
        {
           stateData.SetFadeColor(GetColorForMaskType(stateData.mask));
        }
        ChangeVisuals(MaskType.None);
    }

    public void ChangeVisuals(MaskType maskType)
    {
        _activeSequence?.Kill();
        _activeSequence = DOTween.Sequence();
        foreach (var stateData in stateDatas)
        {
            _activeSequence.Join(stateData.FadeOut());
        }

        var dataToActivate = stateDatas.Find(x => x.mask == maskType);
        _activeSequence.AppendCallback(() =>
        {
            stateDatas.ForEach(x => x.GetCollider().enabled = x.mask == maskType);
        });
        _activeSequence.Append(dataToActivate?.FadeIn());
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
            _ => Color.black
        };
    }
}