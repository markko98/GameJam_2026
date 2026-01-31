using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BlockView : MonoBehaviour
{
    public PlayerSide Side;
    public BlockType Type;
    public MaskType maskType;

    public BlockState state = BlockState.Inactive;

    [SerializeField] private BlockStateData inactiveStateData;
    [SerializeField] private BlockStateData activeStateData;
    private Renderer rend;
    private Sequence _activeSequence;

    public void Initialize(PlayerSide side, BlockType type)
    {
        Side = side;
        Type = type;
        
        inactiveStateData?.SetFadeColor(GetColorForMaskType(maskType));
        activeStateData?.SetFadeColor(GetColorForMaskType(maskType));

        switch (state)
        {
            case BlockState.Inactive:
                activeStateData?.FadeOut(true);
                break;
            case BlockState.Active:
                inactiveStateData?.FadeOut(true);
                break;
        }
    }

    public void ChangeVisuals(MaskType maskType)
    {
        if (this.maskType != maskType) return;

        _activeSequence?.Kill();
        _activeSequence = DOTween.Sequence();
        
        state = state == BlockState.Inactive ? BlockState.Active: BlockState.Inactive;

        if (state == BlockState.Active)
        { 
            _activeSequence.Append(inactiveStateData.FadeOut());
            _activeSequence.Append(activeStateData.FadeIn());
        }
        else
        {
            _activeSequence.Append(activeStateData.FadeOut());
            _activeSequence.Append(inactiveStateData.FadeIn());
        }
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